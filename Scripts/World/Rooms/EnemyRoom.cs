using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class EnemyRoom : Node2D
{

	[Export]
	public PackedScene EnemyScene { get; set; }
	private bool hasSpawned = false;
	private List<Marker2D> spawnPoints = new();

	Node2D enemyContainer; 
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		// Create my array of spawn points for enemies

		enemyContainer = GetNode<Node2D>("SpawnPoints");

		foreach(Node child in enemyContainer.GetChildren())
		{
			if (child is Marker2D marker)
			{
				spawnPoints.Add(marker);
			}
		}



		
		// Get current room from GameManager
		var room = GameManager.Instance.currentRoom;

		// Check hallways and set door visibility accordingly to only show doors that actually exist for this room
		
		// hide doors that don't exist
		GetNode<Node2D>("Doors/SouthDoor").Visible = room.Doors.Contains("N");
		GetNode<Node2D>("Doors/NorthDoor").Visible = room.Doors.Contains("S");
		GetNode<Node2D>("Doors/EastDoor").Visible = room.Doors.Contains("W");
		GetNode<Node2D>("Doors/WestDoor").Visible = room.Doors.Contains("E");

	}

	private void onRoomEntered(Node2D body)
	{
		GD.Print("Player Entered Enemy Room!!");
		if(body is not Entity entity || hasSpawned) return;
		hasSpawned = true;

		GD.Print("Spawning Enemies!!");
		CallDeferred(MethodName.SpawnEnemies);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void SpawnEnemies()
	{

		
		float depth = GameManager.Instance.currentRoom.Depth;
		int count = GetEnemyCount(depth);
		List<Marker2D> shuffled = spawnPoints.OrderBy(x => GD.Randf()).ToList();

		GD.Print($"Spawning {count} enemies at depth {depth} with {spawnPoints.Count} spawn points");
		for (int i = 0; i< count && i < shuffled.Count; i++)
		{
			Entity enemy = EnemyScene.Instantiate<Entity>();
			AddChild(enemy);
			enemy.GlobalPosition = shuffled[i].GlobalPosition;
		}
	}

	private int GetEnemyCount(float depth)
	{
		// We are going to spawn a number of enemies based on how far in to the dungeon we are

		float[] weights = new float[]
		{
			Mathf.Lerp(30f, 2f, depth), // 1 Enemy, very likely early, very rare late
			Mathf.Lerp(25f, 5f, depth), // 2 Enemies, likely early, rare late
			Mathf.Lerp(20f, 10f, depth), // 3 Enemies, somewhat likely early, somewhat rare late
			Mathf.Lerp(15f, 20f, depth), // 4 Enemies, somewhat likely early, somewhat rare late
			Mathf.Lerp(7f, 30f, depth), // 5 Enemies, somewhat rare early, somewhat likely late
			Mathf.Lerp(3f, 33f, depth), // 6 Enemies, rare early, likely late
		};


		// Pick random weight in the total weight

		float total = weights.Sum();
		float roll = GD.Randf() * total;

		// iterate through weights and subtract from roll until we find the number of enemies to spawn
		float cumulative = 0f;
		for (int i = 0; i< weights.Length; i++)
		{
			cumulative += weights[i];
			if (roll <= cumulative)
			{
				return i+1; // +1 because we want to spawn at least 1 enemy
			}
		}

		return 6; // fallback to max enemies if something goes wrong with the weights



	}
}
