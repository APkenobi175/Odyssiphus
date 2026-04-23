using Godot;
using System;

public partial class ProjectileDeath : BaseProjectileAbility, IDeathComponent
{

  public override void _Ready()
  {
    base._Ready();

    BurstsFinished += OnBurstsFinished;
  }

  public void Die()
  {
    Activate(GlobalPosition, Vector2.Right);
  }

  private void OnBurstsFinished()
  {
    Died?.Invoke();
  }
  public event Action Died;
}
