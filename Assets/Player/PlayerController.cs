using Godot;
using System.Linq;
using FirstPerson.CustomTypes.StateMachine;

public partial class PlayerController : CharacterBody2D
{
    [Export] public float MaxCursorSelectDistance = 200f; // cursor must be this close to a target to lock
    [Export] public float MaxLockDistance = 1500f;        // lock breaks once player/target drift past this
    [Export] public PackedScene ProjectileScene;          // projectile fired by the Q ranged attack
    [Export] public float LungeSpeed = 260f;              // forward nudge applied at each swing's start
    [Export] public float LungeDeceleration = 900f;       // how fast that nudge bleeds back to zero

    // Vignette overlay the crouch state toggles. Cached here so states go through the controller
    // instead of hardcoding scene paths.
    public CanvasItem Vignette { get; private set; }

    // Movement states set this: normal states aim the body at the mouse; Sprinting turns it off and
    // aims at the movement direction itself.
    public bool AimAtMouse = true;

    // Set for the whole combo. Movement states read it to lock WASD (applying AttackVelocity instead).
    public bool IsAttacking { get; private set; }

    // Set once each swing's windup ends (SwingStart). While true, nothing re-aims the body - the
    // player is committed to the direction they chose during the windup.
    public bool RotationLocked { get; private set; }

    // The decaying forward lunge applied during a swing. Movement states drive the body with this
    // while IsAttacking; _PhysicsProcess bleeds it back to zero.
    public Vector2 AttackVelocity { get; private set; }

    // The 3-hit combo, in order: slash R->L, slash L->R, finishing stab.
    private static readonly string[] ComboAnims = { "attack1", "attack2", "attack3" };
    private int _comboIndex;
    // Opened by SwingEnd (after a hit connects), closed when the animation ends or we chain. A live
    // attack press while it's open advances the combo; presses at any other time are ignored (no
    // queuing), so the player is never locked into a swing they mashed for earlier.
    private bool _comboWindowOpen;

    // Current lock-on target, or null. Toggled with the middle mouse button.
    public LockOnComponent LockOnTarget { get; private set; }

    private StateMachine _stateMachine;
    private Label _stateLabel;
    private Camera2D _camera;
    private LockOnReticle _reticle;
    private AnimationPlayer _animationPlayer;
    private WeaponHitbox _weaponHitbox;
    private Node2D _muzzle;

    // _EnterTree, not _Ready: this is the scene root, so its _Ready runs AFTER its children's -
    // including StateMachine._Ready, which enables the start state and touches Vignette.
    // _EnterTree runs before any child _Ready, so the ref is set in time.
    public override void _EnterTree()
    {
        Vignette = GetNode<CanvasItem>("Vignette/VignetteRect");
    }

    public override void _Ready()
    {
        _stateMachine = GetNode<StateMachine>("StateMachine");
        _stateLabel = GetNode<Label>("Hud/StateLabel");
        _camera = GetNode<Camera2D>("Camera2D");
        _reticle = GetNode<LockOnReticle>("LockOnReticle");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _weaponHitbox = GetNode<WeaponHitbox>("Sprites/RightHand/LightWeapon/WeaponHitbox");
        _muzzle = GetNode<Node2D>("Sprites/LeftHand/RangedWeapon");
        _animationPlayer.AnimationFinished += OnAnimationFinished;

        // Cross-fade combo hops so chaining eases between poses instead of hard-snapping the sword.
        _animationPlayer.SetBlendTime("attack1", "attack2", 0.08);
        _animationPlayer.SetBlendTime("attack2", "attack3", 0.08);
    }

    // A combo animation reaching its end means no chain press arrived in the window -> combo over.
    // (Chaining swaps the animation via Play(), which doesn't emit this.) "shoot" ends here too, but
    // IsAttacking is false then, so it's a no-op.
    private void OnAnimationFinished(StringName _)
    {
        if (IsAttacking) EndAttack();
    }

