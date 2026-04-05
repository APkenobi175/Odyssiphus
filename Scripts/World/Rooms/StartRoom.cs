using Godot;
using System;

public partial class StartRoom : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Get current room from GameManager
		var room = GameManager.Instance.currentRoom;

		// hide doors that don't exist
		GetNode<Node2D>("Doors/SouthDoor").Visible = room.Doors.Contains("N");
		GetNode<Node2D>("Doors/NorthDoor").Visible = room.Doors.Contains("S");
		GetNode<Node2D>("Doors/EastDoor").Visible = room.Doors.Contains("W");
		GetNode<Node2D>("Doors/WestDoor").Visible = room.Doors.Contains("E");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
