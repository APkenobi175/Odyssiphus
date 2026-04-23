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
    [Export] public Sprite2D GrabbedItemSprite;
    [Export] public Label GrabbedAmountLabel;

    private List<InventorySlot> _slots = new List<InventorySlot>();

    public static InventoryItem SelectedItem = null;

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

        if (SelectedItem != null)
        {
            if (GrabbedItemSprite != null)
            {
                GrabbedItemSprite.Visible = true;
                GrabbedItemSprite.GlobalPosition = GetGlobalMousePosition();
                GrabbedItemSprite.Scale = new Vector2(42f / SelectedItem.icon.GetSize().X, 42f / SelectedItem.icon.GetSize().Y);
            }
        }
        else
        {
            if (GrabbedItemSprite != null)
            {
                GrabbedItemSprite.Visible = false;
            }
        }
    }

    private void OnSlotInput(InventorySlot slot, InventorySlot.inventorySlotAction action)
    {
        // CASE A: We are holding nothing, and clicking a slot with an item
        if (SelectedItem == null && !slot.IsEmpty())
        {
            SelectedItem = slot.SelectItem(); // This removes it from the slot and gives it to us
            GD.Print($"Picked up {SelectedItem.ItemName}");
        }

        // CASE B: We are holding an item, and clicking an empty (or different) slot
        else if (SelectedItem != null)
        {
            // DeselectItem handles placing it in the slot or swapping if something is already there
            SelectedItem = slot.DeselectItem(SelectedItem);

            // If SelectedItem is now null, we dropped it successfully.
            // If it's NOT null, it means we swapped and are now holding the old item.
            GD.Print(SelectedItem == null ? "Dropped item" : $"Swapped for {SelectedItem.ItemName}");
        }

        // Finally, tell the Inventory Data logic that things moved so it stays in sync
        // RefreshUI(); // Optional: Usually DeselectItem/SelectItem handle the visuals locally
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
            GD.Print("Tab was pressed, opening inventory!");
            Visible = !Visible;
            // Handle the mouse cursor
            if (Visible)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
                RefreshUI(); // Update slots just in case
                GD.Print($"Inventory Pos: {GlobalPosition} | Size: {Size} | Scale: {Scale}");
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