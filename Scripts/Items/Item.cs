using Godot;
using System;

public partial class Item : Area2D
{
	[Export] public string ItemName = "";
	[Export] public Texture2D icon;
	// All items remain unstackable for right now.
	[Export] public bool IsStackable = false;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("items");
	}

}
