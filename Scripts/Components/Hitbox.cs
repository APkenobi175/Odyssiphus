using Godot;
using System;

public partial class Hitbox : Area2D, IFactionable
{
  [Export]
  public float Damage = 10;
  [Export]
  public FactionManager.Faction Faction = FactionManager.Faction.Neutral;

  public override void _Ready()
  {
    SetFaction(Faction);
    AreaEntered += OnAreaEntered;
  }

  public void SetFaction(FactionManager.Faction faction)
  {
    Faction = faction;
    CollisionLayer = 0;
    CollisionMask = FactionManager.GetHitboxMask(faction, false);
  }

  public void OnAreaEntered(Node2D body)
  {
    if (body is Hurtbox hurtbox)
    {
      HurtboxHit?.Invoke(hurtbox);

      hurtbox.Health?.ChangeHealth(-Damage);
    }
  }

  public event Action<Hurtbox> HurtboxHit;
}
