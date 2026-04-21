using Godot;
using System;

public partial class Slime : Entity, IProjectile
{
  [Export]
  public float SpawnDistance = 50;
  public void Initialize(Vector2 position, Vector2 direction)
  {
    Position = position + direction.Normalized() * SpawnDistance;
  }
}
