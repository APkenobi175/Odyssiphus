using Godot;
using System;

public partial class YouWon : CanvasLayer
{

    public Button ReturnToMenuButton;

    public AnimationPlayer CreditsRoll;
    
    public override void _Ready()
    {
        GameManager.Instance.ChangeSong("Menu", 52f);
        GameManager.Instance.PlayMusic();

        ReturnToMenuButton = GetNode<Button>("HomeControls2/Buttons/Continue");
        ReturnToMenuButton.Pressed += OnReturnToMenuPressed;

        CreditsRoll = GetNode<AnimationPlayer>("AnimationPlayer");
        CreditsRoll.Play("Fade");




    }

    public void OnReturnToMenuPressed()
    {
        GameManager.Instance.StopMusic();
        GameManager.Instance.ChangeSong("Menu");
        GameManager.Instance.PlayMusic();
        GameManager.Instance.GoTo("HomeScreen");
        
    }
}
