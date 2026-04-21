using Godot;
using System;
using System.Collections.Generic;

public partial class Inventory : Node
{
    [Export] public InventoryItem[] Items;

    [Export] public int Rows = 4;
    [Export] public int Cols = 5;

    [Signal] public delegate void InventoryChangedEventHandler();

    public override void _Ready()
    {
        Items = new InventoryItem[Rows * Cols];
    }


	public void AddItem(Item worldItem, int amount)
	{
	    bool itemAdded = false;
	
	    // 1. Handle Stacking
	    if (worldItem.IsStackable)
	    {
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

	
}