using Godot;
using System;

public partial class Health : Node
{
  [Export]
  public float MaxHealth = 100;
  [Export]
  public float CurrentHealth { get; private set; }
  [Export]
  public int HealthRegenRate = 0;
  private double _TimeSinceLastHeal = 0;

  public override void _Ready()
  {
    CurrentHealth = MaxHealth;
  }

    public override void _Process(double delta)
    {
      if (CurrentHealth < MaxHealth)
    {
      _TimeSinceLastHeal += delta;
      if (HealthRegenRate > 0 && _TimeSinceLastHeal >= 1.0)
      {
        ChangeHealth(HealthRegenRate);
        GD.Print($"Odyssiphus healed. Health is now {CurrentHealth}");
        _TimeSinceLastHeal = 0;
      }
    }
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
