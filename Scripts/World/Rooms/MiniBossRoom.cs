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

	public AnimationPlayer roomClearedAnimation;
	public Label roomClearLabel;
	public override void _Ready()
	{

		// Make room clear label invisible
		roomClearLabel = GetNode<Label>("Label2");
		roomClearLabel.Visible = false;

		roomClearedAnimation = GetNode<AnimationPlayer>("AnimationPlayer");


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
			//2 second delay before the mini boss spawns to give the player a moment to prepare
			await WaitFor(2);
			spawnEnemy(miniBossScene);
		}
	}

	private PackedScene pickMiniBossToSpawn()
	{

		int miniBossesDefeated = GameManager.Instance.MiniBossesDeafted;

		switch (miniBossesDefeated)
		{
			case 0:
				roomClearLabel.Text = "Polyphemus Deafeted! (Posiedon Will Be PISSED)";
				return Cyclops;
			case 1:
				roomClearLabel.Text = "Large Slime Defeated! (One Step Closer To Penelope!)";
				return Sirens;
			case 2:
				roomClearLabel.Text = "Scylla Defeated! (Maybe You Can Finally Get Home Now?)";
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

	public float clearRoomDelay = 5f;
	private const float ClearCheckTime = 1.5f;

	public override void _Process(double delta)
	{
		if (!hasSpawned) return;
		if (RoomData == null) return;
		if (RoomData.IsCleared) return; // If room is already cleared, no need to check for enemies

		bool hasLivingEnemies = GetTree().GetNodesInGroup("Enemies").Count > 0;

		if (hasLivingEnemies)
		{
			clearRoomDelay = ClearCheckTime; // Start the clear delay timer
			return;
		}

		clearRoomDelay -= (float)delta;
		if (clearRoomDelay <= 0f)
		{
			GameManager.Instance.OnRoomCleared(RoomData.Position);

			GD.Print("Mini Boss Room Clearred!!");

			// Play animation room cleared!
			roomClearedAnimation.Play("Room Cleared!");
		}
	}

}