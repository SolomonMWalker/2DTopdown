using System;

namespace FirstPerson.CustomTypes.StateMachine;

// A declarative transition: where to go, an optional guard that decides when it's
// eligible, and an optional effect that fires as the edge is taken.
public class Transition
{
    public string ToStateName { get; }

    private readonly Func<bool> _guard;
    private readonly Action _onTransition;

    public Transition(string toStateName, Func<bool> guard = null, Action onTransition = null)
    {
        ToStateName = toStateName;
        _guard = guard;
        _onTransition = onTransition;
    }

    // True when this transition is eligible to be taken. A null guard is always eligible.
    // Guards must be side-effect free: they are polled every frame while the source is active.
    public bool GuardPasses() => _guard is null || _guard();

    // Effect fired as the transition is taken, before the source exits / target enters.
    // Override for class-based transitions, or pass an action to the constructor.
    public virtual void OnTransition() => _onTransition?.Invoke();
}
