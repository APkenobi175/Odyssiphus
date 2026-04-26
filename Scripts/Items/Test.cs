using Godot;
using System;


public partial class Test : InventoryItem
{
    public override void _Ready()
    {
        ItemName = "Test";
        BodyEntered += (body) =>
        {
			GD.Print($"Touched by: {body.Name}");
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
                    GD.Print("Item pickup scheduled...");
                }
            }
        };
    }
}
