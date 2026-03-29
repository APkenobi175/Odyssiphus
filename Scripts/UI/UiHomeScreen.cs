using Godot;
using System;

public partial class UiHomeScreen : CanvasLayer
{
    public Button loadGame;
    public Button newGame;
    public Button exitGame;
    public Button settings;



    public override void _Ready()
    {

        loadGame = GetNode<Button>("HomeControls/Buttons/Continue");
        newGame = GetNode<Button>("HomeControls/Buttons/StartNew");
        exitGame = GetNode<Button>("HomeControls/Buttons/Exit");
        settings = GetNode<Button>("HomeControls/Buttons/Settings");

        loadGame.Pressed += OnLoadGamePressed;
        newGame.Pressed += OnNewGamePressed;
        exitGame.Pressed += OnExitGamePressed;
        settings.Pressed += OnSettingsPressed;
    }


    public void OnLoadGamePressed()
    {
        GameManager.Instance.GoTo("LoadGame");
    }

    public void OnNewGamePressed()
    {
        GD.Print("New Game Pressed");
        // TODO: Start new game 

        // TEMPORARY: Load the ship scene
        GD.Print("Game Manager Instance: " + GameManager.Instance);
        GameManager.Instance.StopMusic();
        GameManager.Instance.GoTo("Dungeon");
    }

    public void OnExitGamePressed()
    {
        GD.Print("Exit Game Pressed");
        GetTree().Quit();
    }

    public void OnSettingsPressed()
    {
        GameManager.Instance.GoTo("Settings");
    }
    
}
