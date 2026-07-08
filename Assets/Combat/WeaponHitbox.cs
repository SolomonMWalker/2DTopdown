using Godot;
using System.Collections.Generic;

// The damage-dealing region of a weapon. Starts inactive; the attack animation/driver calls
// Activate() at the start of a swing and Deactivate() when it ends. While active it hits any
// Hurtbox it overlaps, once per swing.
[GlobalClass]
public partial class WeaponHitbox : Area2D
{
    private readonly HashSet<Hurtbox> _hitThisSwing = new();

    public override void _Ready()
    {
        Monitoring = false; // begin unactivated regardless of what the scene saved
        AreaEntered += OnAreaEntered;
    }

    public void Activate()
    {
        _hitThisSwing.Clear();
        Monitoring = true;
    }

    public void Deactivate() => Monitoring = false;

    private void OnAreaEntered(Area2D area)
    {
        if (area is not Hurtbox hurtbox) return;
        if (!_hitThisSwing.Add(hurtbox)) return; // already hit this swing
        hurtbox.TakeHit();
    }
}
