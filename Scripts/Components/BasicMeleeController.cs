using Godot;
using System;

public partial class BasicMeleeController : Node2D, IInputController
{
  [Export]
  public float TargetSwitchTime = 2.0f;
  private Node2D target;
  public override void _Ready()
  {
    OnTargetSwitchTimerTimeout();

    Timer targetSwitchTimer = new Timer();
    AddChild(targetSwitchTimer);
    targetSwitchTimer.WaitTime = TargetSwitchTime;
    targetSwitchTimer.Timeout += OnTargetSwitchTimerTimeout;
    targetSwitchTimer.Start();
  }

  public override void _Process(double delta)
  {
    if (target is not null)
    {
      Vector2 targetRelativePosition = target.GlobalPosition - GlobalPosition;
      MovementInput?.Invoke(targetRelativePosition.Normalized());
      FocusInput?.Invoke(targetRelativePosition);

      if (targetRelativePosition.Length() <= 50)
      {
        Ability1?.Invoke();
        GD.Print("Enemy attacking!");
      }
    }
  }


  private void OnTargetSwitchTimerTimeout()
  {
    float closestDistance = float.PositiveInfinity;
    foreach (Node node in GetTree().GetNodesInGroup("PlayerFaction"))
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

  public event Action<Vector2> MovementInput;
  public event Action<Vector2> FocusInput;
  public event Action Ability1;

}
