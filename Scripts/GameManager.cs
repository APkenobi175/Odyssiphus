using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    // This is used for the settings page so the back button can do different things based on where you came from
    public string PreviousScene { get; set; } = "";
    public string CurrentScene { get; set; } = "";

    public RandomWalkRoom currentRoom; // Track the player's current room for easy access to its properties when needed

    public bool characterIsTransitioning = false; // Flag to prevent multiple room transitions at once

    public int MiniBossesDeafted = 0; // Track the number of mini bosses defeated for potential use in scaling difficulty or unlocking content
    



    public override void _Ready()
    {
        Instance = this; // Set the instance to this object
        musicPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        CurrentScene = "HomeScreen"; // Set the initial scene to the home screen
        


        // ================LEVEL LOADING================== //
        Levels["Ship"] = GD.Load<PackedScene>("Scenes/World/Ship.tscn"); // Load the ship level and add it to the dictionary
        Levels["HomeScreen"] = GD.Load<PackedScene>("Scenes/Main.tscn"); // Load the home screen and add it to the dictionary
        Levels["Settings"] = GD.Load<PackedScene>("Scenes/Settings/Settings.tscn"); // Load the settings screen and add it to the dictionary
        Levels["Dungeon"] = GD.Load<PackedScene>("Scenes/World/Dungeon.tscn"); // Load the dungeon scene and add it to the dictionary
        Levels["LoadGame"] = GD.Load<PackedScene>("Scenes/World/LoadGame.tscn"); // Load the load game screen and add it to the dictionary
        Levels["DevMapView"] = GD.Load<PackedScene>("Scenes/AmmonsTestScenes/DEV_FullMap.tscn"); // Load the dev map view scene and add it to the dictionary
        Levels["YouDied"] = GD.Load<PackedScene>("Scenes/UI/YouDied.tscn"); // Load the you died screen and add it to the dictionary
        Levels["YouWon"] = GD.Load<PackedScene>("Scenes/UI/YouWon.tscn"); // Load the you won screen and add it to the dictionary
        // TODO: ADD MORE LEVELS



        // ================MUSIC================== //
        Tracks["Menu"] = GD.Load<AudioStream>("Assets/Sounds/Music/TitleScreen.ogg"); // Load the menu music and add it to the dictionary
         // TODO: ADD MORE TRACKS




    }

        //========================== DUNGEON STUFF =================================== //

    public void LoadDungeon(List<RandomWalkRoom> rooms, List<RandomWalkHallway> hallways, int seed)
    {
        CurrentDungeonRooms = rooms; // Set the current dungeon rooms to the generated rooms
        CurrentDungeonHallways = hallways; // Set the current dungeon hallways to the generated
        CurrentDungeonSeed = seed;

        var graphReplacement = new DungeonGraphReplacement();

        // Perform graph replacement to set rooms
        graphReplacement.Replace(rooms, hallways, seed);

        PlayerCurrentRoom = Vector2I.Zero; // Reset player position to the starting room
        currentRoom = GetRoomAt(PlayerCurrentRoom); // Set the current room to the starting room
        RefreshMiniMap(); // Refresh the minimap to show the new dungeon layout
    }

    public void OnPlayerEnterRoom(Vector2I newRoom)
    {
        PlayerCurrentRoom = newRoom; // Update the player's current room\
        currentRoom = GetRoomAt(PlayerCurrentRoom); // Update the current room reference to the new room

        if (currentRoom != null)
        {
            currentRoom.IsDiscovered = true; // Mark the new room as discovered when the player enters it
        }
        RefreshMiniMap(); // Refresh the minimap to show the player's new position

    }

    public void OnRoomCleared(Vector2I roomPos)
    {
        var room = GetRoomAt(roomPos);
        if (room == null) return; // If no room found at this position, do nothing
        room.IsCleared = true; // Mark the room as cleared
        RefreshMiniMap(); // Refresh the minimap to show the cleared room

        if (room.RoomType == RoomType.MiniBoss)
        {
            MiniBossesDeafted++; // Increment the mini boss defeat count if a mini boss was defeated
            GD.Print($"Mini Boss defeated! Total mini bosses defeated: {MiniBossesDeafted}");
        }

        // TODO: Unlock doors
    }

    public RandomWalkRoom GetRoomAt(Vector2I pos)
    {
        return CurrentDungeonRooms.Find(r => r.Position == pos); // Find and return the room at the specified position
    }



    // ========================== SCENE CHANGE =======================//

    public void GoTo(string key)
    {
        GetTree().Paused = false; // Ensure the game is unpaused when changing scenes
        PreviousScene = CurrentScene; // Set the previous scene to the current scene before changing
        CurrentScene = key; // Update the current scene to the new scene
        GetTree().ChangeSceneToPacked(Levels[key]); // Change the scene to the level specified by the key

        // TODO: ADD transitions or parameters for level change such as if you are allowed to change or not

    }

    public void GoBack()
    {
        if(PreviousScene != "")
        {
            GD.Print($"Going back to {PreviousScene} from {CurrentScene}");
            GoTo(PreviousScene); // Go back to the previous scene if it exists
        }
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

    public async Task ChangeSong(string key, float fromPosition = 0f)
    {
        musicPlayer.Stream = Tracks[key]; // Set the music player's stream to the specified track
        musicPlayer.Play(); // Play the new song
        if (fromPosition > 0f)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); // Wait for the next frame to ensure the song has started playing before seeking
            CallDeferred(nameof(DeferredSeek), fromPosition); // If a starting position is specified, seek to that position after the song starts playing
        }
        GD.Print($"Changed song to {key} starting at {fromPosition} seconds");
        GD.Print($"Current song position: {musicPlayer.GetPlaybackPosition()} seconds");

    }

    public void DeferredSeek(float positon)
    {
        musicPlayer.Seek(positon); // Seek to the specified position in the song
    }
    public void NewGame()
    {
        // If a new game is called make a new dungeon!
        CurrentDungeonRooms = new(); // Clear the current dungeon rooms
        CurrentDungeonHallways = new(); // Clear the current dungeon hallways
        CurrentDungeonSeed = 0; // Reset the current dungeon seed
        PlayerCurrentRoom = Vector2I.Zero; // Reset the player's current room
        currentRoom = null; // Clear the current room reference
        MiniBossesDeafted = 0; // Reset the mini boss defeat count
        GoTo("Ship"); // Go to the ship scene to start a new game
    }



    // ========================= SAVE GAME ========================= //

    private const string SaveDirectory = "user://saves/";
    private string dungeonData;
    private string characterData;
    public void SaveGame()
    {
        // TODO: IMPLEMENT SAVING CHARACTER DATA
        dungeonData = SaveDungeon();
        characterData = SaveCharacter();
        // Write to file 
        WriteSaveData(dungeonData, characterData);
    }

    private string SaveDungeon()
    {
        var data = new Godot.Collections.Dictionary();
        data["seed"] = CurrentDungeonSeed;
        data["playerRoom_x"] = PlayerCurrentRoom.X;
        data["currentScene"] = CurrentScene;
        data["playerRoom_y"] = PlayerCurrentRoom.Y;
        data["miniBossesDefeated"] = MiniBossesDeafted;

        var roomList = new Godot.Collections.Array();
        foreach (var room in CurrentDungeonRooms)
        {
            var r = new Godot.Collections.Dictionary();
            r["x"] = room.Position.X;
            r["y"] = room.Position.Y;
            r["isCleared"] = room.IsCleared;
            r["hasGhost"] = room.hasGhost;
            r["depth"] = room.Depth;
            r["roomType"] = (int)room.RoomType;
            r["isDiscovered"] = room.IsDiscovered;
            roomList.Add(r);
        }
        data["rooms"] = roomList;
        return Json.Stringify(data);
    }

    private string SaveCharacter()
    {
        var data = new Godot.Collections.Dictionary();
        // TODO: ADD CHARACTER DATA TO SAVE
        return Json.Stringify(data);
    }

    private void WriteSaveData(string dungeonData, string characterData)
    {
        // 1. Ensure save directory exists
        if (!DirAccess.DirExistsAbsolute(SaveDirectory))
        {
            DirAccess.MakeDirAbsolute(SaveDirectory);
        }
        // 2. Create unique filename with timestamp
        string timestamp = Time.GetDatetimeStringFromSystem().Replace(":", "-"); // Replace colons in timestamp
        string path = SaveDirectory + $"save_{timestamp}.json";

        // Combine dungeon and character data into one dictionary 

        var saveFile = new Godot.Collections.Dictionary();
        saveFile["savedAt"] = timestamp;
        saveFile["dungeon"] = dungeonData;
        saveFile["character"] = characterData;

        // 3. Write to file
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreString(Json.Stringify(saveFile));
        GD.Print($"Game saved to {path}");
    }

    public void LoadGame(string path)
    {
        // 1. Check if file exists
        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr($"Save file {path} does not exist!");
            return;
        }
        // 2. Open file and read data
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var json = new Json();
        // 3. Parse JSON 
        json.Parse(file.GetAsText());
        var saveFile = json.Data.AsGodotDictionary();

        // 4. Get dungeon data, and load it into the game manager
        var dungeonJson = new Json();
        dungeonJson.Parse(saveFile["dungeon"].AsString());
        var dungeonData = dungeonJson.Data.AsGodotDictionary();
        LoadDungeon(dungeonData);

        // 5. Get character data, and load it into the game manager
        var characterJson = new Json();
        characterJson.Parse(saveFile["character"].AsString());
        LoadCharacter(characterJson.Data.AsGodotDictionary());

        string scene = dungeonData.ContainsKey("currentScene") ? dungeonData["currentScene"].AsString() : "Ship";
        GoTo(scene); // Go to the saved scene after loading

    }

    private void LoadDungeon(Godot.Collections.Dictionary data)
    {
        //1.  Get the seed and play position
        CurrentDungeonSeed = (int)data["seed"];
        PlayerCurrentRoom = new Vector2I((int)data["playerRoom_x"], (int)data["playerRoom_y"]);
        


        //2.  ReGenerate the dungeon with the loaded seed
        var walker = new RandomWalk();
        var result = walker.Generate(
            minSteps: 5,
            maxSteps: 15,
            stepChance: 0.8f,
            branchChance: 0.3f,
            allowLoops: false,
            allowBranches: true,
            allowBranchesToConnect: false,
            seed: CurrentDungeonSeed
        );

        // 3. Re populate the doors for each room
        RandomWalk.PopulateDoors(result.Rooms, result.Hallways);

        // TODO: Do the graph replacement thingy (not done yet)
        // Maybe we don't need to do that here because we'll already be setting room type next

        // 5. Update the generated rooms with the cleared status and room types from the save data
        foreach(var entry in data["rooms"].AsGodotArray())
        {
            var r = entry.AsGodotDictionary();
            var pos = new Vector2I((int)r["x"], (int)r["y"]);
            var room = result.Rooms.Find(room => room.Position == pos);
            if (room == null) continue; // If no room found at this position, skip it
            room.IsCleared = r["isCleared"].AsBool();
            room.hasGhost = r["hasGhost"].AsBool();
            room.RoomType = (RoomType)(int)r["roomType"];
            room.Depth = (float)r["depth"];
            room.IsDiscovered = r["isDiscovered"].AsBool();
        }

        CurrentDungeonRooms = result.Rooms; // Set the current dungeon rooms to the loaded rooms
        CurrentDungeonHallways = result.Hallways; // Set the current dungeon hallways to the loaded hallways
        currentRoom = GetRoomAt(PlayerCurrentRoom); // Set the current room to the player's current room after loading
        MiniBossesDeafted = (int)data["miniBossesDefeated"]; // Set the mini boss defeat count to the loaded value
        RefreshMiniMap(); // Refresh the minimap to show the loaded dungeon
    }

    private void LoadCharacter(Godot.Collections.Dictionary data)
    {
        // TODO: IMPLEMENT CHARACTER LOADING
    }


    public Godot.Collections.Array<string> GetSaveFiles()
    {
        // 1. Create array for save files
        var saves = new Godot.Collections.Array<string>();

        if (!DirAccess.DirExistsAbsolute(SaveDirectory))
        {
            return saves; // Return empty array if no saves exist
        }

        // 2. Open save directory and get all save files
        using var dir = DirAccess.Open(SaveDirectory);
        if (dir == null)
        {
            GD.PrintErr($"Failed to open save directory {SaveDirectory}");
            return saves;
        }

        dir.ListDirBegin();
        string fileName = dir.GetNext();
        while (fileName != "")
        {
            if (fileName.EndsWith(".json"))
            {
                saves.Add(SaveDirectory + fileName); // Add full path of save file to array
            }
            fileName = dir.GetNext();
        }

        saves.Sort(); // Sort save files alphabetically
        saves.Reverse(); // Reverse to have most recent saves first
        return saves;


    }



