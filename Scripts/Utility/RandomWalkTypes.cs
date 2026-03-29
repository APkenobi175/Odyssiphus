using Godot;
using System;
using System.Collections.Generic;
// Define a room and a hallway
// THIS IS WHAT I USED FOR RANDOM WALK ASSIGNMENT - Ammon Phipps 3/20/26

// NEW
public enum RoomType
{
    Start, // I think the ship room should be seperate from the dungeon and just in its own scene, so this would be the start of the dungeon
    EnemyRoom, // Room with enemies you have to fight
    TreasureRoom, // Room with treasure/loot
    MiniBoss, // Room with a mini boss fight
    PuzzleRoom, // Room with puzzle
    BossRoom, // Final boss fight (Final room)

}

public class RandomWalkRoom
{
    // Grid position of the room
    public Vector2I Position;
    public RoomType RoomType; // NEW: room type
    public bool IsCleared; // NEW: Whether the room is cleared
    public List<string> Doors; // NEW: List of doors (N, E, S, W)
    public bool hasGhost; // NEW: Whether the room has a ghost
    public float Depth; // NEW: Depth of the room (so we can calculate difficulty and shaders for the room)

    // Constructor
    public RandomWalkRoom(Vector2I pos) 
    {
        Position = pos; 
        RoomType = RoomType.EnemyRoom; // NEW default to enemy room
        IsCleared = false; // NEW default to not cleared
        Doors = new List<string>(); // NEW initialize empty list of doors
        hasGhost = false; // NEW default to no ghost
        Depth = 0.0f; // NEW default depth to 0, will be updated during generation
        
    }
}

public class RandomWalkHallway
{
    // Position of hallway start and end points
    public Vector2I From;
    public Vector2I To;

    // Constructor
    public RandomWalkHallway(Vector2I from, Vector2I to)
    {
        From = from;
        To = to; 
    }
}

public class RoomSaveData
{
    public Vector2I Position;
    public RoomType RoomType;
    public bool IsCleared;
    public List<string> Doors;
    public bool hasGhost;
    public float Depth;

    // Constructor to convert from RandomWalkRoom to RoomSaveData
    public RoomSaveData(RandomWalkRoom room)
    {
        Position = room.Position;
        RoomType = room.RoomType;
        IsCleared = room.IsCleared;
        Doors = room.Doors;
        hasGhost = room.hasGhost;
        Depth = room.Depth;
    }
}


