using Godot;
using System;

public partial class InventorySlot : CenterContainer
{
	[Export] PackedScene InventoryItemScene = GD.Load<PackedScene>("res://Scenes/Inventory/InventoryItem.tscn");
	[Export] public InventoryItem item;
	[Export] public InventoryItem ItemHint = null;

	public enum inventorySlotAction
	{
		Select,
		Split,
		LeftClick,
	}

	[Signal] public delegate void SlotInputEventHandler(InventorySlot which, inventorySlotAction action);
	[Signal] public delegate void SlotHoveredEventHandler(InventorySlot which, bool isHovering);

	public override void _Ready()
	{
		AddToGroup("InventorySlots");
	}

	public void OnTextureButtonGUIInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Left)
			{
				EmitSignal(SignalName.SlotInput, this, (int)inventorySlotAction.LeftClick);
			}
			else if (mouseEvent.ButtonIndex == MouseButton.Right)
			{
				EmitSignal(SignalName.SlotInput, this, (int)inventorySlotAction.Split);
			}
		}
	}

	public void OnTextureButtonMouseEntered()
	{
		EmitSignal(SignalName.SlotHovered, this, true);
	}

	public void OnTextureButtonMouseExited()
	{
		EmitSignal(SignalName.SlotHovered, this, false);
	}

	public bool IsRespectingHint(InventoryItem NewItem, bool InAmmountAsWell = true)
	{
		if (ItemHint == null)
		{
			return true;
		}

		if (InAmmountAsWell)
		{
			return NewItem.ItemName == ItemHint.ItemName && NewItem.Amount >= ItemHint.Amount;
		}
		else
		{
			return NewItem.ItemName == ItemHint.ItemName;
		}
	}

	public void SetItemHint(InventoryItem NewItemHint)
	{
		if (ItemHint != null) ItemHint.QueueFree();
		
		ItemHint = NewItemHint;
		AddChild(NewItemHint);
		UpdateSlot();
	}

	public void ClearItemHint()
	{
		if (ItemHint != null)
		{
			ItemHint.QueueFree();
		}
		ItemHint = null;
		UpdateSlot();
	}

	public void RemoveItem()
	{
		if (item != null)
		{
			RemoveChild(item);
			item.QueueFree();
			item = null;
		}
		UpdateSlot();
	}

	public InventoryItem SelectItem()
	{
		Node inventory = GetParent().GetParent();
		InventoryItem TempItem = item;

		if (TempItem != null)
		{
			TempItem.Reparent(inventory);
			item = null;
			TempItem.ZIndex = 128;
		}
		return TempItem;
	}

	public InventoryItem DeselectItem(InventoryItem NewItem)
	{
		if (!IsRespectingHint(NewItem)) return NewItem;
		if (IsEmpty())
		{
			item = NewItem;
			UpdateSlot();
			return null;
		}
		else
		{
			if (HasSameItem(NewItem) && item.IsStackable)
			{
				item.Amount += NewItem.Amount;
				NewItem.QueueFree();
				return null;
			}
			else
			{
				InventoryItem oldItem = item;
				item = NewItem;

				UpdateSlot();

				return oldItem;
			}
		}	
	}

	public InventoryItem SplitItem()
	{
		if (IsEmpty()) return null;

		Node inventory = GetParent().GetParent();

		if (item.Amount > 1)
		{
			InventoryItem NewItem = InventoryItemScene.Instantiate<InventoryItem>();

			NewItem.SetData(item.ItemName, item.icon, item.IsStackable, item.Amount);

			NewItem.Amount = item.Amount / 2;
			item.Amount -= NewItem.Amount;

			inventory.AddChild(NewItem);
			NewItem.ZIndex = 128;
			return NewItem;
		}
		else if (item.Amount == 1)
		{
			return SelectItem();
		}
		return null;
	}

	public bool IsEmpty() => item == null;

	public bool HasSameItem(InventoryItem otherItem)
	{
		if (item == null || otherItem == null) return false;
		return otherItem.ItemName == item.ItemName;
	}

	public void UpdateSlot()
	{
	    if (item != null)
	    {
	        // 1. Safe Disable (using SetDeferred)
	        item.SetDeferred("process_mode", (int)ProcessModeEnum.Disabled);
	        item.SetDeferred("monitoring", false);
	        item.SetDeferred("monitorable", false);
	
	        // 2. Safe Reparenting
	        if (item.GetParent() != this)
	        {
	            // If it has a parent, remove it safely
	            item.GetParent()?.CallDeferred("remove_child", item);
	            // Add it to this slot safely
	            CallDeferred("add_child", item);
	        }
	
	        // 3. Safe Positioning
	        item.SetDeferred("position", Vector2.Zero);
	
	        if (item.Amount < 1) item.Fade();
	    }
	}
}
