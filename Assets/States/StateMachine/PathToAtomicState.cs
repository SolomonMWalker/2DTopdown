using System.Collections.Generic;

namespace FirstPerson.CustomTypes.StateMachine;

public class PathToAtomicState
{
    public AtomicState AtomicState { get; set; }
    public List<State> Path { get; set; }

    public PathToAtomicState(AtomicState state)
    {
        AtomicState = state;
        Path = GetPathToRootNode(state);
    }

    public static List<State> GetPathToRootNode(State state)
    {
        var parent = state.GetParent();
        List<State> path = [];
        if (parent is State parentState)
        {
            path.AddRange(GetPathToRootNode(parentState));
        }

        path.Add(state);
        return path;
    }
}