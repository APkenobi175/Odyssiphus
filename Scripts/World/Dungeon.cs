using Godot;
using System;



public partial class Dungeon : Node2D
{

    public override void _Ready()
    {
        // 1. Generate the dungeon layout using the RandomWalk algorithm
        var walker = new RandomWalk();
        var result = walker.Generate(
            minSteps: 5,
            maxSteps: 15,
            stepChance: 0.8f,
            branchChance: 0.3f,
            allowLoops: false,
            allowBranches: true,
            allowBranchesToConnect: false,
            seed: 12345
        );

        // 2. Populate the doors for each room
        RandomWalk.PopulateDoors(result.Rooms, result.Hallways);

        // 3. Mark start room as cleared so it shows on miniMap

        result.Rooms[0].IsCleared = true;

        GameManager.Instance.LoadDungeon(result.Rooms, result.Hallways, 12345); // Load the generated dungeon into the GameManager so it can be accessed by other parts of the game (like the minimap and player movement)

        GD.Print($"Generated dungeon with {result.Rooms.Count} rooms and {result.Hallways.Count} hallways. Max rooms hit: {result.maxRoomsHit}");
    }



    //////////////// FOR TESTING ONLY //////////////////////
    /// 
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey key && key.Pressed && key.Keycode == Key.Space)
        {
            var rooms = GameManager.Instance.CurrentDungeonRooms;
            var next = rooms.Find(r => !r.IsCleared);
            if (next != null)
            {
                GD.Print($"Marking room at {next.Position} as cleared");
                GameManager.Instance.OnRoomCleared(next.Position);
                GameManager.Instance.OnPlayerEnterRoom(next.Position); // Move player to the next room for testing purposes
            }
            else
            {
                GD.Print("All rooms cleared!");
            }
        }
    }
    
}
