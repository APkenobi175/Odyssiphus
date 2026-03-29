using Godot;
using System;
using System.Collections.Generic;

public partial class MiniMap : Node2D
{
    // These get set by GameManager when a dungeon is loaded/generated
    public List<RandomWalkRoom> DungeonRooms = new();
    public Vector2I PlayerCurrentRoom = Vector2I.Zero;

    // Minimap display settings
    private const int CellSize = 10;   // pixels per room square
    private const int DoorSize = 4;    // pixels for door indicator squares
    private const int Spacing = 16;    // grid spacing between room centers
    private Vector2 Origin = new Vector2(80, 80); // offset from top-left of minimap area

    public override void _Ready()
    {
        GameManager.Instance.RegisterMiniMap(this); // Register this minimap with the GameManager so it can call Refresh when needed
    }

    public override void _Draw()
    {
        foreach (var room in DungeonRooms)
        {
            // Only draw cleared rooms + the current room (even if not cleared yet)
            if (!room.IsCleared && room.Position != PlayerCurrentRoom) continue;

            Vector2 screenPos = MapToScreen(room.Position);

            // Highlight current room with a white border
            if (room.Position == PlayerCurrentRoom)
                DrawRect(new Rect2(screenPos - new Vector2(7, 7), new Vector2(14, 14)), Colors.White);

            Color color = room.RoomType switch
            {
                RoomType.Start       => Colors.Green,
                RoomType.EnemyRoom   => Colors.Red,
                RoomType.TreasureRoom => Colors.Gold,
                RoomType.MiniBoss    => Colors.Purple,
                RoomType.PuzzleRoom  => Colors.Cyan,
                RoomType.BossRoom    => Colors.DarkRed,
                _                   => Colors.Gray
            };
            DrawRect(new Rect2(screenPos - new Vector2(CellSize / 2, CellSize / 2), 
                               new Vector2(CellSize, CellSize)), color);

            // Draw small door stubs on the edges of the room square
            foreach (var door in room.Doors)
            {
                Vector2 doorOffset = door switch
                {
                    "N" => new Vector2(0, -(CellSize / 2 + DoorSize / 2)),
                    "E" => new Vector2(CellSize / 2 + DoorSize / 2, 0),
                    "S" => new Vector2(0, CellSize / 2 + DoorSize / 2),
                    "W" => new Vector2(-(CellSize / 2 + DoorSize / 2), 0),
                    _   => Vector2.Zero
                };
                DrawRect(new Rect2(screenPos + doorOffset - new Vector2(DoorSize / 2, DoorSize / 2),
                                   new Vector2(DoorSize, DoorSize)), Colors.White);
            }
        }
    }

    // Converts grid coordinates to minimap screen coordinates
    private Vector2 MapToScreen(Vector2I gridPos)
    {
        return Origin + new Vector2(gridPos.X * Spacing, gridPos.Y * Spacing);
    }

    // Call this from GameManager whenever a room is cleared or the player moves
    public void Refresh(List<RandomWalkRoom> rooms, Vector2I currentRoomPos)
    {
        DungeonRooms = rooms;
        PlayerCurrentRoom = currentRoomPos;
        QueueRedraw();
    }
}