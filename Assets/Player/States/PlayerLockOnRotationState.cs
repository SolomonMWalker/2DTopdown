using Godot;

namespace DTopdown.Assets.Player.States;

public partial class PlayerLockOnRotationState : PlayerBaseRotationState
{
    public override void _Ready()
    {
        base._Ready();
        AddTransition("DefaultRotationState", () => !_player.HasTarget());
    }

    // Guards are polled in _Process but this runs in _PhysicsProcess, so there's a window
    // where the target is already gone and the transition to DefaultRotationState hasn't
    // resolved yet. Fall back to the mouse -- the same thing that state is about to do.
    public override Vector2 GetTargetGlobalPosition() =>
        _player.HasTarget() ? _player.Target.GlobalPosition : _player.GetGlobalMousePosition();
}