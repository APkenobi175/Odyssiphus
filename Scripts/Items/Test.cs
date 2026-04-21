using Godot;
using System;


public partial class Test : InventoryItem
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	    BodyEntered += (body) =>
	    {
	        if (body.Name == "Player")
	        {
	            var inv = body.GetNodeOrNull<Inventory>("InventoryController");
	            if (inv != null)
	            {
	                // We use CallDeferred to "schedule" the pickup for the next idle frame
	                // This gets us out of the Physics Callback safely.
	                inv.CallDeferred("AddItem", this, 1);
	                GD.Print("Item pickup scheduled...");
	            }
	        }
	    };
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
