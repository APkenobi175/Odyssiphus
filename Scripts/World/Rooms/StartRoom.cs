using Godot;
using System;

public partial class StartRoom : Node2D
{
	public RandomWalkRoom RoomData { get; set; } // This will be set by the Dungeon when it creates the room instance so that the room can spawn the correct enemies and update the cleared state when all enemies are defeated.

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		// APPLY SHADER TO TILEMAP and set the DEPTH PAREMETER
		var room = RoomData;
		float depth = room.Depth;
		var tilemap = GetNode<TileMapLayer>("TileMapLayer");
		var material = (ShaderMaterial)tilemap.Material.Duplicate(); // Added duplicate so that every room has its own.
		tilemap.Material = material;
		material.SetShaderParameter("depth", depth);
		GD.Print($"Setting shader depth to {depth} for room {room.Position} START room btw");

		// hide doors that don't exist
		GetNode<Node2D>("Doors/SouthDoor").Visible = room.Doors.Contains("S");
		GetNode<Node2D>("Doors/NorthDoor").Visible = room.Doors.Contains("N");
		GetNode<Node2D>("Doors/EastDoor").Visible = room.Doors.Contains("E");
		GetNode<Node2D>("Doors/WestDoor").Visible = room.Doors.Contains("W");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
