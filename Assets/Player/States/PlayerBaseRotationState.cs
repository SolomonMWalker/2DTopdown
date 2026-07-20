using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace DTopdown.Assets.Player.States;

public abstract partial class PlayerBaseRotationState : AtomicState
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
        _player.GlobalRotation = (GetTargetGlobalPosition() - _player.GlobalPosition).Angle();
    }

    public abstract Vector2 GetTargetGlobalPosition();
}