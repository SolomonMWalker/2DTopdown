using Godot;

public partial class PlayerController : CharacterBody2D
{
    [Export] public float Speed = 200f;

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = Vector2.Zero;

        if (Input.IsActionPressed("forward"))
            direction.Y -= 1f;
        if (Input.IsActionPressed("backward"))
            direction.Y += 1f;
        if (Input.IsActionPressed("left"))
            direction.X -= 1f;
        if (Input.IsActionPressed("right"))
            direction.X += 1f;

        Velocity = direction.Normalized() * Speed;
        MoveAndSlide();
    }

    public override void _Process(double delta)
    {
        Vector2 toMouse = GetGlobalMousePosition() - GlobalPosition;
        if (toMouse.LengthSquared() > 0.0001f)
            Rotation = toMouse.Angle();
    }
}
