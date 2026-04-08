using Godot;
using System;

public partial class ToolTip : ColorRect
{
    private MarginContainer _marginContainer;
    private Label _itemNameLabel;

    public override void _Ready()
    {
        // Equivalent to @onready: assigning the nodes when the script starts
        _marginContainer = GetNode<MarginContainer>("MarginContainer");
        _itemNameLabel = GetNode<Label>("MarginContainer/Label");
    }

    public void SetText(string text)
    {
        if (_itemNameLabel != null)
        {
            _itemNameLabel.Text = text;
        }

        if (_marginContainer != null)
        {
            // Resetting size to Vector2.Zero forces the container 
            // to shrink-wrap to the new text size immediately.
            _marginContainer.Size = Vector2.Zero;
            this.Size = _marginContainer.Size;
        }
    }
}