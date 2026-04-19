using Godot;
using System;

public partial class BossRoom : Node2D
{
	[Export]
	public PackedScene BossScene { get; set; }

	private Marker2D spawnPoint;

	private bool hasSpawned = false;

	public RandomWalkRoom RoomData { get; set; } // This will be set by the Dungeon when it creates the room instance so that the room can spawn the correct enemies and update the cleared state when all enemies are defeated.
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		spawnPoint = GetNode<Marker2D>("Marker2D");

		


	}

	private void onRoomEntered(Node2D body)
	{
		GD.Print("Player Entered Boss Room!!");
		if(body is not Entity entity || hasSpawned) return;

		hasSpawned = true;

		GD.Print("Spawning Boss!!");
		var room = RoomData ?? GameManager.Instance.currentRoom;
		// return; // Comment this out to enable enemy spawning
		if(room.IsCleared){
			GD.Print("Room is already cleared, not spawning boss.");
			return;
		}
		CallDeferred(MethodName.SpawnBoss);
	}

	private void SpawnBoss()
	{
		var bossInstance = (Node2D)BossScene.Instantiate();
		bossInstance.Position = spawnPoint.Position;
		AddChild(bossInstance);
	}
}
