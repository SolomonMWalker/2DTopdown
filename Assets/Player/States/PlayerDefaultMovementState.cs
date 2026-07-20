using Godot;

namespace DTopdown.Assets.Player.States;

public partial class PlayerDefaultMovementState : PlayerBaseMovementState
{
    protected override void Move(Vector2 inputVector)
    {
        _player.Velocity = inputVector * _player.DefaultSpeed;
        _player.MoveAndSlide();
    }
}