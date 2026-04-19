using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

public partial class MiniBossRoom : Node2D
{

	[Export]
	public PackedScene Cyclops { get; set; }
	[Export]
	public PackedScene Sirens { get; set; }
	[Export]
	public PackedScene Scylla { get; set; }
	[Export]
	public PackedScene Cerberus { get; set; }
	private bool hasSpawned = false;
	private Marker2D spawnPoint;

	Node2D enemyContainer; 

	public RandomWalkRoom RoomData { get; set; } 
	public override void _Ready()
	{


		// APPLY SHADER TO TILEMAP and set the DEPTH PAREMETER
		var room = RoomData ?? GameManager.Instance.currentRoom;
		float depth = room.Depth;
		var tilemap = GetNode<TileMapLayer>("TileMapLayer");
		var material = (ShaderMaterial)tilemap.Material.Duplicate(); // Added duplicate so that every room has its own.
		tilemap.Material = material;
		material.SetShaderParameter("depth", depth);
		GD.Print($"Setting shader depth to {depth} for room {room.Position} Mini boss room btw");

		// Check hallways and set door visibility accordingly to only show doors that actually exist for this room

		spawnPoint = GetNode<Marker2D>("SpawnPoint");
		
		// hide doors that don't exist
		GetNode<Node2D>("Doors/SouthDoor").Visible = room.Doors.Contains("S");
		GetNode<Node2D>("Doors/NorthDoor").Visible = room.Doors.Contains("N");
		GetNode<Node2D>("Doors/EastDoor").Visible = room.Doors.Contains("E");
		GetNode<Node2D>("Doors/WestDoor").Visible = room.Doors.Contains("W");



	}

	private async void onRoomEntered(Node2D body)
	{
		GD.Print("Player Entered Mini Boss Room!!");
		if(body is not Entity entity || hasSpawned) return;

		hasSpawned = true;

		GD.Print("Spawning Mini Boss!!");

		var miniBossScene = pickMiniBossToSpawn();
		if (miniBossScene == null)
		{
			return;
		}

		var room = RoomData ?? GameManager.Instance.currentRoom;

		if(!room.IsCleared)
		{
			//3 second delay before the mini boss spawns to give the player a moment to prepare
			await WaitFor(3);
			spawnEnemy(miniBossScene);
		}
	}

	private PackedScene pickMiniBossToSpawn()
	{
		int miniBossesDefeated = GameManager.Instance.MiniBossesDeafted;

		switch (miniBossesDefeated)
		{
			case 0:
				return Cyclops;
			case 1:
				return Sirens;
			case 2:
				return Scylla;
			case 3:
				return Cerberus;
			default:
				return null;
		}
	}

	private void spawnEnemy(PackedScene miniBossScene)
	{
		var miniBossInstance = (Node2D)miniBossScene.Instantiate();
		miniBossInstance.Position = spawnPoint.Position;
		AddChild(miniBossInstance);

	}

	private async Task WaitFor(float seconds)
	{
		await ToSignal(GetTree().CreateTimer(seconds), "timeout");
	}

}