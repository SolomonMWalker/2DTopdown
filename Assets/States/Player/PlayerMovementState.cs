using Godot;
using FirstPerson.CustomTypes.StateMachine;

// The movement group: holds the atomic movement states (Walking, Crouching, ...) and owns the
// crouch toggle. Pressing "crouch" swaps between the crouch state and the default (walk) state.
[GlobalClass]
public partial class PlayerMovementState : CompoundState
{
    [Export] public string CrouchStateName = "Crouching";

    public override void StateProcessing(double delta)
    {
        if (!Input.IsActionJustPressed("crouch")) return;

        // Imperative (not a declarative Transition) on purpose: the machine re-polls guards after
        // each transition in the same frame, so an edge-triggered guard would ping-pong. Raising
        // the event once from this compound's _Process avoids that.
        // ponytail: un-crouch always returns to the default state - fine while walk/crouch are the
        // only members; when more movement states arrive, track the pre-crouch state instead.
        var target = ActiveState.Name.ToString() == CrouchStateName ? DefaultStateName : CrouchStateName;
        OnStateChangeRequired(new ChangeStateEventArgs(target));
    }
}
