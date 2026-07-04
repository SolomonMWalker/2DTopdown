using Godot;
using System;

public partial class GameManager : Node
{
    [Signal] public delegate void OnResetGameEventHandler();
    [Signal] public delegate void OnQuitGameEventHandler();

    public void ResetGame()
    {
        EmitSignalOnResetGame();
        GetTree().Paused = false; // Paused is tree-level and survives the reload; clear it or the fresh scene starts frozen.
        GetTree().ReloadCurrentScene();
    }
    
    public void QuitGame()
    {
        EmitSignalOnQuitGame();
        GetTree().Quit();
    }
}
