using Godot;

namespace DTopdown.Assets.Player.States;

//Default rotation rotates to mouse position
public partial class PlayerDefaultRotationState : PlayerBaseRotationState
{
    public override void _Ready()
    {
        base._Ready();
        AddTransition("LockedOnRotationState", () => _player.HasTarget());
    }
    
    public override Vector2 GetTargetGlobalPosition()
    {
        return _player.GetGlobalMousePosition();
    }
}