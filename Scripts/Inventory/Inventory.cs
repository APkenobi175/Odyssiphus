using Godot;
using System;
using System.Collections.Generic;

public partial class Inventory : Node
{
    [Export] public InventoryItem[] Items;

    [Export] public int Rows = 4;
    [Export] public int Cols = 5;

    [Signal] public delegate void InventoryChangedEventHandler();
	public Entity player;
	public Node2D Attack;
	public Node PlayerHealth;
	public BaseProjectileAbility SpecialAttack;

    public override void _Ready()
    {
		player = GetParent<Entity>();
		SpecialAttack = player.GetNode<BaseProjectileAbility>("Special");
		PlayerHealth = player.GetNode<Health>("Health");
        Items = new InventoryItem[Rows * Cols];
    }


	public void AddItem(Item worldItem, int amount)
	{
	    bool itemAdded = false;
	
	    // 1. Handle Stacking
	    if (worldItem.IsStackable)
	    {
			GD.Print($"world item: {worldItem.ItemName} is stackable");
	        foreach (var existingItem in Items)
	        {
	            if (existingItem != null && existingItem.ItemName == worldItem.ItemName)
	            {
	                existingItem.Amount += amount;
	                itemAdded = true;
	                break; 
	            }
	        }
	    }
	
	    // 2. Handle New Slot
		if (!itemAdded) // Added this check so we don't add a stack to a new slot too
		{
		    for (int i = 0; i < Items.Length; i++)
		    {
		        if (Items[i] == null)
		        {
		            if (worldItem is InventoryItem invItem)
		            {
		                Items[i] = invItem;
		                invItem.Amount = amount;
		                itemAdded = true; // Mark as added!
		                break; // Exit the loop
		            }
		        }
		    }
		}
		
		// 3. Finalize
		if (itemAdded)
		{
		    // If it was a stack, we delete it. 
		    // If it's a new slot, UpdateSlot will reparent it, so we don't QueueFree it yet!
		    if (worldItem.IsStackable && worldItem.GetParent() != null && worldItem != Items[0]) 
		    {
		        // This part gets tricky. If worldItem was added to a stack, kill it.
		        // If it's the NEW item in a slot, DON'T kill it.
		    }
		
		    EmitSignal(SignalName.InventoryChanged);
			RefreshPlayerStats();
		}
	}

    public void ClearInventory()
    {
        for (int i = 0; i > Items.Length; i++)
		{
			Items[i] = null;
		}
		EmitSignal(SignalName.InventoryChanged);
    }

	public void RefreshPlayerStats()
	{
		GD.Print("Refreshing player stats");
	    if (player is Entity p) 
	    {
	        // 1. Reset Stats to base player stats
			// reset speed
	        p.Speed = 150.0f;
			// Special attack base stats
			SpecialAttack.Cooldown = 0.3f;
			SpecialAttack.SpreadCount = 1;
			SpecialAttack.SpreadAngle = 0.0f;
			SpecialAttack.BurstCount = 1;
			SpecialAttack.BurstDelay = 0.01f;
			//reset health
	        // 2. Loop through the array
	        foreach (var item in Items)
	        {
	            // We check if the slot is not empty
	            if (item != null)
	            {
					GD.Print($"{item.ItemName} is not null.");
	                // Call that virtual function we talked about!
	                item.ApplyPassive(player);
	            }
	        }
	
	        GD.Print($"Stats Refreshed. Current Speed: {p.Speed}");
	    }
	}

	public void RemoveItem(InventoryItem item)
	{
		GD.Print($"{item.ItemName} was removed");
		EmitSignal(SignalName.InventoryChanged);
		RefreshPlayerStats();
		// May not need below, delete before merge. If not deleted
		// Please yell at Grayson to remove.
		// for (int i = 0; i < Items.Length; i++)
    	// {
    	//     if (Items[i] == item)
    	//     {
    	//         Items[i] = null;
    	//         EmitSignal(SignalName.InventoryChanged);
    	//         RefreshPlayerStats(); // This is the magic line!
    	//         return;
    	//     }
    	// }
	}
}