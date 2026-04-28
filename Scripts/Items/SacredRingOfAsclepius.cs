using Godot;
using System;

public partial class SacredRingOfAsclepius : InventoryItem
{
		[Export] public int RegenRate = 1;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//ItemName = "A Sacred Ring of Asclepius";
		ItemName = "AsclepiusRing";
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
        if (player is Entity)
		{
			var PlayerHealth = player.GetNode<Health>("Health");
			PlayerHealth.HealthRegenRate += RegenRate;
			GD.Print($"Blessing of Asclepius applied. Health regeneration is {PlayerHealth.HealthRegenRate} points per second.");
		}
    }

}


