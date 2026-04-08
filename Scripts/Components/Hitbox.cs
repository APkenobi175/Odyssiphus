using Godot;
using System;

public partial class Hitbox : Area2D
{
  [Export]
  public float damage = 10;

  public override void _Ready()
  {
    AreaEntered += OnAreaEntered;
  }

  public void OnAreaEntered(Node2D body)
  {
    if (body is Hurtbox hurtbox)
    {
      HurtboxHit?.Invoke(hurtbox);

      hurtbox.Health?.ChangeHealth(-damage);
    }
  }

  public event Action<Hurtbox> HurtboxHit;
}
