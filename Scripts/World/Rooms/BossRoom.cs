using Godot;
using System;

public partial class BossRoom : Node2D
{

	public RandomWalkRoom RoomData { get; set; } // This will be set by the Dungeon when it creates the room instance so that the room can spawn the correct enemies and update the cleared state when all enemies are defeated.
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{


	}
}
