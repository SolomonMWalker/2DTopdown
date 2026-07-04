using Godot;
using System;

public partial class GameManager : Node
{
    [Signal] public delegate void OnResetGameEventHandler();
    [Signal] public delegate void OnQuitGameEventHandler();

    public void ResetGame()
    {
        EmitSignalOnResetGame();
        GetTree().ReloadCurrentScene();
    }
    
    public void QuitGame()
    {
        EmitSignalOnQuitGame();
        GetTree().Quit();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
            QuitGame();
    }
}
