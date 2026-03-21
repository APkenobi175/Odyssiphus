using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
    // Instance of game manager
    public static GameManager Instance { get; private set; }
    private AudioStreamPlayer2D musicPlayer; // Music player for background music
    
    public Dictionary<string, PackedScene> Levels = new(); // Dictionary for valid levels

    public Dictionary<string, AudioStream> Tracks = new(); // Dictionary for valid music tracks

    public override void _Ready()
    {
        Instance = this; // Set the instance to this object
        musicPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        


        // ================LEVEL LOADING================== //
        Levels["Ship"] = GD.Load<PackedScene>("Scenes/World/Ship.tscn"); // Load the ship level and add it to the dictionary
        Levels["HomeScreen"] = GD.Load<PackedScene>("Scenes/Main.tscn"); // Load the home screen and add it to the dictionary
        Levels["Settings"] = GD.Load<PackedScene>("Scenes/Settings/AudioSettings.tscn"); // Load the settings screen and add it to the dictionary
        // TODO: ADD MORE LEVELS



        // ================MUSIC================== //
        Tracks["Menu"] = GD.Load<AudioStream>("Assets/Sounds/Music/TitleScreen.mp3"); // Load the menu music and add it to the dictionary
         // TODO: ADD MORE TRACKS



    }

    public void GoTo(string key)
    {
        GetTree().ChangeSceneToPacked(Levels[key]); // Change the scene to the level specified by the key

        // TODO: ADD transitions or parameters for level change such as if you are allowed to change or not

    }

    public void SetVolume(float value)
    {
        AudioServer.SetBusVolumeDb(0, Mathf.LinearToDb(value)); // Set the volume of the master bus to the value specified by the slider
        AudioServer.SetBusMute(0, value == 0); // Mute the master bus if the slider is at 0
    }

    public void PlayMusic()
    {
        musicPlayer.Play(); // Play the background music
    }

    public void StopMusic()
    {
        musicPlayer.Stop(); // Stop the background music
    }

    public void ChangeSong(string key)
    {
        musicPlayer.Stream = Tracks[key]; // Set the music player's stream to the specified track
        musicPlayer.Play(); // Play the new song

    }
}
