using Godot;
using System;

public partial class KeyboardInputController : Node2D, IInputController
{
  public override void _Process(double delta)
  {
    Vector2 direction = new()
    {
      X = Input.GetAxis("move_left", "move_right"),
      Y = Input.GetAxis("move_up", "move_down")
    };
    MovementInput?.Invoke(direction.Normalized());

    Vector2 focusDirection = GetGlobalMousePosition() - GlobalPosition;
    FocusInput?.Invoke(focusDirection);

    if (Input.IsActionJustPressed("attack"))
    {
      Ability1?.Invoke();
    }

    if (Input.IsActionJustPressed("special"))
    {
      Ability2?.Invoke();
    }
  }

  public event Action<Vector2> MovementInput;
  public event Action<Vector2> FocusInput;
  public event Action Ability1;
  public event Action Ability2;

}
