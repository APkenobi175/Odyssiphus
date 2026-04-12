using Godot;
using System;
using System.Collections.Generic;
using System.Linq;



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

    private const int HallwayLength = 64;

    private const int MaxRooms = 30; // Added a max room limit to prevent infinite loops in generation, can be adjusted as needed.
    private const int minRooms = 20; // Added a minimum room limit to ensure dungeons aren't too small, can be adjusted as needed.

    public override void _Ready()
    {

        // ADDED 3/29/26 ONLY GENERATE IF THERE IS NOT ONE ALREADY EXISTING.
        if (GameManager.Instance.CurrentDungeonRooms.Count == 0)
        {
            
            // 1. Generate the dungeon layout using the RandomWalk algorithm
            var walker = new RandomWalk();
            var result = walker.Generate(
                minSteps: 5,
                maxSteps: 10,
                stepChance: 0.7f,
                branchChance: 0.2f,
                allowLoops: false,
                allowBranches: true,
                allowBranchesToConnect: false,
                seed: 0 // random walk function uses random seed when seed is 0
            );

            if (result.Rooms.Count < minRooms || result.Rooms.Count > MaxRooms)
            {
                GD.Print($"Generated dungeon with {result.Rooms.Count} rooms, which is outside the desired range of {minRooms}-{MaxRooms}. Regenerating...");
                // Clear the generated rooms and hallways before regenerating
                result.Rooms.Clear();
                result.Hallways.Clear();
                _Ready(); // Call _Ready again to regenerate
                return;
            }

            // 2. Populate the doors for each room
            RandomWalk.PopulateDoors(result.Rooms, result.Hallways);

            // 3. Mark start room as cleared so it shows on miniMap

            result.Rooms[0].IsCleared = true;

            GameManager.Instance.LoadDungeon(result.Rooms, result.Hallways, result.Seed); // Load the generated dungeon into the GameManager so it can be accessed by other parts of the game (like the minimap and player movement)
            GD.Print($"Generated dungeon with {result.Rooms.Count} rooms and {result.Hallways.Count} hallways. Max rooms hit: {result.maxRoomsHit}");
            var positions = GameManager.Instance.CurrentDungeonRooms.Select(r => r.Position).ToList();
            var duplicates = positions.GroupBy(p => p).Where(g => g.Count() > 1).ToList();
            GD.Print($"Duplicate room positions: {duplicates.Count}");
            foreach (var dup in duplicates)
            {
                GD.Print($"Position {dup.Key} has {dup.Count()} duplicates");
            }

        }

        SpawnRooms();
        


    }

    private bool hasSpawned = false; // Added to prevent multiple spawns when _Ready is called more than once (which can happen when re-entering the dungeon from the boss fight for example)

    private void SpawnRooms()
    {

        if (hasSpawned)
        {
            return;
        }
        hasSpawned = true;
        var container = GetNode<Node2D>("RoomsContainer");
        var spawnedPositions = new HashSet<Vector2I>(); // keep track of spawned positions to avoid duplicates

        foreach (var room in GameManager.Instance.CurrentDungeonRooms)
        {
            if (!spawnedPositions.Add(room.Position))
            {
                GD.PrintErr($"Duplicate room position detected: {room.Position}. Skipping spawn for this room.");
                continue;
            }
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
            instance.Position = new Vector2(room.Position.X * (RoomWidth + HallwayLength), room.Position.Y * (RoomHeight + HallwayLength));

            // WE NEED TO SET DOOR VISIBILITY FOR EACH ROOM

            if (instance is EnemyRoom enemyRoom)
                enemyRoom.RoomData = room;
            else if (instance is MiniBossRoom miniBossRoom)
                miniBossRoom.RoomData = room;
            else if (instance is BossRoom bossRoom)
                bossRoom.RoomData = room;
            else if (instance is TreasureRoom treasureRoom)
                treasureRoom.RoomData = room;
            else if (instance is PuzzleRoom puzzleRoom)
                puzzleRoom.RoomData = room;

            container.AddChild(instance);
            SetRoomDoors(instance, room);





        }

        // move camera to start room

        var camera = GetNode<Camera2D>("Camera2D");
        camera.Position = new Vector2(RoomWidth / 2f, RoomHeight / 2f);
    }

    private void SetRoomDoors(Node2D instance, RandomWalkRoom room)
    {
        SetDoor(instance, "NorthDoor", room.Doors.Contains("N"));
        SetDoor(instance, "SouthDoor", room.Doors.Contains("S"));
        SetDoor(instance, "EastDoor", room.Doors.Contains("E"));
        SetDoor(instance, "WestDoor", room.Doors.Contains("W"));
    }

    private void SetDoor(Node2D instance, string doorName, bool active)
    {
        var door = instance.GetNodeOrNull<Node2D>($"Doors/{doorName}");
        if (door == null)
        {
            GD.PrintErr($"No door named {doorName} found in room instance!");
            return;
        }
        door.Visible = active;
        var collision = door.GetNodeOrNull<CollisionShape2D>("Door");
        if (collision != null) collision.Disabled = !active;
    }



    //////////////// FOR TESTING ONLY //////////////////////
    /// 
    
    public override void _Input(InputEvent e)
    {
        // Changed - Now it only marks room as cleared when space is pressed.
        if (e is InputEventKey key && key.Pressed && key.Keycode == Key.Space)
        {
            {
                var currentPos = GameManager.Instance.PlayerCurrentRoom;
                GameManager.Instance.OnRoomCleared(currentPos);
                GD.Print($"Cleared room at {currentPos}");
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

    public void MoveCamera(Vector2I roomPos)
    {
        var camera = GetNode<Camera2D>("Camera2D");
        var tween = CreateTween();
        tween.TweenProperty(camera, "position",
            new Vector2(
                roomPos.X * (RoomWidth + HallwayLength) + RoomWidth / 2f,
                roomPos.Y * (RoomHeight + HallwayLength) + RoomHeight / 2f),
            0.3f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }
}
