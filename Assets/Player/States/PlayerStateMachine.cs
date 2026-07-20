using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace DTopdown.Assets.Player.States;

public partial class PlayerStateMachine : StateMachine
{
    [Export] public Player Player { get; set; }
}