using Godot;

// Standard on-foot movement. Home for any walk-specific behaviour. Speed is set per-node in the
// inspector (don't assign it in the constructor - that races the scene-restored value).
[GlobalClass]
public partial class WalkingState : PlayerMovementLeafState
{
    public override void _Ready()
    {
        base._Ready();
        AddTransition("Sprinting", () => Input.IsActionPressed("sprint"));
    }
}
