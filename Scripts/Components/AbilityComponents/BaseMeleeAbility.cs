using Godot;
using System;

public partial class BaseMeleeAbility : Node2D, IAbility, IFactionable
{
  [Export]
  public float Cooldown = 1;
  [Export]
  public AnimationPlayer AnimationPlayer;

  private Hitbox hitbox;
  private Timer cooldownTimer;

  public override void _Ready()
  {
    hitbox = GetNodeOrNull<Hitbox>("Hitbox");

    cooldownTimer = new Timer
    {
      WaitTime = Cooldown,
      OneShot = true
    };
    AddChild(cooldownTimer);
  }

  public bool Activate(Vector2 position, Vector2 direction)
  {
    if (!cooldownTimer.IsStopped()) return false;

    GlobalRotation = direction.Angle();
    AnimationPlayer?.Play("Attack");
    cooldownTimer.Start();
    return true;
  }

  public void SetFaction(FactionManager.Faction faction)
  {
    hitbox?.SetFaction(faction);
  }

}
