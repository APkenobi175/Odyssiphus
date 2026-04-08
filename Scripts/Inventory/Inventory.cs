using Godot;
using System;
using System.Collections.Generic;

[Tool] // Equivalent to @tool
public partial class Inventory : Control
{
    private PackedScene _inventoryItemScene = GD.Load<PackedScene>("res://Inventory/InventorySlot/InventoryItem/InventoryItem.tscn");

    [Export] public int Rows = 3;
    [Export] public int Cols = 6;
    [Export] public GridContainer InventoryGrid;
    [Export] public PackedScene InventorySlotScene;
    [Export] public ToolTip Tooltip;

    // Use List for Array[InventorySlot]
    private List<InventorySlot> _slots = new List<InventorySlot>();

    // Static variables work the same as static var
    public static Item SelectedItem = null;

    public override void _Ready()
    {
        if (InventoryGrid == null) return;

        InventoryGrid.Columns = Cols;

        for (int i = 0; i < (Rows * Cols); i++)
        {
            var slot = InventorySlotScene.Instantiate<InventorySlot>();
            _slots.Add(slot);
            InventoryGrid.AddChild(slot);

            // Connecting signals in C# 
            slot.SlotInput += OnSlotInput;
            slot.SlotHovered += OnSlotHovered;
        }

        if (Tooltip != null)
            Tooltip.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (Tooltip != null)
        {
            Tooltip.GlobalPosition = GetGlobalMousePosition() + Vector2.One * 8;
        }

        if (SelectedItem != null)
        {
            if (Tooltip != null) Tooltip.Visible = false;
            
            // Note: If SelectedItem is a Node2D/Area2D, it has GlobalPosition
            if (SelectedItem is Node2D node)
            {
                node.GlobalPosition = GetGlobalMousePosition();
            }
        }
    }

    private void OnSlotInput(InventorySlot which, InventorySlot.inventorySlotAction action)
    {
        GD.Print(action);

        if (SelectedItem == null)
        {
            if (action == InventorySlot.inventorySlotAction.Select)
            {
                SelectedItem = which.SelectItem();
            }
            else if (action == InventorySlot.inventorySlotAction.Split)
            {
                SelectedItem = which.SplitItem();
            }
        }
        else
        {
            SelectedItem = which.DeselectItem((InventoryItem)SelectedItem);
        }
    }

    private void OnSlotHovered(InventorySlot which, bool isHovering)
    {
        if (Tooltip == null) return;

        if (which.item != null)
        {
            Tooltip.SetText(which.item.ItemName);
            Tooltip.Visible = isHovering;
        }
        else if (which.ItemHint != null)
        {
            Tooltip.SetText(which.ItemHint.ItemName);
            Tooltip.Visible = isHovering;
        }
        else
        {
            Tooltip.Visible = false;
        }
    }

    // --- API SECTION ---

    public void AddItem(Item item, int amount)
    {
        InventoryItem newItem = _inventoryItemScene.Instantiate<InventoryItem>();
        newItem.SetData(item.ItemName, item.icon, item.IsStackable, amount);
        item.QueueFree();

        if (newItem.IsStackable)
        {
            foreach (var slot in _slots)
            {
                if (slot.item != null && slot.item.ItemName == newItem.ItemName)
                {
                    slot.item.Amount += newItem.Amount;
                    return;
                }
            }
        }

        foreach (var slot in _slots)
        {
            if (slot.item == null && slot.IsRespectingHint(newItem))
            {
                slot.item = newItem;
                slot.UpdateSlot();
                return;
            }
        }
    }

    public Item RetrieveItem(string itemName)
    {
        foreach (var slot in _slots)
        {
            if (slot.item != null && slot.item.ItemName == itemName)
            {
                // To create a new "Item" instance in C#
                Item copyItem = new Item(); 
                copyItem.ItemName = slot.item.ItemName;
                copyItem.Name = copyItem.ItemName;
                copyItem.icon = slot.item.icon;
                copyItem.IsStackable = slot.item.IsStackable;

                if (slot.item.Amount > 1)
                {
                    slot.item.Amount -= 1;
                }
                else
                {
                    slot.RemoveItem();
                }
                return copyItem;
            }
        }
        return null;
    }

    public List<Item> GetAllItems()
    {
        List<Item> items = new List<Item>();
        foreach (var slot in _slots)
        {
            if (slot.item != null) items.Add(slot.item);
        }
        return items;
    }

    public void ClearInventory()
    {
        foreach (var slot in _slots)
        {
            slot.RemoveItem();
        }
    }
}