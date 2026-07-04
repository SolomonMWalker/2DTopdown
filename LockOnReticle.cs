using Godot;

// White dot with a black outline. PlayerController positions it on the current lock-on target and
// toggles its visibility. Shape is constant, so it only draws once.
[GlobalClass]
public partial class LockOnReticle : Node2D
{
    [Export] public float Radius = 6f;
    [Export] public float Outline = 2f;

    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, Radius + Outline, Colors.Black);
        DrawCircle(Vector2.Zero, Radius, Colors.White);
    }
}
