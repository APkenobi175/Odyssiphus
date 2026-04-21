using Godot;
using System;

public partial class YouDied : CanvasLayer
{
    
public Button backToMenuButton;

public Button backToShipButton;

    public override void _Ready()
    {
        backToMenuButton = GetNode<Button>("HomeControls/Buttons/Exit");
        backToMenuButton.Pressed += OnBackToMenuPressed;
        backToShipButton = GetNode<Button>("HomeControls/Buttons/Continue");
        backToShipButton.Pressed += OnBackToShipPressed;

    }

    private void OnBackToMenuPressed()
    {
        GameManager.Instance.GoTo("HomeScreen");
    }

    private void OnBackToShipPressed()
    {
        GameManager.Instance.GoTo("Ship");
        GameManager.Instance.SaveGame();
    }
}
