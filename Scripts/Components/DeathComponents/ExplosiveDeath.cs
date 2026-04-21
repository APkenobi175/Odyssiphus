using Godot;
using System;

public partial class ExplosiveDeath : Node2D, IDeathComponent
{
  [Export]
  public PackedScene ExplosionScene;

  public override void _Ready()
  {
    if (ExplosionScene is null)
    {
      GD.PrintErr("ExplosiveDeath: No explosion set!");
      return;
    }
  }

  public void Die()
  {
    Node node = ExplosionScene.Instantiate<Node>();
    if (node is IExplosion explosion)
    {
      GetTree().Root.CallDeferred("add_child", node);
      explosion.Explode(GlobalPosition);
    }

    Died?.Invoke();
  }

  public event Action Died;

}
