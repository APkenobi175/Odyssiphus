using Godot;
using System;

public partial class Ship : Node2D
{

    public Node2D openingCutscene;
    public AnimationPlayer cutsceneAnimation;
    


    public override void _Ready()

    {
        cutsceneAnimation = GetNode<AnimationPlayer>("AnimationPlayer");
        GameManager.Instance.StopMusic(); // Stop the music when the ship level is loaded 
        // TODO: Instead of stopping the music, use GameManager.Instance.ChangeSong(some song) to change the music to something for the ship

        if (GameManager.Instance.playOpeningCutscene)
        {
            cutsceneAnimation.Play("opening");
            GameManager.Instance.playOpeningCutscene = false; // Set the flag to false after playing the cutscene
        }
        
    }
}