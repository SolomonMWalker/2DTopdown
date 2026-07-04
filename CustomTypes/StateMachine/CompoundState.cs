using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FirstPerson.CustomTypes.StateMachine;

//runs processing for itself and its chosen child
[GlobalClass]
public partial class CompoundState : State
{
    [Export] public string DefaultStateName;

    public List<State> ChildrenStates { get; private set; } = [];
    public State ActiveState;
    public State NextState;

    public override void _Ready()
    {
        ChildrenStates = GetChildren().OfType<State>().ToList();
        if (ChildrenStates.Count == 0)
        {
            throw new Exception("Compound state has no children states");
        }

        if (String.IsNullOrWhiteSpace(DefaultStateName))
        {
            DefaultStateName = ChildrenStates.First().Name;
        }

        if (!TryGetStateByName(DefaultStateName, out var state))
        {
            throw new Exception($"Default state name of {DefaultStateName} doesn't match with children states");
        }

        // Record the default child but DON'T enable it here. _Ready runs for every compound
        // bottom-up, so enabling the default child here would leave the default leaf of every
        // off-path compound (e.g. PreRound, RoundIntro, AttemptStart) Enabled at startup, even
        // though only the root's active branch should be live. Enabling flows top-down instead:
        // StateMachine._Ready calls RootState.Enable(), and CompoundState.Enable() cascades into
        // ActiveState, so exactly the active root-to-leaf path comes up enabled.
        ActiveState = state;
    }

    public override List<State> GetAllStates()
    {
        List<State> states = [this];
        foreach (var child in ChildrenStates)
        {
            states.AddRange(child.GetAllStates());
        }

        return states;
    }

    // Entering a compound cascades Enable() to its active child but does NOT call that child's
    // StateEntered() -- the short-circuit in ChangeState (target == current ActiveState) skips it
    // when the target leaf is the compound's default. Consequence: a leaf that is the default child
    // of its compound never gets StateEntered() on entry; only Enable(). Put entry logic for such a
    // leaf in Enable(), or drive it with a declarative transition (polled while enabled), not in
    // StateEntered(). See PassThroughState / RoundIntroState / AimMeterState for the pattern.
    public override void Enable()
    {
        base.Enable();
        ActiveState.Enable();
    }

    public override void Disable()
    {
        base.Disable();
        ChildrenStates.ForEach(cs => cs.Disable());
    }

    public bool TryGetStateByName(string stateName, out State state)
    {
        state = ChildrenStates.FirstOrDefault(s => s.Name.ToString().Equals(stateName));
        return state is not null;
    }

    public void ChangeState(string stateName = null)
    {
        stateName ??= NextState?.Name;
        NextState = null;

        if(TryGetStateByName(stateName, out var state))
        {
            if (state.Name == ActiveState.Name) return;
            
            ActiveState.StateExited();
            ActiveState = state;
            ActiveState.StateEntered();
        }
        else
        {
            throw new Exception($"State with name {stateName} is not found in children states");
        }
    }

    public override string GetFullStateString()
    {
        return $"{Name}({ActiveState.GetFullStateString()})";
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if(Enabled) StatePhysicsProcessing(delta);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if(Enabled) StateProcessing(delta);
    }
}