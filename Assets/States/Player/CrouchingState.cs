using Godot;

// Slower, crouched movement. Home for any crouch-specific behaviour; sets its own default Speed.
// Owns the screen-edge vignette: on while crouched, off otherwise.
[GlobalClass]
public partial class CrouchingState : PlayerMovementLeafState
{
    public override void _Ready()
    {
        base._Ready();
        AddTransition("Sprinting", () => Input.IsActionPressed("sprint"));
    }

    public override void Enable()
    {
        base.Enable();
        _player.Vignette.Visible = true;
    }

    public override void Disable()
    {
        base.Disable();
        _player.Vignette.Visible = false;
    }
}
