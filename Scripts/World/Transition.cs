using Godot;
using System;

public partial class Transition : Area2D
{

	[Export]
	public string TargetScene { get; set; } = "Dungeon";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnBodyEntered(Node2D body)
	{
		if (body is CharacterBody2D)
		{
			CallDeferred(nameof(DoTransition));
		}


	}

	private void DoTransition()
	{
		GameManager.Instance.GoTo(TargetScene);
	}
}
