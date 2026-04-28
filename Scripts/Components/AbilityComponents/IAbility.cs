using Godot;
using System;

public partial interface IAbility
{
  public bool Activate(Vector2 position, Vector2 direction);
}
