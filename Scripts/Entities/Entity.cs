using Godot;
using System;

public partial class Entity : CharacterBody2D
{
  [Export]
  public float Speed = 100;

  private Vector2 focus = Vector2.Right;

  private IInputController input;
  private Health health;
  private IDeathComponent death;
  private IAbility attack;
  private IAbility special;

  private bool canAct = true;
  public override void _Ready()
  {
    input = GetNodeOrNull<IInputController>("Input");
    health = GetNodeOrNull<Health>("Health");
    death = GetNodeOrNull<IDeathComponent>("Death");

    attack = GetNodeOrNull<IAbility>("Attack");
    special = GetNodeOrNull<IAbility>("Special");

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
      input.Ability1 -= OnAttack;
      input.Ability2 -= OnSpecial;
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
      input.Ability1 += OnAttack;
      input.Ability2 += OnSpecial;
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
    this.focus = focus;
    if (canAct) Rotation = Position.AngleTo(focus);
  }

  public void OnAttack()
  {
    if (canAct && attack != null)
    {
      attack.Activate(GlobalPosition, focus);
    }
  }

  public void OnSpecial()
  {
    if (canAct && special != null)
    {
      special.Activate(GlobalPosition, focus);
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
