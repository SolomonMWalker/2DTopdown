using Godot;
using System.Linq;
using FirstPerson.CustomTypes.StateMachine;

public partial class PlayerController : CharacterBody2D
{
    [Export] public float MaxCursorSelectDistance = 200f; // cursor must be this close to a target to lock
    [Export] public float MaxLockDistance = 1500f;        // lock breaks once player/target drift past this

    // Vignette overlay the crouch state toggles. Cached here so states go through the controller
    // instead of hardcoding scene paths.
    public CanvasItem Vignette { get; private set; }

    // Movement states set this: normal states aim the body at the mouse; Sprinting turns it off and
    // aims at the movement direction itself.
    public bool AimAtMouse = true;

    // Current lock-on target, or null. Toggled with the middle mouse button.
    public LockOnComponent LockOnTarget { get; private set; }

    private StateMachine _stateMachine;
    private Label _stateLabel;
    private Camera2D _camera;
    private LockOnReticle _reticle;

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
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Middle })
            ToggleLockOn();
    }

    public override void _Process(double delta)
    {
        DropStaleLock();

        // Walk/crouch aim: at the lock-on target if locked, else at the mouse. Sprinting sets
        // AimAtMouse=false and drives its own rotation, but the lock stays active.
        if (AimAtMouse)
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
