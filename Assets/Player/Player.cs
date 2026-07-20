using Godot;
using System;
using System.Linq;
using DTopdown.Assets.Components;

namespace DTopdown.Assets.Player;

public partial class Player : CharacterBody2D
{
    [Export] public float DefaultSpeed { get; set; }
    [Export] public float LockOnSearchRadius { get; set; } = 100f;
    
    private LockOnComponent _target;
    public LockOnComponent Target
    {
        get => _target;
        private set
        {
            if (IsInstanceValid(_target)) _target.DisableLockOn();
            _target = value;
            _target?.LockOn();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!@event.IsActionPressed("lock_on")) return;
        if (HasTarget()) Target = null;   // toggle off                                                                                                               
        else Acquire();
    }
    
    public bool HasTarget() => IsInstanceValid(Target);
    public bool Acquire()
    {
        var mousePos = GetGlobalMousePosition();
        Target = GetTree().GetNodesInGroup("lockonComponents")
            .OfType<LockOnComponent>()
            .Where(n => n.GlobalPosition.DistanceTo(mousePos) < LockOnSearchRadius)
            .MinBy(n => n.GlobalPosition.DistanceTo(mousePos));
        return Target != null;
    }
}
