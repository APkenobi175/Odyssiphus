using Godot;
using System;

public partial class BasicProjectile : Node2D, IProjectile
{
  [Export]
  public float Speed = 1000;
  [Export]
  public float LifeDuration = 1;
  private Vector2 direction;

  public override void _PhysicsProcess(double delta)
  {
    Position += direction * Speed * (float)delta;
  }

  public void Initialize(Vector2 position, Vector2 direction)
  {
    Hitbox hitbox = GetNodeOrNull<Hitbox>("Hitbox");
    if (hitbox != null) hitbox.HurtboxHit += OnHurtboxHit;

    Position = position;
    this.direction = direction.Normalized();

    Timer lifeTimer = new Timer
    {
      WaitTime = LifeDuration,
      OneShot = true
    };
    lifeTimer.Timeout += OnLifeTimerTimeout;
    AddChild(lifeTimer);
    lifeTimer.Autostart = true;
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
