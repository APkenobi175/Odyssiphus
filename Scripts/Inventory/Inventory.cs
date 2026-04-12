using Godot;
using System;
using System.Collections.Generic;

public partial class Inventory : Node
{
    [Export] public InventoryItem[] Items;

    [Export] public int Rows = 3;
    [Export] public int Cols = 6;

    [Signal] public delegate void InventoryChangedEventHandler();

    public override void _Ready()
    {
        Items = new InventoryItem[Rows * Cols];
    }


	public void AddItem(Item worldItem, int amount)
	{
	    if (worldItem.IsStackable)
	    {
	        foreach (var existingItem in Items)
	        {
	            if (existingItem != null && existingItem.ItemName == worldItem.ItemName)
	            {
	                existingItem.Amount += amount;
	                worldItem.QueueFree(); 
	                EmitSignal(SignalName.InventoryChanged);
	                return;
	            }
	        }
	    }
	
	    for (int i = 0; i < Items.Length; i++)
	    {
	        if (Items[i] == null)
	        {
	            if (worldItem is InventoryItem invItem)
	            {
	                Items[i] = invItem;
	                invItem.Amount = amount;
	                invItem.GetParent()?.RemoveChild(invItem); 
	            }
	
	            EmitSignal(SignalName.InventoryChanged);
	            return;
	        }
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