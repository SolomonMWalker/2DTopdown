using Godot;
using FirstPerson.CustomTypes.StateMachine;

// Shared base for atomic movement states: while active, reads WASD and moves the owning
// PlayerController at Speed. WalkingState / CrouchingState subclass this; each sets its Speed
// and is the home for any behaviour unique to that movement.
public partial class PlayerMovementLeafState : AtomicState
{
    [Export] public float Speed = 200f;

    protected PlayerController _player;

    public override void _Ready()
    {
        base._Ready();
        _player = (PlayerController)GetOwner();
    }

    public override void StatePhysicsProcessing(double delta)
    {
        // Movement locked while attacking: no WASD, just the decaying forward lunge.
        if (_player.IsAttacking)
        {
            _player.Velocity = _player.AttackVelocity;
            _player.MoveAndSlide();
            return;
        }

        Vector2 direction = Vector2.Zero;
        if (Input.IsActionPressed("forward")) direction.Y -= 1f;
        if (Input.IsActionPressed("backward")) direction.Y += 1f;
        if (Input.IsActionPressed("left")) direction.X -= 1f;
        if (Input.IsActionPressed("right")) direction.X += 1f;

        _player.Velocity = direction.Normalized() * Speed;
        _player.MoveAndSlide();
    }
}
