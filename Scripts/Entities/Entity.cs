using Godot;
using System;

public partial class Entity : CharacterBody2D
{
  [Export]
  public float Speed = 100;
  [Export]
  public FactionManager.Faction Faction = FactionManager.Faction.Neutral;

  private IInputController input;
  private Hurtbox hurtbox;
  private Health health;
  private IDeathComponent death;
  private IAbility attack;
  private IAbility special;

  private Vector2 focus = Vector2.Right;

  private bool canAct = true;
  public override void _Ready()
  {
    input = GetNodeOrNull<IInputController>("Input");
    hurtbox = GetNodeOrNull<Hurtbox>("Hurtbox");
    health = GetNodeOrNull<Health>("Health");
    death = GetNodeOrNull<IDeathComponent>("Death");

    attack = GetNodeOrNull<IAbility>("Attack");
    special = GetNodeOrNull<IAbility>("Special");

    ConnectEvents();
    SetComponentFactions(Faction);
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

  public void SetComponentFactions(FactionManager.Faction faction)
  {
    // Should add IFactionable? With change to make it var with getter/setter?
    // Also add entity to relevant group?
    if (input is IFactionable factionableInput)
    {
      factionableInput.SetFaction(faction);
    }

    // Hitboxes/hurtboxes may not need check, since they most likely will always require a "faction"
    if (hurtbox is IFactionable factionableHurtbox)
    {
      factionableHurtbox.SetFaction(faction);
    }

    if (death is IFactionable factionableDeath)
    {
      factionableDeath.SetFaction(faction);
    }

    if (attack is IFactionable factionableAttack)
    {
      factionableAttack.SetFaction(faction);
    }

    if (special is IFactionable factionableSpecial)
    {
      factionableSpecial.SetFaction(faction);
    }
  }

  public void OnMovementInput(Vector2 moveInput)
  {
    if (canAct) Velocity = moveInput * Speed;
  }

  public void OnFocusInput(Vector2 focus)
  {
    this.focus = focus;
    //if (canAct) Rotation = Position.AngleTo(focus);
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
