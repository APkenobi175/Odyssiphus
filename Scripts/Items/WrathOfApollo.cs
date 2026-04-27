using Godot;
using System;

public partial class WrathOfApollo : InventoryItem
{
	[Export]
	public int SpreadIncrease = 1;
	[Export]
	public float AngleIncrease = 10.0f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{		
		ItemName = "Wrath Of Apollo";
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
			var PlayerSpecial = player.GetNode<BaseProjectileAbility>("Special");
			PlayerSpecial.SpreadCount += SpreadIncrease;
			PlayerSpecial.SpreadAngle += AngleIncrease;
			GD.Print($"A relic of Apollo blesses your bow. You shoot {PlayerSpecial.SpreadCount} arrows now.");
		}
    }
}
