using Godot;
using System;
using System.Collections.Generic;

[Tool] 
public partial class InventoryData : Control
{
    [Export] public PackedScene InventorySlotScene = GD.Load<PackedScene>("res://Scenes/Inventory/InventorySlot.tscn");

    [Export] public GridContainer InventoryGrid;
    [Export] public ToolTip Tooltip;
    [Export] public Inventory TargetInventory;

    private List<InventorySlot> _slots = new List<InventorySlot>();

    public static Item SelectedItem = null;

   public override void _Ready()
    {
        if (TargetInventory == null)
        {
            TargetInventory = GetNodeOrNull<Inventory>("/root/Dungeon/Player/InventoryController");

            if (TargetInventory == null)
            {
                TargetInventory = GetParent().GetNodeOrNull<Inventory>("../Player/InventoryController");
            }
        }

        if (TargetInventory != null)
        {
            TargetInventory.InventoryChanged += RefreshUI;
            InitializeGrid();
            RefreshUI();
        }
        else
        {
            GD.PrintErr("CRITICAL: InventoryUI could not find the Player's InventoryController");
        }
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
            GD.Print($"Created slot {i}");

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

    public override void _Input(InputEvent @event)
    {
        // Make sure "toggle_inventory" is defined in your Project Settings -> Input Map
        if (@event.IsActionPressed("Inventory"))
        {
            Visible = !Visible;
            // Handle the mouse cursor
            if (Visible)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
                RefreshUI(); // Update slots just in case
            }
            else
            {
                // If it's a top-down/action game, you might want to capture the mouse again
                // Input.MouseMode = Input.MouseModeEnum.Captured; 
                if (Tooltip != null) Tooltip.Visible = false;
            }
        }
    }

    private void CreateVisualSlots()
    {
        // Clear old slots
        foreach (Node child in InventoryGrid.GetChildren()) child.QueueFree();
        _slots.Clear();

        // Loop through how many slots we are *supposed* to have
        // (We get this from TargetInventory.Rows * TargetInventory.Cols)
        for (int i = 0; i < (TargetInventory.Rows * TargetInventory.Cols); i++)
        {
            // 1. Instantiate the slot scene
            var slot = InventorySlotScene.Instantiate<InventorySlot>();

            // 2. Add it to the visual GridContainer
            InventoryGrid.AddChild(slot);

            // 3. Keep track of it in our list
            _slots.Add(slot);
        }
    }
}