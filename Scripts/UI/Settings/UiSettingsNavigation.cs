using Godot;
using System;

public partial class UiSettingsNavigation : CanvasLayer
{
    
    public Button backButton;


    public override void _Ready()
    {
        backButton = GetNode<Button>("SettingsNavigation/Back");
        backButton.Pressed += onBackPressed;
    }

    public void onBackPressed()
    {
        GameManager.Instance.GoTo("HomeScreen");
    }
}
