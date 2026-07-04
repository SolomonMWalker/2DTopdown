using System;
using System.Collections.Generic;
using Godot;

namespace FirstPerson.CustomTypes.StateMachine;

[GlobalClass]
public partial class State : Node
{
    public event EventHandler<ChangeStateEventArgs> StateChangeRequired;
    protected void OnStateChangeRequired(ChangeStateEventArgs e)
    {
        var handler = StateChangeRequired;
        handler?.Invoke(this, e);
    }

    public virtual List<State> GetAllStates()
    {
        return [this];
    }

    // Declarative outgoing transitions, evaluated by the StateMachine while this state
    // is active. Optional: states can still drive transitions imperatively via
    // OnStateChangeRequired. Transitions are evaluated in insertion order; first match wins.
    public List<Transition> Transitions { get; } = [];

    public Transition AddTransition(string toStateName, Func<bool> guard = null, Action onTransition = null)
    {
        var transition = new Transition(toStateName, guard, onTransition);
        Transitions.Add(transition);
        return transition;
    }

    // The first transition whose guard currently passes, or null if none are eligible.
    public Transition GetEligibleTransition()
    {
        foreach (var transition in Transitions)
        {
            if (transition.GuardPasses()) return transition;
        }

        return null;
    }
    
    private bool _enabled { get; set; }
    public bool Enabled => _enabled;

    public virtual void Enable()
    {
        _enabled = true;
    }

    public virtual void Disable()
    {
        _enabled = false;
    }

    public virtual void StateEntered()
    {
        Enable();
    }

    public virtual void StateExited()
    {
        Disable();
    }
    public virtual void StatePhysicsProcessing(double delta) {}
    public virtual void StateProcessing(double delta) {}

    public virtual string GetFullStateString() => "";
}