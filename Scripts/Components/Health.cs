using Godot;
using System;

public partial class Health : Node
{
  [Export]
  public float MaxHealth = 100;
  public float CurrentHealth { get; private set; }

  public override void _Ready()
  {
    CurrentHealth = MaxHealth;
  }

  public void SetMaxHealth(float newMaxHealth, bool full)
  {
    MaxHealth = Mathf.Max(1.0f, newMaxHealth);

    if (full)
    {
      CurrentHealth = MaxHealth;
    }

    HealthChanged?.Invoke(CurrentHealth, MaxHealth);
  }

  public void ChangeHealth(float amount)
  {
    CurrentHealth += amount;

    if (CurrentHealth <= 0) HealthDepleted?.Invoke();

    CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
    HealthChanged?.Invoke(CurrentHealth, MaxHealth);
  }

  public event Action HealthDepleted;
  public event Action<float, float> HealthChanged;

}
