using Godot;
using System;

public partial class KeyboardInputController : Node2D, IInputController
{
  public override void _Process(double delta)
  {
    // this checks to see if the UI is consuming the mouse input, or has focus
    if (GetViewport().GuiGetFocusOwner() != null || InventoryData.SelectedItem != null)
    {
      MovementInput?.Invoke(Vector2.Zero);
      return;
    }

    Vector2 direction = new()
    {
      X = Input.GetAxis("move_left", "move_right"),
      Y = Input.GetAxis("move_up", "move_down")
    };
    MovementInput?.Invoke(direction.Normalized());

    Vector2 focusDirection = GetGlobalMousePosition() - GlobalPosition;
    FocusInput?.Invoke(focusDirection);
  }

  // Moved this out of process so that it does not eat clicks when
  // the invnetory is open.
    public override void _UnhandledInput(InputEvent @event)
    {
      // This does the same thing as "IsActionJustPressed" when not in a _Prosses block.
      if (@event.IsActionPressed("attack"))
      {
        Ability1?.Invoke();
      }

      if (@event.IsActionPressed("special"))
      {
        Ability2?.Invoke();
      }
    }


  public event Action<Vector2> MovementInput;
  public event Action<Vector2> FocusInput;
  public event Action Ability1;
  public event Action Ability2;

}
