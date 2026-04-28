using Godot;
using System;

public partial class BasicDeath : Node2D, IDeathComponent
{
  [Export]
  public float DeathTime = 1;

  public void Die()
  {
    GetTree().CreateTimer(DeathTime).Timeout += OnDeathTimerTimeout;
  }

  private void OnDeathTimerTimeout()
  {
    Died?.Invoke();
  }

  public event Action Died;
}
