using Godot;

// Fast movement, held with the "sprint" action. Unlike walk/crouch it doesn't aim at the mouse -
// the body faces the direction it's moving. Enter from Walking or Crouching; release to return.
[GlobalClass]
public partial class SprintingState : PlayerMovementLeafState
{
    public override void _Ready()
    {
        base._Ready();
        // Declarative transition: leave sprint the moment the key is released.
        // ponytail: always returns to Walking, not the pre-sprint state; track it if crouch->sprint
        // ->crouch matters.
        AddTransition("Walking", () => !Input.IsActionPressed("sprint"));
    }

    public override void Enable()
    {
        base.Enable();
        _player.AimAtMouse = false;
    }

    public override void Disable()
    {
        base.Disable();
        _player.AimAtMouse = true;
    }

    // Higher = snappier turn. Frame-rate independent via 1 - exp(-rate*dt).
    private const float TurnRate = 18f;

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta); // sets Velocity + moves
        if (_player.Velocity.LengthSquared() > 0.0001f)
        {
            float weight = 1f - Mathf.Exp(-TurnRate * (float)delta);
            _player.Rotation = Mathf.LerpAngle(_player.Rotation, _player.Velocity.Angle(), weight);
        }
    }
}
