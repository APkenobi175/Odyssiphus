using Godot;
using System;

public partial class Entity : CharacterBody2D
{
  [Export]
  public float Speed = 100;
  private IInputController input;
  private Health health;
  private IDeathComponent death;

  private bool canAct = true;
  public override void _Ready()
  {
    input = GetNodeOrNull<IInputController>("Input");
    health = GetNodeOrNull<Health>("Health");
    death = GetNodeOrNull<IDeathComponent>("Death");

    ConnectEvents();
  }

  public override void _PhysicsProcess(double delta)
  {
    MoveAndSlide();
  }


  public void DisconnectEvents()
  {
    // To avoid duplicate events?
    if (input != null)
    {
      input.MovementInput -= OnMovementInput;
      input.FocusInput -= OnFocusInput;
    }

    if (health != null)
    {
      health.HealthDepleted -= OnHealthDepleted;
    }

    if (death != null)
    {
      death.Died -= OnDied;
    }
  }

  public void ConnectEvents()
  {
    if (input != null)
    {
      input.MovementInput += OnMovementInput;
      input.FocusInput += OnFocusInput;
    }

    if (health != null)
    {
      health.HealthDepleted += OnHealthDepleted;
    }

    if (death != null)
    {
      death.Died += OnDied;
    }
  }

  public void OnMovementInput(Vector2 moveInput)
  {
    if (canAct) Velocity = moveInput * Speed;
  }

  public void OnFocusInput(Vector2 focus)
  {
    if (canAct) Rotation = Position.AngleTo(focus);
  }

  public void OnAttack()
  {
    if (canAct)
    {
      //
    }
  }

  public void OnSpecial()
  {
    if (canAct)
    {
      //
    }
  }

  public void OnHealthDepleted()
  {
    canAct = false;
    Velocity = Vector2.Zero;
    death?.Die();
  }

  public void OnDied()
  {
    // Probably not the best way to handle this?
    // Should override for player
    QueueFree();
  }

}
