using Godot;
using System;

public partial class Healthbar : ProgressBar
{
  [Export]
  public Health Health;
  [Export]
  public float Length = 50;
  [Export]
  public float Offset = 40;

  private float widthRatio = 0.1f;

  public override void _Ready()
  {
    if (Health == null)
    {
      GD.PrintErr("Healthbar: Health is not assigned.");
      return;
    }

    MaxValue = Health.MaxHealth;
    Value = Health.CurrentHealth;

    Health.HealthChanged += OnHealthChanged;

    Size = new Vector2(Length, Length * widthRatio);

    Position = new Vector2(-Size.X/2, -Offset);
  }

  public void OnHealthChanged(float currentHealth, float maxHealth)
  {
    MaxValue = maxHealth;
    Value = currentHealth;
  }

}
