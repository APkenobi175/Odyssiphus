using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
    // Instance of game manager
    public static GameManager Instance { get; private set; }
    
    public Dictionary<string, PackedScene> Levels = new(); // Dictionary for valid levels

    public override void _Ready()
    {
        Instance = this; // Set the instance to this object

        Levels["Ship"] = GD.Load<PackedScene>("Scenes/Ship.tscn"); // Load the ship level and add it to the dictionary

        // TODO: ADD MORE LEVELS
    }

    public void GoTo(string key)
    {
        GetTree().ChangeSceneToPacked(Levels[key]); // Change the scene to the level specified by the key

        // TODO: ADD transitions or parameters for level change such as if you are allowed to change or not
        // I added the ship level just to start.

    }
}
