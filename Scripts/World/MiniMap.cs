using Godot;
using System;
using System.Collections.Generic;

public partial class MiniMap : Control
{
    public List<RandomWalkRoom> DungeonRooms = new();
    public Vector2I PlayerCurrentRoom = Vector2I.Zero;

    private const int CellSize = 10;
    private const int DoorSize = 3;
    private const int Spacing = 20;

    // The fixed size of the minimap window in pixels
    private const int MapWidth = 80;
    private const int MapHeight = 80;

    public List<RandomWalkHallway> Hallways = new();



    public override void _Ready()
    {
        ClipContents = true; // Ensure the minimap does not draw outside its bounds
        Size = new Vector2(MapWidth, MapHeight); // Set the size of the minimap control
        GameManager.Instance.RegisterMiniMap(this);
        Position = new Vector2(GetViewport().GetVisibleRect().Size.X - MapWidth - 10, 10); // Position the minimap in the top-right corner with a 10 pixel margin
    }

    public override void _Draw()
    {
    DrawRect(new Rect2(0, 0, MapWidth, MapHeight), new Color(0, 0, 0, 0.6f));
    DrawRect(new Rect2(0, 0, MapWidth, MapHeight), new Color(1, 1, 1, 0.2f), false, 1f);

    Vector2 center = new Vector2(MapWidth / 2f, MapHeight / 2f);

    // Draw hallway connections FIRST (so room squares render on top)
    foreach (var hallway in Hallways)
    {
        Vector2I deltaFrom = hallway.From - PlayerCurrentRoom;
        Vector2I deltaTo = hallway.To - PlayerCurrentRoom;
        Vector2 fromPos = center + new Vector2(deltaFrom.X * Spacing, deltaFrom.Y * Spacing);
        Vector2 toPos = center + new Vector2(deltaTo.X * Spacing, deltaTo.Y * Spacing);

        // Only draw if both rooms are cleared (or one is the current room)
        var fromRoom = DungeonRooms.Find(r => r.Position == hallway.From);
        var toRoom = DungeonRooms.Find(r => r.Position == hallway.To);
        if (fromRoom == null || toRoom == null) continue;
        bool eitherIsCurrent = fromRoom.Position == PlayerCurrentRoom || toRoom.Position == PlayerCurrentRoom;
        bool bothCleared = fromRoom.IsCleared && toRoom.IsCleared;
        if (!eitherIsCurrent && !bothCleared) continue;

        

        

        DrawLine(fromPos, toPos, new Color(1, 1, 1, 0.5f), 2f);
    }

    // Draw rooms on top of the lines
    foreach (var room in DungeonRooms)
    {
        if (!room.IsCleared && room.Position != PlayerCurrentRoom) continue;

        Vector2I delta = room.Position - PlayerCurrentRoom;
        Vector2 screenPos = center + new Vector2(delta.X * Spacing, delta.Y * Spacing);

        if (screenPos.X < 0 || screenPos.X > MapWidth ||
            screenPos.Y < 0 || screenPos.Y > MapHeight) continue;

        if (room.Position == PlayerCurrentRoom)
            DrawRect(new Rect2(screenPos - new Vector2(7, 7), new Vector2(14, 14)), Colors.White);

        Color color = room.RoomType switch
        {
            RoomType.Start        => Colors.LimeGreen,
            RoomType.EnemyRoom    => new Color(0.8f, 0.2f, 0.2f),
            RoomType.TreasureRoom => Colors.Gold,
            RoomType.MiniBoss     => Colors.MediumPurple,
            RoomType.PuzzleRoom   => Colors.CornflowerBlue,
            RoomType.BossRoom     => Colors.DarkRed,
            _                    => Colors.Gray
        };


        DrawRect(new Rect2(screenPos - new Vector2(CellSize / 2f, CellSize / 2f),
                           new Vector2(CellSize, CellSize)), color);

        if (room.IsCleared && room.Position != PlayerCurrentRoom)
        {
            // Draw checkmark for cleared room
            Vector2 checkStart = screenPos + new Vector2(-3, 0);
            Vector2 checkMid = screenPos + new Vector2(-1, 3);
            Vector2 checkEnd = screenPos + new Vector2(4, -3);
            DrawLine(checkStart, checkMid, Colors.White, 1.5f);
            DrawLine(checkMid, checkEnd, Colors.White, 1.5f);
        }
        }
    }

    public void Refresh(List<RandomWalkRoom> rooms, Vector2I currentRoomPos, List<RandomWalkHallway> hallways)
    {
        Hallways = hallways;
        DungeonRooms = rooms;
        PlayerCurrentRoom = currentRoomPos;
        QueueRedraw();
    }

    public override void _ExitTree()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsMiniMapRegistered(this))
        {
            GameManager.Instance.UnregisterMiniMap(this);
        }
           
    }
}