    public override void _PhysicsProcess(double delta)
    {
        AttackVelocity = AttackVelocity.MoveToward(Vector2.Zero, LungeDeceleration * (float)delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Middle })
            ToggleLockOn();
        else if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            Attack();
        else if (@event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Q })
            Shoot();
    }

    private void Shoot()
    {
        if (IsAttacking || _animationPlayer.IsPlaying()) return; // one action at a time
        _animationPlayer.Play("shoot"); // FireProjectile is invoked from a method track at the hand's apex
    }

    // Called by the "shoot" animation's method track when the left hand reaches its furthest point.
    public void FireProjectile()
    {
        if (ProjectileScene is null) return;
        var projectile = ProjectileScene.Instantiate<Projectile>();
        GetParent().AddChild(projectile); // live in the world, not parented to (and dragged by) the player
        projectile.GlobalPosition = _muzzle.GlobalPosition;

        // Aim at the locked-on target if any, else at the mouse - independent of which way the body faces.
        Vector2 aim = LockOnTarget is not null ? LockOnTarget.GlobalPosition : GetGlobalMousePosition();
        projectile.GlobalRotation = (aim - projectile.GlobalPosition).Angle();
        projectile.Launch(this);
    }

    private void Attack()
    {
        if (IsAttacking)
        {
            // Mid-combo: a press only counts inside the window, and only if there's a next hit.
            if (_comboWindowOpen && _comboIndex < ComboAnims.Length - 1)
            {
                _comboIndex++;
                PlayCurrentAttack();
            }
            return;
        }

        if (_animationPlayer.IsPlaying()) return; // don't interrupt a shoot
        IsAttacking = true;
        _comboIndex = 0;
        PlayCurrentAttack();
    }

    private void PlayCurrentAttack()
    {
        _comboWindowOpen = false;
        RotationLocked = false;     // free to re-aim during this hit's windup; SwingStart re-locks it
        _weaponHitbox.Deactivate(); // safety: SwingStart turns it back on for the active frames
        _animationPlayer.Play(ComboAnims[_comboIndex]);
    }

    // Called from each attack animation's method track when the windup ends and the swing begins:
    // commit the aim, arm the hitbox, and nudge the body forward.
    public void SwingStart()
    {
        RotationLocked = true;
        _weaponHitbox.Activate();
        AttackVelocity = Vector2.Right.Rotated(GlobalRotation) * LungeSpeed;
    }

    // Called when the swing's active frames end: disarm the hitbox and open the chain window.
    public void SwingEnd()
    {
        _weaponHitbox.Deactivate();
        _comboWindowOpen = true;
    }

    private void EndAttack()
    {
        IsAttacking = false;
        RotationLocked = false;
        _comboWindowOpen = false;
        AttackVelocity = Vector2.Zero;
        _weaponHitbox.Deactivate();
    }

    public override void _Process(double delta)
    {
        DropStaleLock();

        // Walk/crouch aim: at the lock-on target if locked, else at the mouse. Sprinting sets
        // AimAtMouse=false and drives its own rotation, but the lock stays active. RotationLocked
        // freezes aim once a swing commits.
        if (AimAtMouse && !RotationLocked)
        {
            Vector2 aim = LockOnTarget is not null ? LockOnTarget.GlobalPosition : GetGlobalMousePosition();
            Vector2 to = aim - GlobalPosition;
            if (to.LengthSquared() > 0.0001f)
                Rotation = to.Angle();
        }

        // Camera rides the player, or the midpoint to the target when locked. Camera2D is top_level
        // so we drive its global position directly; its own smoothing eases the move.
        _camera.GlobalPosition = LockOnTarget is not null
            ? (GlobalPosition + LockOnTarget.GlobalPosition) * 0.5f
            : GlobalPosition;

        // Reticle sits on the target while locked.
        _reticle.Visible = LockOnTarget is not null;
        if (LockOnTarget is not null)
            _reticle.GlobalPosition = LockOnTarget.GlobalPosition;

        _stateLabel.Text = _stateMachine.GetStateMachineString();
    }

    private void ToggleLockOn()
    {
        if (LockOnTarget is not null) { LockOnTarget = null; return; }

        Vector2 mouse = GetGlobalMousePosition();
        LockOnTarget = LockOnComponent.All
            .Where(c => c.GlobalPosition.DistanceTo(mouse) <= MaxCursorSelectDistance)
            .OrderBy(c => c.GlobalPosition.DistanceTo(mouse))
            .FirstOrDefault(); // null if nothing near the cursor -> stays unlocked
    }

    // Drop the lock if the target was freed or drifted too far.
    private void DropStaleLock()
    {
        if (LockOnTarget is null) return;
        if (!IsInstanceValid(LockOnTarget) ||
            GlobalPosition.DistanceTo(LockOnTarget.GlobalPosition) > MaxLockDistance)
            LockOnTarget = null;
    }
}
