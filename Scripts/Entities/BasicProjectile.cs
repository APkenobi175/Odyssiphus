using Godot;
using System;

public partial class BasicProjectile : Node2D, IProjectile, IFactionable
{
  [Export]
  public float Speed = 1000;
  [Export]
  public float LifeDuration = 1;
  private Vector2 direction;
  private Hitbox hitbox;

  public override void _PhysicsProcess(double delta)
  {
    Position += direction * Speed * (float)delta;
  }

  public void Initialize(Vector2 position, Vector2 direction)
  {
    hitbox = GetNodeOrNull<Hitbox>("Hitbox");
    if (hitbox != null) hitbox.HurtboxHit += OnHurtboxHit;

    LookAt(direction);
    this.direction = direction.Normalized();
    Position = position;

    Timer lifeTimer = new()
    {
      WaitTime = LifeDuration,
      OneShot = true
    };
    lifeTimer.Timeout += OnLifeTimerTimeout;
    AddChild(lifeTimer);
    lifeTimer.Autostart = true;
  }

  public void SetFaction(FactionManager.Faction faction)
  {
    hitbox?.SetFaction(faction);
  }

  private void OnHurtboxHit(Hurtbox hurtbox)
  {
    // Add special effects after a hit
    QueueFree();
  }

  private void OnLifeTimerTimeout()
  {
    // Add special effects for missing
    QueueFree();
  }

}
