using Godot;

// Attach under anything that can be hit. It just marks a region as damageable and fires Hit when a
// WeaponHitbox strikes it; the owner decides what a hit means (flash, damage, knockback...).
[GlobalClass]
public partial class Hurtbox : Area2D
{
    [Signal] public delegate void HitEventHandler();

    public void TakeHit() => EmitSignal(SignalName.Hit);
}
