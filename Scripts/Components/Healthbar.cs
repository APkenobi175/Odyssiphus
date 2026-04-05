using Godot;
using System;

public partial class Healthbar : ProgressBar
{
  [Export]
  public Health Health;

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
  }

  public void OnHealthChanged(float currentHealth, float maxHealth)
  {
    MaxValue = maxHealth;
    Value = currentHealth;
  }

}
