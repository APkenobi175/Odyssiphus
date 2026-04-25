using Godot;
using System;

public partial class InventoryItem : Item
{
	[Export] public int Amount = 0;
	[Export] Sprite2D ItemSprite;
	[Export] Label SpriteLabel;
	protected bool _OffPickupCooldown = true;

	public void SetData(string name, Texture2D icon, bool IsStackable, int amount)
	{
		this.ItemName = name;
		this.Name = name;
		this.icon = icon;
		this.IsStackable = IsStackable;
		this.Amount = amount;
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (ItemSprite != null && icon != null)
		{
			ItemSprite.Texture = icon;
			SetSpriteSizeTo(ItemSprite, new Vector2(42, 42));
		}
		if (SpriteLabel != null)
		{
			SpriteLabel.Text = Amount.ToString();
			SpriteLabel.Visible = true;
		}
		else
		{
			if (SpriteLabel != null)
			{
				SpriteLabel.Visible = false;
			}
		}
	}

	public void SetSpriteSizeTo(Sprite2D sprite, Vector2 size)
	{
		if (sprite.Texture == null) return;

		Vector2 textureSize = sprite.Texture.GetSize();
		Vector2 scaleFactor = new Vector2(size.X / textureSize.X, size.Y / textureSize.Y);
		sprite.Scale = scaleFactor;
	}

	public void Fade()
	{
		Color fadeColor = new Color(1, 1, 1, 0.4f);
		ItemSprite.Modulate = fadeColor;
		SpriteLabel.Modulate = fadeColor;
	}

	public async void StartDropCooldown(float duration)
	{
	    _OffPickupCooldown = false;
	    // Optional: make it slightly transparent to show it's "cooling down"
	    Modulate = new Color(1, 1, 1, 0.5f); 
	
	    await ToSignal(GetTree().CreateTimer(duration), "timeout");
	
	    _OffPickupCooldown = true;
	    Modulate = new Color(1, 1, 1, 1f); // Back to normal
	}

}
