using Godot;
using System;

public partial class Settings : Node2D
{

    public CanvasLayer AudioSettings;
    public CanvasLayer VideoSettings;
    public CanvasLayer GameSettings;
    public CanvasLayer miscSettings;

    public UiSettingsNavigation uiSettingsNavigation;

    public Button GameSettingsButton;
    public Button AudioSettingsButton;
    public Button VideoSettingsButton;
    public Button MiscSettingsButton;



    public override void _Ready()
    {
        AudioSettings = GetNode<CanvasLayer>("UiAudioSettings");
        VideoSettings = GetNode<CanvasLayer>("UiVideoSettings");
        GameSettings = GetNode<CanvasLayer>("UiGameSettings");
        miscSettings = GetNode<CanvasLayer>("UiMiscSettings");

        uiSettingsNavigation = GetNode<UiSettingsNavigation>("UiSettingsNavigation");
        GameSettingsButton = uiSettingsNavigation.GetNode<Button>("SettingsNavigation/Game");
        AudioSettingsButton = uiSettingsNavigation.GetNode<Button>("SettingsNavigation/Audio");
        VideoSettingsButton = uiSettingsNavigation.GetNode<Button>("SettingsNavigation/Video");
        MiscSettingsButton = uiSettingsNavigation.GetNode<Button>("SettingsNavigation/Misc");

        AudioSettings.Visible = false;
        VideoSettings.Visible = false;
        GameSettings.Visible = true; // Default to Game Settings
        miscSettings.Visible = false;

        GameSettingsButton.GrabFocus(); // Set initial focus to the Game Settings button

        GameSettingsButton.Pressed += () => ShowSettings("Game");
        AudioSettingsButton.Pressed += () => ShowSettings("Audio");
        VideoSettingsButton.Pressed += () => ShowSettings("Video");
        MiscSettingsButton.Pressed += () => ShowSettings("Misc");

        

    }

    private void ShowSettings(string settingsType)
    {
        // Untoggle all buttons first so only the selected one is toggled
        GameSettingsButton.ButtonPressed = false;
        AudioSettingsButton.ButtonPressed = false;
        VideoSettingsButton.ButtonPressed = false;
        MiscSettingsButton.ButtonPressed = false;
        if (settingsType == "Game")
        {
            GD.Print("Game Settings Button Pressed");
            GameSettings.Visible = true;
            AudioSettings.Visible = false;
            VideoSettings.Visible = false;
            miscSettings.Visible = false;

        }
        else if (settingsType == "Audio")
        {
            GameSettings.Visible = false;
            AudioSettings.Visible = true;
            VideoSettings.Visible = false;
            miscSettings.Visible = false;
        }
        else if (settingsType == "Video")
        {
            GameSettings.Visible = false;
            AudioSettings.Visible = false;
            VideoSettings.Visible = true;
            miscSettings.Visible = false;
        }
        else if (settingsType == "Misc")
        {
            GameSettings.Visible = false;
            AudioSettings.Visible = false;
            VideoSettings.Visible = false;
            miscSettings.Visible = true;
        }
    }


}
