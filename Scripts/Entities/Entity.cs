using Godot;
using System;

public partial class Entity : CharacterBody2D
{
  [Export]
  public float Speed = 100;
  private IInputController input;
  public override void _Ready()
  {
    if (HasNode("Input"))
    {
      input = GetNodeOrNull<IInputController>("Input");
    }

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
  }

  public void ConnectEvents()
  {
    if (input != null)
    {
      input.MovementInput += OnMovementInput;
      input.FocusInput += OnFocusInput;
    }
  }

  public void OnMovementInput(Vector2 moveInput)
  {
    Velocity = moveInput * Speed;
  }

  public void OnFocusInput(Vector2 focus)
  {
    Rotation = Position.AngleTo(focus);
  }

  public void OnAttack()
  {
    //
  }

  public void OnSpecial()
  {
    //
  }

}
