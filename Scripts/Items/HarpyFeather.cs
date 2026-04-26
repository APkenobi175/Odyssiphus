using Godot;
using System;

public partial class HarpyFeather : InventoryItem
{
	[Export] public float SpeedBoost = 200.0f;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ItemName = "Feather of a Harpy";
        BodyEntered += (body) =>
        {
			GD.Print($"{ItemName} Touched by: {body.Name}");
            if (!_OffPickupCooldown)
			{
				GD.Print($"Cannot be picked up, cooldown active");
				return;
			} 

            if (body.Name == "Player")
            {
                var inv = body.GetNodeOrNull<Inventory>("InventoryController");
                if (inv != null)
                {
                    inv.CallDeferred("AddItem", this, 1);
                    GD.Print($"{ItemName} pickup scheduled...");
                }
            }
        };
    
	}

    public override void ApplyPassive(CharacterBody2D player)
    {
        if (player is Entity p)
		{
			p.Speed += SpeedBoost;
			GD.Print($"Feather of a harpy applied. New speed is {p.Speed}");
		}
    }

}
