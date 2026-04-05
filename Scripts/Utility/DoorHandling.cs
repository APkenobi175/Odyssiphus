using Godot;

public partial class BaseRoom : Node2D
{
    public Vector2I RoomGridPosition { get; private set; }

    public void Init(Vector2I gridPos)
    {
        RoomGridPosition = gridPos;
    }

    public override void _Ready()
    {
        var hallways = GameManager.Instance.CurrentDungeonHallways;
        var pos = RoomGridPosition;

        SetDoor("NorthDoor", hallways.Exists(h =>
            (h.From == pos && h.To == pos + new Vector2I(0, -1)) ||
            (h.To == pos && h.From == pos + new Vector2I(0, -1))));

        SetDoor("SouthDoor", hallways.Exists(h =>
            (h.From == pos && h.To == pos + new Vector2I(0, 1)) ||
            (h.To == pos && h.From == pos + new Vector2I(0, 1))));

        SetDoor("EastDoor", hallways.Exists(h =>
            (h.From == pos && h.To == pos + new Vector2I(1, 0)) ||
            (h.To == pos && h.From == pos + new Vector2I(1, 0))));

        SetDoor("WestDoor", hallways.Exists(h =>
            (h.From == pos && h.To == pos + new Vector2I(-1, 0)) ||
            (h.To == pos && h.From == pos + new Vector2I(-1, 0))));
    }

    private void SetDoor(string doorName, bool active)
    {
        var door = GetNodeOrNull<Node2D>($"Doors/{doorName}");
        if (door == null) return;
        door.Visible = active;
        var collision = door.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        if (collision != null) collision.Disabled = !active;
    }
}