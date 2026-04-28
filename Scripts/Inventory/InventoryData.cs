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
        // Now that you added it to the Enum, this should work!
        if (action != InventorySlot.inventorySlotAction.LeftClick) return;

        // This is the "Muzzle" for the sword. 
        // It tells Godot: "I handled this event, stop passing it around."
        GetViewport().SetInputAsHandled(); 

        if (SelectedItem == null && !slot.IsEmpty())
        {
            SelectedItem = slot.SelectItem();
            GD.Print($"Picked up {SelectedItem.ItemName}");
        }
        else if (SelectedItem != null)
        {
            SelectedItem = slot.DeselectItem(SelectedItem);
            GD.Print(SelectedItem == null ? "Dropped item" : $"Swapped for {SelectedItem.ItemName}");
        }
        for (int i = 0; i < _slots.Count; i++)
        {
            TargetInventory.Items[i] = _slots[i].item;
        }
    }

    private void OnSlotHovered(InventorySlot which, bool isHovering)
    {
        if (Tooltip == null) return;
        GD.Print($"Hovering over {which.item.ItemName}");

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
            // GD.Print($"Created slot {i}");

            slot.SlotInput += OnSlotInput;
            slot.SlotHovered += OnSlotHovered;
            GD.Print($"slot {i} signal connected");
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
        if (!Visible && SelectedItem != null)
        {
           TargetInventory.AddItem(SelectedItem, SelectedItem.Amount);
        }
        // Make sure "toggle_inventory" is defined in your Project Settings -> Input Map
        if (@event.IsActionPressed("Inventory"))
        {
            Visible = !Visible;
            // Handle the mouse cursor
            if (Visible)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
                this.GrabFocus();
                RefreshUI(); // Update slots just in case
            }
            else
            {
                // Puts item in the first available slot if the inventory is closed without
                // putting the item in a slot.
                if (SelectedItem != null)
                {
                    ReturnItemToFirstAvailableSlot();
                }
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

    private void ReturnItemToFirstAvailableSlot()
    {
        if (SelectedItem == null) return;

        foreach (var slot in _slots)
        {
            if (slot.IsEmpty())
            {
                // 1. Hand the item to the slot data
                slot.item = SelectedItem; 
                
                // 2. IMPORTANT: Manually call UpdateSlot so the item is 
                // re-enabled and positioned correctly in its new home
                slot.UpdateSlot();

                // 3. Sync the backend data array
                int index = _slots.IndexOf(slot);
                if (index != -1)
                {
                    TargetInventory.Items[index] = slot.item;
                }

                // 4. Clear the "Grabbed" state
                SelectedItem = null;
                if (GrabbedItemSprite != null) 
                {
                    GrabbedItemSprite.Visible = false;
                    GrabbedItemSprite.Texture = null; // Clear the ghost texture
                }
                
                GD.Print("Item safely returned and reactivated.");
                return;
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // If we click the mouse AND the inventory is open AND we are holding an item...
        if (Visible && SelectedItem != null && @event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                // If the click reaches here, it means we didn't click a slot.
                // Drop the item!
                DropItem();
            }
        }
    }

   private void DropItem()
    {
        if (SelectedItem == null) return;

        var player = GetTree().GetFirstNodeInGroup("PlayerFaction") as CharacterBody2D;
        var inv = player?.GetNodeOrNull<Inventory>("InventoryController");
        if (player == null) return;
    
        var world = GetTree().CurrentScene;
        SelectedItem.Reparent(world);
    
        
        SelectedItem.SetDeferred("monitoring", true);
        SelectedItem.SetDeferred("monitorable", true);
        
        
        SelectedItem.ProcessMode = ProcessModeEnum.Inherit; 
        
    
        SelectedItem.Visible = true;
    
        if (player != null)
        {
            SelectedItem.GlobalPosition = player.GlobalPosition + new Vector2(30, 0);
        }

        if (inv != null)
        {
            GD.Print("Inventory is not null");
            inv.RemoveItem(SelectedItem);
        }
    
        if (SelectedItem.HasMethod("StartDropCooldown"))
        {
            SelectedItem.Call("StartDropCooldown", 1.5f);
        }
    
        SelectedItem = null;
        if (GrabbedItemSprite != null) GrabbedItemSprite.Visible = false;
    
        GD.Print("Item dropped successfully!");
    }
    
    

}