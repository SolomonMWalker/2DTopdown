using Godot;

public partial class PlayerController : CharacterBody2D
{
    // Vignette overlay the crouch state toggles. Cached here so states go through the controller
    // instead of hardcoding scene paths.
    public CanvasItem Vignette { get; private set; }

    // _EnterTree, not _Ready: this is the scene root, so its _Ready runs AFTER its children's -
    // including StateMachine._Ready, which enables the start state and touches Vignette.
    // _EnterTree runs before any child _Ready, so the ref is set in time.
    public override void _EnterTree()
    {
        Vignette = GetNode<CanvasItem>("Vignette/VignetteRect");
    }

    // Movement (WASD + speed) lives in the MovementState machine now; this just aims at the mouse.
    public override void _Process(double delta)
    {
        Vector2 toMouse = GetGlobalMousePosition() - GlobalPosition;
        if (toMouse.LengthSquared() > 0.0001f)
            Rotation = toMouse.Angle();
    }
}
