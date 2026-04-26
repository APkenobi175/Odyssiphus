using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class EnemyRoom : Node2D
{


	[Export]
	public PackedScene SlimeSmall { get; set; }
	[Export]
	public PackedScene BasicMeleeEnemy { get; set; }
	[Export]
	public PackedScene RangedEnemy { get; set; }
	[Export]
	public PackedScene ExplodingEnemy { get; set; }
	[Export]
	public PackedScene SlimeMedium { get; set; }

	public List<PackedScene> EnemySceneList => new List<PackedScene> {SlimeMedium, SlimeSmall, BasicMeleeEnemy, RangedEnemy, ExplodingEnemy };
	private bool hasSpawned = false;
	private List<Marker2D> spawnPoints = new();

	Node2D enemyContainer; 

	private Label roomClearLabel;
	private AnimationPlayer roomClearedAnimation;

	public RandomWalkRoom RoomData { get; set; } // This will be set by the Dungeon when it creates the room instance so that the room can spawn the correct enemies and update the cleared state when all enemies are defeated.
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		// Make room clear label invisible
		roomClearLabel = GetNode<Label>("Label2");
		roomClearLabel.Visible = false;

		// Get node reference to room cleared animation player
		roomClearedAnimation = GetNode<AnimationPlayer>("AnimationPlayer");

		// APPLY SHADER TO TILEMAP and set the DEPTH PAREMETER
		var room = RoomData;
		float depth = room.Depth;
		var tilemap = GetNode<TileMapLayer>("TileMapLayer");
		var material = (ShaderMaterial)tilemap.Material.Duplicate(); // Added duplicate so that every room has its own.
		tilemap.Material = material;
		material.SetShaderParameter("depth", depth);
		GD.Print($"Setting shader depth to {depth} for room {room.Position}");

		// Create my array of spawn points for enemies

		enemyContainer = GetNode<Node2D>("SpawnPoints");

		foreach(Node child in enemyContainer.GetChildren())
		{
			if (child is Marker2D marker)
			{
				spawnPoints.Add(marker);
			}
		}

		// Check hallways and set door visibility accordingly to only show doors that actually exist for this room
		
		// hide doors that don't exist
		GetNode<Node2D>("Doors/SouthDoor").Visible = room.Doors.Contains("N");
		GetNode<Node2D>("Doors/NorthDoor").Visible = room.Doors.Contains("S");
		GetNode<Node2D>("Doors/EastDoor").Visible = room.Doors.Contains("W");
		GetNode<Node2D>("Doors/WestDoor").Visible = room.Doors.Contains("E");

	}

	private async void onRoomEntered(Node2D body)
	{

		GD.Print("Player Entered Enemy Room!!");
		if(body is not Entity entity || hasSpawned) return;

		hasSpawned = true;

		GD.Print("Spawning Enemies!!");
		var room = RoomData ?? GameManager.Instance.currentRoom;
		// return; // Comment this out to enable enemy spawning
		if(room.IsCleared){
			GD.Print("Room is already cleared, not spawning enemies.");
			return;
		}
		// 2 second delay
		await WaitFor(2);
		CallDeferred(MethodName.SpawnEnemies);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.

	public float clearRoomDelay = 5f;
	private const float ClearCheckTime = 1.5f;

	public override void _Process(double delta)
	{


		// Check the enemy container, if the container it means all the enemies are dead and we can clear the room
		if (!hasSpawned) return;
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
			GD.Print("Enemies Clearred!!");

			// Play animation room cleared!
			roomClearedAnimation.Play("Room Cleared!");
		}



	}

	private void SpawnEnemies()
	{

		
		float depth = RoomData?.Depth ?? GameManager.Instance.currentRoom.Depth;
		int count = GetEnemyCount(depth);
		List<Marker2D> shuffled = spawnPoints.OrderBy(x => GD.Randf()).ToList();

		GD.Print($"Spawning {count} enemies at depth {depth} with {spawnPoints.Count} spawn points");
		for (int i = 0; i< count && i < shuffled.Count; i++)
		{
			// Pick what type of enemy to spawn
			PackedScene enemyType = getEnemyType(depth);
			Entity enemy = enemyType.Instantiate<Entity>();
			AddChild(enemy);
			enemy.GlobalPosition = shuffled[i].GlobalPosition;
		}
	}

	private PackedScene getEnemyType(float depth)
	{
		// We are going to spawn different types of enemies based on how far in to the dungeon we are

		float[] weights = new float[]
		{
			Mathf.Lerp(2f, 25f, depth),  // Slime Medium - rare early, common late
			Mathf.Lerp(35f, 15f, depth), // Slime Small - very common early, less late
			Mathf.Lerp(15f, 30f, depth), // Basic Melee Enemy
			Mathf.Lerp(7f, 25f, depth),  // Ranged Enemy
			Mathf.Lerp(3f, 25f, depth),  // Exploding Enemy
		};

		// Pick random weight in the total weight
		float total = weights.Sum();
		float roll = GD.Randf() * total;

		// iterate through weights and subtract from roll until we find the enemy type to spawn
		float cumulative = 0f;
		for (int i = 0; i< weights.Length; i++)
		{
			cumulative += weights[i];
			if (roll <= cumulative)			{
				return EnemySceneList[i];
			}
		}

		return EnemySceneList[0]; // fallback to first enemy type if something goes wrong
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

	private async Task WaitFor(float seconds)
	{
		await ToSignal(GetTree().CreateTimer(seconds), "timeout");
	}

}
