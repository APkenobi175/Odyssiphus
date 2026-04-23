using Godot;
using System;

public partial class BasicExplosion : Node2D, IExplosion
{
  [Export]
  public AnimationPlayer animationPlayer;

  public override void _Ready()
  {
    if (animationPlayer is not null) animationPlayer.AnimationFinished += OnExplosionFinished;
  }

  public void Explode(Vector2 position)
  {
    GlobalPosition = position;
    if (animationPlayer is not null) animationPlayer.Play("explode");
    else QueueFree();
  }

  private void OnExplosionFinished(StringName animationName)
  {
    if (animationName == "explode") QueueFree();
  }

}
