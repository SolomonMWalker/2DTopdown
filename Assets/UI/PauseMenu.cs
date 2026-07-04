using Godot;

public partial class PauseMenu : CanvasLayer
{
    private GameManager _gameManager;

    public override void _Ready()
    {
        _gameManager = GetTree().GetFirstNodeInGroup("GameManager") as GameManager;
        Visible = false;
        GetNode<Button>("Overlay/Menu/Resume").Pressed += Resume;
        GetNode<Button>("Overlay/Menu/Reset").Pressed += () => _gameManager.ResetGame();
        GetNode<Button>("Overlay/Menu/Quit").Pressed += () => _gameManager.QuitGame();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!@event.IsActionPressed("ui_cancel")) return;

        if (GetTree().Paused) Resume();
        else Pause();
        GetViewport().SetInputAsHandled();
    }

    private void Pause()
    {
        GetTree().Paused = true;
        Visible = true;
    }

    private void Resume()
    {
        GetTree().Paused = false;
        Visible = false;
    }
}