////////////////////// MINIMAP STUFF //////////////////////
/// 

    public void RegisterMiniMap(MiniMap map)
    {
        miniMap = map;
        RefreshMiniMap(); // Refresh the minimap to show the current dungeon layout

    }

    public void UnregisterMiniMap(MiniMap map)
    {
        if (miniMap == map)
        {
            miniMap = null;
        }
    }

    public bool IsMiniMapRegistered(MiniMap map)
    {
        return miniMap == map;
    }

    public void RefreshMiniMap()
    {
        // Check IsInstanceValid so we don't call into a freed Godot object
        if (miniMap != null && GodotObject.IsInstanceValid(miniMap))
            miniMap.Refresh(CurrentDungeonRooms, PlayerCurrentRoom, CurrentDungeonHallways);
        else
            miniMap = null; // clean up bad reference if the minimap was freed without unregistering
    }

    /// FOR CHARACTER TRANSITIONS
    
    public void StartCharacterTransition()
    {
        characterIsTransitioning = true;
    }
    public void EndCharacterTransition()
    {
        characterIsTransitioning = false;
    }


    /// Other Utilities
    


    public void ResetDungeon()
    {
        foreach(var room in CurrentDungeonRooms)
        {
            if (room.RoomType == RoomType.Start || room.RoomType == RoomType.TreasureRoom)
            {
                room.IsCleared = true; // Keep start and treasure rooms set to clear so you can traverse them.
            }
            else
            {
                room.IsCleared = false;
            }
            
        }

        PlayerCurrentRoom = Vector2I.Zero; // Reset player position to the starting room
        currentRoom = GetRoomAt(PlayerCurrentRoom); // Set the current room to the starting room

        miniMap = null; // clear mini map

        characterIsTransitioning = false; // reset character transition state
        GetTree().Paused = false; // Unpause the game 
        MiniBossesDeafted = 0; // Reset mini boss defeat count
    }

    public void OnPlayerDied()
    {
        GoTo("YouDied"); // Go to the you died screen when the player dies
        ResetDungeon(); // Reset the dungeon
    }

    public void OnPlayerWon()
    {
        GoTo("YouWon"); // Go to the you won screen when the player wins
        ResetDungeon(); // Reset the dungeon
    }

}