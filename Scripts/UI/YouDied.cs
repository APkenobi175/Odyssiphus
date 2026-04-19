using Godot;
using System;

public partial class YouDied : CanvasLayer
{
    
public Button backToMenuButton;

public Button backToShipButton;

    public override void _Ready()
    {
        backToMenuButton = GetNode<Button>("Continue");
        backToMenuButton.Pressed += OnBackToMenuPressed;
        backToShipButton = GetNode<Button>("Exit");
        backToShipButton.Pressed += OnBackToShipPressed;

    }

    private void OnBackToMenuPressed()
    {
        GameManager.Instance.ResetDungeon();
        GameManager.Instance.GoTo("HomeScreen");
    }

    private void OnBackToShipPressed()
    {
        GameManager.Instance.ResetDungeon();
        GameManager.Instance.GoTo("Ship");
    }
}
