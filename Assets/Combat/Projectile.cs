using Godot;

// Straight-flying projectile. Travels along its local +x, hits the first thing it touches, and frees
// itself: on a Hurtbox it registers the hit first; on anything else (walls, bodies) it just dies.
// A lifetime timer cleans up projectiles that never hit anything.
[GlobalClass]
public partial class Projectile : Area2D
{
    [Export] public float Speed = 900f;
    [Export] public float Lifetime = 3f;

    private Node2D _shooter;
    private bool _spent; // one projectile = one hit; guards against striking two overlaps in the same frame

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
        BodyEntered += OnBodyEntered;
        // ponytail: safety net so projectiles that hit nothing (e.g. no collision on that wall) don't leak.
        GetTree().CreateTimer(Lifetime).Timeout += Die;
    }

    // Call right after spawning: remembers the shooter so the projectile doesn't detonate on its owner.
    public void Launch(Node2D shooter) => _shooter = shooter;

    public override void _PhysicsProcess(double delta)
    {
        // Global-space move so it flies straight even if spawned under a transformed parent.
        GlobalPosition += Vector2.Right.Rotated(GlobalRotation) * Speed * (float)delta;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (_spent) return;
        if (area is Hurtbox hurtbox) hurtbox.TakeHit(); // register the hit before we die
        Die();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_spent || body == _shooter) return; // don't detonate on whoever fired us
        Die(); // wall / environment / body enemy
    }

    private void Die()
    {
        _spent = true;
        QueueFree();
    }
}
