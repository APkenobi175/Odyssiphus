using Godot;
using System;

public partial class Ship : Node2D
{
    
public Button homeButton; //TEMP

    public override void _Ready()

    {
        GameManager.Instance.StopMusic(); // Stop the music when the ship level is loaded 
        // TODO: Instead of stopping the music, use GameManager.Instance.ChangeSong(some song) to change the music to something for the ship
        
        homeButton = GetNode<Button>("Button"); //TEMP
        homeButton.Pressed += OnHomeButtonPressed; //TEMP
    }

    public void OnHomeButtonPressed() //TEMP
    {
        
        GameManager.Instance.GoTo("HomeScreen"); //TEMP\
        GameManager.Instance.ChangeSong("Menu"); // Change the music to the menu track when the main scene is ready
        GameManager.Instance.PlayMusic(); // Start playing music when the main scene is ready
    }
}
