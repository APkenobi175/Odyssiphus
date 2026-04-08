using Godot;
using System;

public partial class BaseMeleeAbility : Node2D, IAbility
{
  [Export]
  public Sprite2D Sprite;
  [Export]
  public AnimationPlayer AnimationPlayer;

  public override void _Ready()
  {
    //
  }

  public void Activate(Vector2 position, Vector2 direction)
  {
    GlobalRotation = direction.Angle();
    AnimationPlayer?.Play("Attack");
  }

}
