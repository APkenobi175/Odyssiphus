using Godot;
using System;

public partial class Entity : CharacterBody2D
{
  [Export]
  public float Speed = 100;
  [Export]
  public FactionManager.Faction Faction = FactionManager.Faction.Neutral;
  [Export]
  public AnimationComponent AnimationComponent;

  private IInputController input;
  private Hurtbox hurtbox;
  private Health health;
  private IDeathComponent death;
  private IAbility attack;
  private IAbility special;

  private Vector2 focus = Vector2.Right;

  private bool canAct = true;
  private bool usingAbility = false;

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

    if (AnimationComponent is not null)
    {
      AnimationComponent.KeyAnimationFinished -= OnKeyAnimationFinished;
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

    if (AnimationComponent is not null)
    {
      AnimationComponent.KeyAnimationFinished += OnKeyAnimationFinished;
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

    if (Faction == FactionManager.Faction.Enemy)
    {
      AddToGroup("Enemies");
    }
    if (Faction == FactionManager.Faction.Player)
    {
      AddToGroup("Player");
    }
    if(Faction == FactionManager.Faction.Boss)
    {
      AddToGroup("Boss");
    }
  }

  public void OnMovementInput(Vector2 moveInput)
  {
    if (!canAct) return;

    Velocity = moveInput * Speed;

    if (usingAbility) return;
    if (Velocity == Vector2.Zero)
    {
      AnimationComponent?.Play("idle", focus);
    }
    else
    {
      AnimationComponent?.Play("move", Velocity);
    }
  }

  public void OnFocusInput(Vector2 focus)
  {
    this.focus = focus;
  }

  public void OnAttack()
  {
    if (!canAct) return;
    if (attack is null) return;

    if (!attack.Activate(GlobalPosition, focus)) return;
    AnimationComponent?.Play("attack", focus);
    usingAbility = true;
  }

  public void OnSpecial()
  {
    if (!canAct) return;
    if (special is null) return;
    
    if (!special.Activate(GlobalPosition, focus)) return;
    AnimationComponent?.Play("special", focus);
    usingAbility = true;
  }

  public void OnHealthDepleted()
  {
    canAct = false;
    Velocity = Vector2.Zero;
    
    AnimationComponent?.Play("death", focus);
    death?.Die();
  }

  private void OnKeyAnimationFinished(string type)
  {
    if(type == "attack" || type == "special") usingAbility = false;
  }

  public void OnDied()
  {
    // Probably not the best way to handle this?
    // Should override for player
    QueueFree();
  }

}
