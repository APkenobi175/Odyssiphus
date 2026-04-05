using Godot;
using System;



public partial class Dungeon : Node2D
{

    // Packed scenes for all the room types

    [Export]
    public PackedScene StartRoomScene { get; set; }
    [Export]
    public PackedScene EnemyRoomScene { get; set; }
    [Export]
    public PackedScene TreasureRoomScene { get; set; }
    [Export]
    public PackedScene BossRoomScene { get; set; }
    [Export]
    public PackedScene MiniBossRoomScene { get; set; }
    [Export]
    public PackedScene PuzzleRoomScene { get; set; }

    // 640 x 360 
    private const int RoomWidth = 640;
    private const int RoomHeight = 360;

    private const float ZoomSpeed = 0.1f;
    private const float MinZoom = 0.1f;
    private const float MaxZoom = 1f;

    public override void _Ready()
    {

        // ADDED 3/29/26 ONLY GENERATE IF THERE IS NOT ONE ALREADY EXISTING.
        if (GameManager.Instance.CurrentDungeonRooms.Count == 0)
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
                seed: 0 // random walk function uses random seed when seed is 0
            );

            // 2. Populate the doors for each room
            RandomWalk.PopulateDoors(result.Rooms, result.Hallways);

            // 3. Mark start room as cleared so it shows on miniMap

            result.Rooms[0].IsCleared = true;

            GameManager.Instance.LoadDungeon(result.Rooms, result.Hallways, result.Seed); // Load the generated dungeon into the GameManager so it can be accessed by other parts of the game (like the minimap and player movement)
            GD.Print($"Generated dungeon with {result.Rooms.Count} rooms and {result.Hallways.Count} hallways. Max rooms hit: {result.maxRoomsHit}");

            
        }

        SpawnRooms();


    }

    private void SpawnRooms()
    {
        var container = GetNode<Node2D>("RoomsContainer");

        foreach (var room in GameManager.Instance.CurrentDungeonRooms)
        {
            PackedScene scene = room.RoomType switch
            {
                RoomType.Start => StartRoomScene,
                RoomType.EnemyRoom => EnemyRoomScene,
                RoomType.TreasureRoom => TreasureRoomScene,
                RoomType.BossRoom => BossRoomScene,
                RoomType.MiniBoss => MiniBossRoomScene,
                RoomType.PuzzleRoom => PuzzleRoomScene,
                _ => EnemyRoomScene
            };

            if (scene == null)
            {
                GD.PrintErr($"No scene assigned for room type {room.RoomType}");
                continue;
            }

            var instance = scene.Instantiate<Node2D>();
            instance.Position = new Vector2(room.Position.X * RoomWidth, room.Position.Y * RoomHeight);
            container.AddChild(instance);




        }

        // move camera to start room

        var camera = GetNode<Camera2D>("Camera2D");
        camera.Position = new Vector2(RoomWidth / 2f, RoomHeight / 2f);
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
                GameManager.Instance.OnPlayerEnterRoom(next.Position);

                // Move camera to the new room
                var camera = GetNode<Camera2D>("Camera2D");
                camera.Position = new Vector2(
                    next.Position.X * RoomWidth + RoomWidth / 2f,
                    next.Position.Y * RoomHeight + RoomHeight / 2f
                );
            }
            else
            {
                GD.Print("All rooms cleared!");
            }
        }

        if (e is InputEventMouseButton mouse)
        {
            var camera = GetNode<Camera2D>("Camera2D");
            if (mouse.ButtonIndex == MouseButton.WheelUp)
                camera.Zoom = (camera.Zoom + new Vector2(ZoomSpeed, ZoomSpeed)).Clamp(new Vector2(MinZoom, MinZoom), new Vector2(MaxZoom, MaxZoom));
            else if (mouse.ButtonIndex == MouseButton.WheelDown)
                camera.Zoom = (camera.Zoom - new Vector2(ZoomSpeed, ZoomSpeed)).Clamp(new Vector2(MinZoom, MinZoom), new Vector2(MaxZoom, MaxZoom));
        }
    }
    
}
