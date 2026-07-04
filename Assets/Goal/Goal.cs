using Godot;

public partial class Goal : Area2D
{
    private GameManager _gameManager;
    private CanvasLayer _winScreen;

    public override void _Ready()
    {
        _gameManager = GetTree().GetFirstNodeInGroup("GameManager") as GameManager;
        _winScreen = GetNode<CanvasLayer>("WinScreen");
        GetNode<Button>("WinScreen/Overlay/Menu/Reset").Pressed += () => _gameManager.ResetGame();
        GetNode<Button>("WinScreen/Overlay/Menu/Quit").Pressed += () => _gameManager.QuitGame();
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not PlayerController) return;

        _winScreen.Visible = true;
        GetTree().Paused = true;
    }
}
