using Godot;
using System.Collections.Generic;

// Drop this node into a scene to make it lock-on capable. Its global position is the point the
// player locks onto. Instances self-register so PlayerController can enumerate targets without
// needing a group.
[GlobalClass]
public partial class LockOnComponent : Node2D
{
    public static readonly List<LockOnComponent> All = new();

    public override void _EnterTree() => All.Add(this);
    public override void _ExitTree() => All.Remove(this);
}
