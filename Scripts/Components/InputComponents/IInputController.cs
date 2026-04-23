using Godot;
using System;

public partial interface IInputController
{
  event Action<Vector2> MovementInput {add{} remove{}}
  event Action<Vector2> FocusInput {add{} remove{}}
  event Action Ability1 {add{} remove{}}
  event Action Ability2 {add{} remove{}}
}
