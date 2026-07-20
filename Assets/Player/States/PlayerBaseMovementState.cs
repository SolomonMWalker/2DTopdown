using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace DTopdown.Assets.Player.States;

public abstract partial class PlayerBaseMovementState : AtomicState
{
    [Export] public PlayerStateMachine PlayerStateMachine { get; set; }

    protected Player _player;

    public override void _Ready()
    {
        base._Ready();
        _player = PlayerStateMachine.Player;
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);

        var movementVector =
            Input.GetVector("left", "right", "up", "down");

        Move(movementVector);
    }

    protected abstract void Move(Vector2 inputVector);
}