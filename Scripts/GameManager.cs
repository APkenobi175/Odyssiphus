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

    // Current dungeon data
    public List<RandomWalkRoom> CurrentDungeonRooms = new();
    public List<RandomWalkHallway> CurrentDungeonHallways = new();
    public int CurrentDungeonSeed = 0;
    public Vector2I PlayerCurrentRoom = Vector2I.Zero; // Track the player's current room position in the dungeon
    private MiniMap miniMap; // Reference to the minimap to update it when the dungeon changes or player moves

    public override void _Ready()
    {
        Instance = this; // Set the instance to this object
        musicPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        


        // ================LEVEL LOADING================== //
        Levels["Ship"] = GD.Load<PackedScene>("Scenes/World/Ship.tscn"); // Load the ship level and add it to the dictionary
        Levels["HomeScreen"] = GD.Load<PackedScene>("Scenes/Main.tscn"); // Load the home screen and add it to the dictionary
        Levels["Settings"] = GD.Load<PackedScene>("Scenes/Settings/Settings.tscn"); // Load the settings screen and add it to the dictionary
        Levels["Dungeon"] = GD.Load<PackedScene>("Scenes/World/Dungeon.tscn"); // Load the dungeon scene and add it to the dictionary
        // TODO: ADD MORE LEVELS



        // ================MUSIC================== //
        Tracks["Menu"] = GD.Load<AudioStream>("Assets/Sounds/Music/TitleScreen.mp3"); // Load the menu music and add it to the dictionary
         // TODO: ADD MORE TRACKS




    }

        //========================== DUNGEON STUFF =================================== //

    public void LoadDungeon(List<RandomWalkRoom> rooms, List<RandomWalkHallway> hallways, int seed)
    {
        CurrentDungeonRooms = rooms; // Set the current dungeon rooms to the generated rooms
        CurrentDungeonHallways = hallways; // Set the current dungeon hallways to the generated
        CurrentDungeonSeed = seed;
        PlayerCurrentRoom = Vector2I.Zero; // Reset player position to the starting room
        RefreshMiniMap(); // Refresh the minimap to show the new dungeon layout
    }

    public void OnPlayerEnterRoom(Vector2I newRoom)
    {
        PlayerCurrentRoom = newRoom; // Update the player's current room
        RefreshMiniMap(); // Refresh the minimap to show the player's new position
    }

    public void OnRoomCleared(Vector2I roomPos)
    {
        var room = GetRoomAt(roomPos);
        if (room == null) return; // If no room found at this position, do nothing
        room.IsCleared = true; // Mark the room as cleared
        RefreshMiniMap(); // Refresh the minimap to show the cleared room

        // TODO: Unlock doors
    }

    public RandomWalkRoom GetRoomAt(Vector2I pos)
    {
        return CurrentDungeonRooms.Find(r => r.Position == pos); // Find and return the room at the specified position
    }

    public void RegisterMiniMap(MiniMap map)
    {
        miniMap = map;
        RefreshMiniMap(); // Refresh the minimap to show the current dungeon layout

    }

    private void RefreshMiniMap()
    {
        miniMap?.Refresh(CurrentDungeonRooms, PlayerCurrentRoom); // Update the minimap with the current dungeon rooms and player position
    }

    // ========================== SCENE CHANGE =======================//

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
