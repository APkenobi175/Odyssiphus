using Godot;
using System;
using System.Collections.Generic;

public partial class TreasureRoom : Node2D
{

	public RandomWalkRoom RoomData { get; set; } // This will be set by the Dungeon when it creates the room instance so that the room can spawn the correct enemies and update the cleared state when all enemies are defeated.
	// Called when the node enters the scene tree for the first time.

	[Export]
	public PackedScene AsclepiusRing { get; set; }
	[Export]
	public PackedScene HarpyFeather { get; set; }
	[Export]
	public PackedScene MirrorOfIaso { get; set; }
	[Export]
	public PackedScene WrathOfApollo { get; set; }

	public List<PackedScene> ItemSceneList => new List<PackedScene> {AsclepiusRing, HarpyFeather, MirrorOfIaso, WrathOfApollo };

	private bool hasSpawned = false;
	private Marker2D spawnPoint;



	public override void _Ready()
	{


		// APPLY SHADER TO TILEMAP and set the DEPTH PAREMETER
		var room = RoomData ?? GameManager.Instance.currentRoom;
		float depth = room.Depth;
		var tilemap = GetNode<TileMapLayer>("TileMapLayer2");
		var material = (ShaderMaterial)tilemap.Material.Duplicate(); // Added duplicate so that every room has its own.
		tilemap.Material = material;
		material.SetShaderParameter("depth", depth);
		GD.Print($"Setting shader depth to {depth} for room {room.Position} Treasure room btw");

		// Create my array of spawn points for enemies


		// Check hallways and set door visibility accordingly to only show doors that actually exist for this room
		
		// hide doors that don't exist
		GetNode<Node2D>("Doors/SouthDoor").Visible = room.Doors.Contains("N");
		GetNode<Node2D>("Doors/NorthDoor").Visible = room.Doors.Contains("S");
		GetNode<Node2D>("Doors/EastDoor").Visible = room.Doors.Contains("W");
		GetNode<Node2D>("Doors/WestDoor").Visible = room.Doors.Contains("E");

	}

	private void onRoomEntered(Node2D body)
	{
		if(body is not Entity entity || hasSpawned) return;
		hasSpawned = true;
		var room = RoomData ?? GameManager.Instance.currentRoom;
		// return; // Comment this out to enable enemy spawning
		if(room.IsCleared){
			return;
		}
		GD.Print("Player Entered Treasure Room!!");
		GameManager.Instance.OnRoomCleared(RoomData.Position);
		CallDeferred(nameof(SpawnTreasure));
	}

	private void SpawnTreasure()
	{
		GD.Print("Spawning Treasure!!");
		// 1. Randomly select treasure from list
		var random = new Random();
		int index = random.Next(ItemSceneList.Count);
		var treasureScene = ItemSceneList[index];

		// 2. Instance the treasure and add it to the scene
		var treasureInstance = (Node2D)treasureScene.Instantiate();
		spawnPoint = GetNode<Marker2D>("Marker2D");
		treasureInstance.Position = spawnPoint.Position;
		AddChild(treasureInstance);
	}
}
