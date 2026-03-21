using Godot;
using System;

public partial class Ship : Node2D
{
    
public Button homeButton; //TEMP

    public override void _Ready()
    {
        homeButton = GetNode<Button>("Button"); //TEMP
        homeButton.Pressed += OnHomeButtonPressed; //TEMP
    }

    public void OnHomeButtonPressed() //TEMP
    {
        GameManager.Instance.GoTo("HomeScreen"); //TEMP
    }
}
