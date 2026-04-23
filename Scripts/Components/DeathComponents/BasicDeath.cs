using Godot;
using System;

public partial class BasicDeath : Node2D, IDeathComponent
{
  public void Die()
  {
    Died?.Invoke();
  }

  public event Action Died;
}
