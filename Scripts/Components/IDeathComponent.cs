using Godot;
using System;

public partial interface IDeathComponent
{
  public void Die();
  event Action Died;
}
