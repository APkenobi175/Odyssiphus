using Godot;
using System;

public partial class MirrorOfIaso : InventoryItem
{
	[Export] public float HealthBoost = 100.0f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		ItemName = "Iaso Blessed Mirror";
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

	// Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void ApplyPassive(CharacterBody2D player)
    {
        if (player is Entity)
		{
			var PlayerHealth = player.GetNode<Health>("Health");
			PlayerHealth.MaxHealth += HealthBoost;
			PlayerHealth.ChangeHealth(HealthBoost);
			GD.Print($"Gazing into the mirror boosts vitality. Max health is now {PlayerHealth.MaxHealth} points.");
		}
    }
}
