using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FirstPerson.CustomTypes.StateMachine;

public class ChangeStateEventArgs(string stateName) : EventArgs
{
    public string StateName { get; set; } = stateName;
}

[GlobalClass]
public partial class StateMachine: Node
{
    [Export] public State RootState { get; set; }
    public List<State> States { get; set; }
    public Dictionary<string, PathToAtomicState> PathsToAtomicStatesDict { get; set; } = [];

    // Safety cap: if more than this many transitions resolve in a single frame we assume a
    // transition loop (e.g. two states that keep targeting each other) and bail out.
    private const int MaxTransitionsPerFrame = 64;

    private List<AtomicState> _atomicStates = [];
    private readonly Queue<PendingTransition> _pendingTransitions = new();

    private readonly struct PendingTransition(string toStateName, Transition transition = null)
    {
        public string ToStateName { get; } = toStateName;
        public Transition Transition { get; } = transition;
    }

    public override void _Ready()
    {
        base._Ready();
        States = GetAllStates(RootState);
        if (RootState is null)
        {
            throw new Exception("No Root state present as a child of state machine");
        }
        RootState.Enable();
        AddChangeStateDelegatesToEventHandler();
        BuildPaths();
    }

    private List<State> GetAllStates(State rootState)
    {
        return rootState.GetAllStates();
    }

    public void BuildPaths()
    {
        _atomicStates = States.OfType<AtomicState>().ToList();
        foreach (var aState in _atomicStates)
        {
            PathsToAtomicStatesDict.Add(aState.Name, new PathToAtomicState(aState));
        }
    }

    public void AddChangeStateDelegatesToEventHandler()
    {
        States.ForEach(s => s.StateChangeRequired += HandleChangeStateEvent);
    }

    // Imperative path: a state raised OnStateChangeRequired. We only enqueue here; the change is
    // applied from _Process. This is what keeps a transition raised inside StateEntered from
    // re-entering the machine mid-ChangeState.
    public void HandleChangeStateEvent(object sender, ChangeStateEventArgs args)
    {
        Enqueue(new PendingTransition(args.StateName));
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        EvaluateGuards();
        ProcessPendingTransitions();
    }

    // Poll the declarative transitions of every currently active leaf and enqueue the first
    // eligible one. Guards are expected to be side-effect free.
    private void EvaluateGuards()
    {
        foreach (var atomic in _atomicStates)
        {
            if (!atomic.Enabled) continue;

            var transition = atomic.GetEligibleTransition();
            if (transition is null) continue;
            if (transition.ToStateName == atomic.Name) continue; // ignore self-targets

            Enqueue(new PendingTransition(transition.ToStateName, transition));
        }
    }

    private void Enqueue(PendingTransition pending)
    {
        // Collapse duplicate requests to the same target queued within the same drain.
        foreach (var queued in _pendingTransitions)
        {
            if (queued.ToStateName == pending.ToStateName) return;
        }

        _pendingTransitions.Enqueue(pending);
    }

    // Drain the queue fully each frame. Because applying a transition fully completes its
    // ChangeState (and any transition raised during StateEntered just appends here), chains of
    // transient states resolve within this single frame instead of one hop per frame.
    private void ProcessPendingTransitions()
    {
        var processed = 0;
        while (_pendingTransitions.Count > 0)
        {
            if (++processed > MaxTransitionsPerFrame)
            {
                GD.PushError($"StateMachine exceeded {MaxTransitionsPerFrame} transitions in a single " +
                             "frame - likely a transition loop. Clearing pending transitions.");
                _pendingTransitions.Clear();
                return;
            }

            var pending = _pendingTransitions.Dequeue();
            ApplyTransition(pending);

            // Catch on-enter transitions of the states we just activated so they chain this frame.
            EvaluateGuards();
        }
    }

    private void ApplyTransition(PendingTransition pending)
    {
        if (!PathsToAtomicStatesDict.TryGetValue(pending.ToStateName, out var path))
        {
            throw new Exception($"Path to state {pending.ToStateName} not in this state machine");
        }

        pending.Transition?.OnTransition();

        for (int i = 0; i < path.Path.Count; i++)
        {
            if (path.Path[i] is CompoundState cState)
            {
                cState.NextState = path.Path[i + 1];
            }
        }

        foreach (var cState in path.Path.OfType<CompoundState>())
        {
            cState.ChangeState();
        }
    }

    public string GetStateMachineString()
    {
        return RootState.GetFullStateString();
    }
}
