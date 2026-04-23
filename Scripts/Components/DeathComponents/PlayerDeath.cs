using Godot;
using System;

public partial class PlayerDeath : Node2D, IDeathComponent
{
  public void Die()
  {
    // Special player death effects
    // Create ghost?

    //GameManager.Instance.OnPlayerDied

    Died?.Invoke();
  }

  public event Action Died;
}
