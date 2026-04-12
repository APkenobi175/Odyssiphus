using Godot;
using System;

public partial class BaseMeleeAbility : Node2D, IAbility, IFactionable
{
  [Export]
  public Sprite2D Sprite;
  [Export]
  public AnimationPlayer AnimationPlayer;

  private Hitbox hitbox;

  public override void _Ready()
  {
    hitbox = GetNodeOrNull<Hitbox>("Hitbox");
  }

  public void Activate(Vector2 position, Vector2 direction)
  {
    GlobalRotation = direction.Angle();
    AnimationPlayer?.Play("Attack");
  }

  public void SetFaction(FactionManager.Faction faction)
  {
    hitbox?.SetFaction(faction);
  }

}
