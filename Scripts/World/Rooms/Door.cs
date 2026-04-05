using Godot;
using System;

public partial class Door : Area2D
{
    
    // This script will be applied to every single door, so we should set the direction of the door in the editor for each one.
    [Export] public string Direction { get; set; } // "N", "S", "E", "W"

    const int ROOM_WIDTH = 640;
    const int ROOM_HEIGHT = 360;

    const int HALLWAY_LENGTH = 64;

    Vector2 roomWorldPos;

    private bool characterTransitioning = false; // flag to prevent multiple transitions at once


    public override void _Ready()
    {
        
        var room = GameManager.Instance.currentRoom;
        if (room == null) return;

        SetDoor("NorthDoor", room.Doors.Contains("N"));
        SetDoor("SouthDoor", room.Doors.Contains("S"));
        SetDoor("EastDoor", room.Doors.Contains("E"));
        SetDoor("WestDoor", room.Doors.Contains("W"));
        // 1. Detect on body entered
        BodyEntered += OnBodyEntered;
    }

    private void SetDoor(string doorName, bool active)
    {
        var door = GetNodeOrNull<Area2D>($"Doors/{doorName}");
        if (door == null)
        {
            return;
        }
        door.Visible = active;
        door.GetNode<CollisionShape2D>("Door").Disabled = !active;
    }

    private void OnBodyEntered(Node2D body)
    {
        // 1. Check and make sure room is cleared before allowing player to move through door
        if (GameManager.Instance.currentRoom == null || !GameManager.Instance.currentRoom.IsCleared)
        {
            GD.Print("Room is not cleared yet! Cannot move through door.");
            return;
        }

        if (GameManager.Instance.characterIsTransitioning)
        {
            GD.Print("Already transitioning between rooms! Please wait.");
            return;
        }

        // 2. If it is call deffered a transition to the next room in the direction of the door

        if (!(body is CharacterBody2D))
        {
            return;

        }

        var currentPos = GameManager.Instance.PlayerCurrentRoom;
        Vector2I nextPos = GetNextPos(currentPos);

        var nextRoom = GameManager.Instance.GetRoomAt(nextPos);
        if (nextRoom == null)
        {
            GD.PrintErr($"No room exists at position {nextPos}! Cannot transition.");
            return;
        }
        if (!GameManager.Instance.currentRoom.IsCleared && !nextRoom.IsCleared)
        {
            GD.Print("Room is not cleared yet! Cannot move through door.");
            return;
        }

        GameManager.Instance.characterIsTransitioning = true; // set flag to prevent multiple transitions at once

        CallDeferred(nameof(TransitionToNextRoom));




    }

    private void TransitionToNextRoom()
    {
        Vector2I nextPos;
        // 1. Figure out which room to go to
        var currentPos = GameManager.Instance.PlayerCurrentRoom;
        if (Direction == "N")
        {
            nextPos = currentPos + new Vector2I(0, -1);
        } else if (Direction == "S")
        {
            nextPos = currentPos + new Vector2I(0, 1);
        }
        else if (Direction == "E")
        {
            nextPos = currentPos + new Vector2I(1, 0);
        }
        else if (Direction == "W")
        {
            nextPos = currentPos + new Vector2I(-1, 0);
        }
        else
        {
            GD.PrintErr("Invalid door direction! Must be N, S, E, or W.");
            return;
        }

        // Check and make sure that room actually exists
        var nextRoom = GameManager.Instance.GetRoomAt(nextPos);
        if (nextRoom == null)
        {
            GD.PrintErr($"No room exists at position {nextPos}! Cannot transition.");
            return;
        }

        // 2. Update Game manager

        GameManager.Instance.OnPlayerEnterRoom(nextPos);

        // 3. Move Camera nice and smoothly to the new room (center of the room)
        var dungeon = GetTree().CurrentScene as Dungeon;
        dungeon?.MoveCamera(nextPos);

        // 4. Move player to opposite side of the new room
        // Change this to correct player node.
        var player = GetTree().CurrentScene.GetNode<CharacterBody2D>("Player");

        Vector2 roomWorldPos = new Vector2(nextPos.X * (ROOM_WIDTH + HALLWAY_LENGTH), nextPos.Y * (ROOM_HEIGHT + HALLWAY_LENGTH));

        int wallThickness = 32;
        int buffer = 10 + wallThickness; // small buffer to prevent player from getting stuck in door collision

        if (Direction == "N")
        {
            Vector2 oldPosition = player.Position;
            player.Position = roomWorldPos + new Vector2(320, ROOM_HEIGHT - buffer); // spawn at south side of new room
            GD.Print("Moved to new room to the North!");
            GD.Print($"Old Position: {oldPosition}, New Position: {player.Position}");
            
        }
        else if (Direction == "S")
        {
            Vector2 oldPosition = player.Position; 
            player.Position = roomWorldPos + new Vector2(320, buffer);  // spawn at north side of new room
            GD.Print("Moved to new room to the South!");
            GD.Print($"Old Position: {oldPosition}, New Position: {player.Position}");

        }
        else if (Direction == "E")
        {
            Vector2 oldPosition = player.Position;

            player.Position = roomWorldPos + new Vector2(buffer, ROOM_HEIGHT / 2f);  // spawn at west side of new room
            GD.Print("Moved to new room to the East!");
            GD.Print($"Old Position: {oldPosition}, New Position: {player.Position}");
        }
        else if (Direction == "W")
        {
            Vector2 oldPosition = player.Position;
            player.Position = roomWorldPos + new Vector2(ROOM_WIDTH - buffer, ROOM_HEIGHT / 2f); // spawn at east side of new room
            GD.Print("Moved to new room to the West!");
            GD.Print($"Old Position: {oldPosition}, New Position: {player.Position}");
        }

        GetTree().CreateTimer(0.5f).Timeout += () => GameManager.Instance.EndCharacterTransition(); // reset flag after transition is done, allowing new transitions to occur
    }

    private Vector2I GetNextPos(Vector2I currentPos)
    {
        if (Direction == "N")
        {
            return currentPos + new Vector2I(0, -1);
        }
        else if (Direction == "S")
        {
            return currentPos + new Vector2I(0, 1);
        }
        else if (Direction == "E")
        {
            return currentPos + new Vector2I(1, 0);
        }
        else if (Direction == "W")
        {
            return currentPos + new Vector2I(-1, 0);
        }
        else
        {
            GD.PrintErr("Invalid door direction! Must be N, S, E, or W.");
            return currentPos; // default to no movement if invalid direction
        }
    }
}

