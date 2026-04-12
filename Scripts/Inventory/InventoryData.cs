using Godot;
using System;
using System.Collections.Generic;

[Tool] // Equivalent to @tool
public partial class InventoryData : Control
{
    private PackedScene _inventoryItemScene = GD.Load<PackedScene>("res://Inventory/InventorySlot/InventoryItem/InventoryItem.tscn");

    [Export] public GridContainer InventoryGrid;
    [Export] public PackedScene InventorySlotScene;
    [Export] public ToolTip Tooltip;
    [Export] public Inventory TargetInventory;

    private List<InventorySlot> _slots = new List<InventorySlot>();

    public static Item SelectedItem = null;

    public override void _Ready()
    {
        if (TargetInventory == null)
        {
            GD.PrintErr("InventoryData: TargetInventory is not assigned!");
            return;
        }

        TargetInventory.InventoryChanged += RefreshUI;

        InitializeGrid();
        RefreshUI();
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
            
            if (SelectedItem is Node2D node)
            {
                node.GlobalPosition = GetGlobalMousePosition();
            }
        }
    }

    private void OnSlotInput(InventorySlot which, InventorySlot.inventorySlotAction action)
    {
        int index = _slots.IndexOf(which);

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

            TargetInventory.Items[index] = which.item;
        }
        else
        {
            SelectedItem = which.DeselectItem((InventoryItem)SelectedItem);

            TargetInventory.Items[index] = which.item;
        }
    
        TargetInventory.EmitSignal(Inventory.SignalName.InventoryChanged);
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

    private void InitializeGrid()
    {
        foreach (Node child in InventoryGrid.GetChildren()) child.QueueFree();
        _slots.Clear();

        InventoryGrid.Columns = TargetInventory.Cols;

        for (int i = 0; i < (TargetInventory.Rows * TargetInventory.Cols); i++)
        {
            var slot = InventorySlotScene.Instantiate<InventorySlot>();
            _slots.Add(slot);
            InventoryGrid.AddChild(slot);

            slot.SlotInput += OnSlotInput;
            slot.SlotHovered += OnSlotHovered;
        }
    }

    public void RefreshUI()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (i < TargetInventory.Items.Length)
            {
                _slots[i].item = TargetInventory.Items[i];
                _slots[i].UpdateSlot();
            }
        }
    }
}