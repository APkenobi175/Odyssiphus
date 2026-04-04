using Godot;
using System.Collections.Generic;

public partial class DevFullMap : Node2D
{
    private const int CellSize = 20;
    private const int Offset = 400;

    public override void _Ready()
    {
        var rooms = GameManager.Instance.CurrentDungeonRooms;
        GD.Print($"DevMap: {rooms?.Count ?? 0} rooms loaded");

        // Calculate center of all rooms and move camera there
        if (rooms != null && rooms.Count > 0)
        {
            Vector2 avg = Vector2.Zero;
            foreach (var room in rooms)
                avg += (Vector2)room.Position;
            avg /= rooms.Count;

            var camera = GetNode<Camera2D>("Camera2D");
            camera.Position = avg * CellSize + Vector2.One * Offset;
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        var rooms = GameManager.Instance.CurrentDungeonRooms;
        var hallways = GameManager.Instance.CurrentDungeonHallways;
        var playerRoom = GameManager.Instance.PlayerCurrentRoom;

        if (rooms == null || rooms.Count == 0)
        {
            DrawString(ThemeDB.FallbackFont, new Vector2(20, 20), "No dungeon loaded", HorizontalAlignment.Left, -1, 16, Colors.Red);
            return;
        }

        Vector2 center = new Vector2(600, 400); // center of your dev view
        int Spacing = 40;
        int CellSize = 16;

        // Draw hallways first
        foreach (var hallway in hallways)
        {
            Vector2 fromPos = center + new Vector2(hallway.From.X * Spacing, hallway.From.Y * Spacing);
            Vector2 toPos = center + new Vector2(hallway.To.X * Spacing, hallway.To.Y * Spacing);
            DrawLine(fromPos, toPos, new Color(1, 1, 1, 0.5f), 2f);
        }

        // Draw rooms
        foreach (var room in rooms)
        {
            Vector2 screenPos = center + new Vector2(room.Position.X * Spacing, room.Position.Y * Spacing);

            // White border if player is here
            if (room.Position == playerRoom)
                DrawRect(new Rect2(screenPos - new Vector2(10, 10), new Vector2(20, 20)), Colors.White);

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

            // Room type label
            DrawString(ThemeDB.FallbackFont, screenPos + new Vector2(-5, 4),
                room.RoomType switch {
                    RoomType.Start        => "S",
                    RoomType.BossRoom     => "B",
                    RoomType.MiniBoss     => "M",
                    RoomType.TreasureRoom => "T",
                    RoomType.PuzzleRoom   => "P",
                    _                     => "E"
                },
                HorizontalAlignment.Left, -1, 10, Colors.White);
        }
    }
}