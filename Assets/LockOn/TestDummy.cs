using Godot;

// Scaffold hit reaction: flash the sprite white on hit, then tween back to its resting modulate.
// (The dummy already rests on red, so the flash pops to white to read as a hit.)
public partial class TestDummy : Node2D
{
    private Sprite2D _sprite;
    private Color _restColor;
    private Tween _flash;

    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _restColor = _sprite.Modulate;
        GetNode<Hurtbox>("Hurtbox").Hit += OnHit;
    }

    private void OnHit()
    {
        _flash?.Kill();
        _sprite.Modulate = Colors.White;
        _flash = CreateTween();
        _flash.TweenProperty(_sprite, "modulate", _restColor, 0.25);
    }
}
