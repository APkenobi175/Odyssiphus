using Godot;
using System;

public partial class BasicProjectileController : Node2D, IInputController
{
  [Export]
  public float TargetSwitchTime = 2.0f;
  [Export]
  public float AttackDistance = 500;
  [Export]
  public float MaxIdealRange = 200;
  [Export]
  public float MinIdealRange = 50;
  [Export]
  public bool IgnoreObstacles = false;

  private RayCast2D raycast;
  private Node2D target;
  private String targetGroup = "PlayerFaction";

  public override void _Ready()
  {
    SwitchTarget();

    Timer targetSwitchTimer = new Timer();
    AddChild(targetSwitchTimer);
    targetSwitchTimer.WaitTime = TargetSwitchTime;
    targetSwitchTimer.Timeout += OnTargetSwitchTimerTimeout;
    targetSwitchTimer.Start();

    raycast = GetNodeOrNull<RayCast2D>("RayCast2D");
    if (raycast is not null)
    {
      raycast.ForceRaycastUpdate();
      if (raycast.GetCollider() is CollisionObject2D self)
      {
        raycast.AddException(self);
      }
    }
  }

  public override void _Process(double delta)
  {
    if (target is not null && IsInstanceValid(target))
    {
      Vector2 targetRelativePosition = target.GlobalPosition - GlobalPosition;
      FocusInput?.Invoke(targetRelativePosition);

      if (targetRelativePosition.Length() < MinIdealRange) MovementInput?.Invoke(-targetRelativePosition.Normalized());
      else if (targetRelativePosition.Length() > MaxIdealRange) MovementInput?.Invoke(targetRelativePosition.Normalized());


      if (CanAttack(targetRelativePosition)) Ability1?.Invoke();
    }
    else
    {
      MovementInput?.Invoke(Vector2.Zero);
      SwitchTarget();
    }
  }

  private void SwitchTarget()
  {
    float closestDistance = float.PositiveInfinity;
    foreach (Node node in GetTree().GetNodesInGroup(targetGroup))
    {
      if (node is Node2D node2D)
      {
        float distance = GlobalPosition.DistanceSquaredTo(node2D.GlobalPosition);

        if (distance < closestDistance)
        {
          target = node2D;
          closestDistance = distance;
        }
      }
    }
  }

  private bool CanAttack(Vector2 targetRelativePosition)
  {
    if (targetRelativePosition.Length() > AttackDistance) return false;
    if (IgnoreObstacles) return true;
    if (raycast is null) return false;
    
    raycast.TargetPosition = targetRelativePosition;
    if (!raycast.IsColliding()) return false;
    if (raycast.GetCollider() is Node node && node.IsInGroup(targetGroup)) return true;

    return false;
  }

  private void OnTargetSwitchTimerTimeout()
  {
    SwitchTarget();
  }

  public event Action<Vector2> MovementInput;
  public event Action<Vector2> FocusInput;
  public event Action Ability1;

}
