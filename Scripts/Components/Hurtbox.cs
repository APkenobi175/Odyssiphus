using Godot;
using System;

public partial class Hurtbox : Area2D, IFactionable
{
  [Export]
  public Health Health;

  public override void _Ready()
  {
    SetFaction(FactionManager.Faction.Neutral);
  }

  public void SetFaction(FactionManager.Faction faction)
  {
    CollisionLayer = FactionManager.GetHurtboxLayer(faction);
    CollisionMask = 0;
  }
}
