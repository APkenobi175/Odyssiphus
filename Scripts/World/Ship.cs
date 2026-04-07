using Godot;
using System;

public partial class Ship : Node2D
{
    
public Button homeButton; //TEMP

    public override void _Ready()

    {
        GameManager.Instance.StopMusic(); // Stop the music when the ship level is loaded 
        // TODO: Instead of stopping the music, use GameManager.Instance.ChangeSong(some song) to change the music to something for the ship
        
    }
}