using Godot;
using System;

public partial class UiPauseMenu : CanvasLayer
{
    public Button resumeButton;
    public Button teleportToShipButton;
    public Button saveAndExitButton;

    public Button settingsButton;
    
    public override void _Ready()
    {
        Visible = false; // Start with the pause menu hidden
        GetTree().Paused = false; // Ensure the game is not paused when the pause menu is first created

        resumeButton = GetNode<Button>("PauseMenu/Buttons/ResumeButton");
        teleportToShipButton = GetNode<Button>("PauseMenu/Buttons/TeleportToShipButton");
        saveAndExitButton = GetNode<Button>("PauseMenu/Buttons/Exit");
        settingsButton = GetNode<Button>("PauseMenu/Buttons/SettingsButton");

        resumeButton.Pressed += OnResumePressed;
        teleportToShipButton.Pressed += OnTeleportToShipPressed;
        saveAndExitButton.Pressed += OnSaveAndExitPressed;
        settingsButton.Pressed += OnSettingsPressed;

        if(GameManager.Instance.PreviousScene == "Settings")
        {
            Visible = true; // If we are coming from the settings menu, show the pause menu immediately
            GetTree().Paused = true; // Pause the game immediately if we are coming from the settings menu
        }
        if(GameManager.Instance.CurrentScene == "Ship")
        {
            teleportToShipButton.Text = "Return to Dungeon";
        }
        else
        {
            teleportToShipButton.Text = "Teleport to Ship";
        }
    }

    public override void _Input(InputEvent e)
    {
        if (e.IsActionPressed("ui_cancel")){
            TogglePause();
        }
    }

    private void TogglePause()
    {
        Visible = !Visible; // Toggle the visibility of the pause menu
        GetTree().Paused = Visible; // Pause the game when the pause menu is visible, unpause when hidden
    }

    public void OnResumePressed()
    {
        Visible = false;
        GetTree().Paused = false; // Unpause the game when resuming
    }

    public void OnTeleportToShipPressed()
    {

        // get last scene
        if(GameManager.Instance.CurrentScene == "Ship") {
            GetTree().Paused = false; // Unpause the game before teleporting to ensure any necessary cleanup happens
            string previousScene = GameManager.Instance.PreviousScene;
            GameManager.Instance.GoTo(previousScene);
        }else{
            GetTree().Paused = false; // Unpause the game before teleporting to ensure any necessary cleanup happens
            GameManager.Instance.GoTo("Ship");
        }
    }

    public void OnSaveAndExitPressed()
    {
        GetTree().Paused = false; // Unpause the game before exiting to ensure any necessary cleanup happens
        GameManager.Instance.SaveGame();
        GameManager.Instance.GoTo("HomeScreen");
        GameManager.Instance.ChangeSong("Menu");
        GameManager.Instance.PlayMusic();
    }

    public void OnSettingsPressed()
    {
        GetTree().Paused = false; // Unpause the game before navigating to settings
        GameManager.Instance.GoTo("Settings");
    }

}
