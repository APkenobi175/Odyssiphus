using Godot;
using System;

public partial class BaseProjectileAbility : Node2D, IAbility, IFactionable
{
  [Export]
  public float Cooldown = 1;
  [Export]
  public PackedScene ProjectileScene;
  [Export]
  public AudioStreamPlayer AudioStream;
  [Export]
  public int SpreadCount = 1; // Number of projectiles in each spread
  [Export]
  public float SpreadAngle = 0; // Total angle of spread (0 = no spread, 360 = circle)
  [Export]
  public int BurstCount = 1;  // Number of bursts/separate spreads
  [Export]
  public float BurstDelay = 0.01f;  // Seconds between each burst

  private int burstTotal = 0;
  private Timer burstTimer;
  private Timer cooldownTimer;
  private Vector2 direction;
  private FactionManager.Faction faction = FactionManager.Faction.Neutral;

  public override void _Ready()
  {
    burstTimer = new Timer
    {
      WaitTime = BurstDelay,
      OneShot = false
    };
    burstTimer.Timeout += OnBurstTimerTimeout;
    AddChild(burstTimer);

    cooldownTimer = new Timer
    {
      WaitTime = Cooldown,
      OneShot = true
    };
    AddChild(cooldownTimer);
  }


  public bool Activate(Vector2 position, Vector2 direction)
  {
    if (!cooldownTimer.IsStopped()) return false;
    if (BurstCount <= 0) return false;

    this.direction = direction;

    SpawnSpread();
    cooldownTimer.Start();

    if (BurstCount > 1)
    {
      burstTotal = 1;
      burstTimer.Start();
    }
    else BurstsFinished?.Invoke();
    return true;
  }

  public void SetFaction(FactionManager.Faction faction)
  {
    this.faction = faction;
  }

  private void SpawnSpread()
  {
    if (SpreadCount <= 0) return;
    float totalAngle = SpreadAngle * Mathf.Pi / 180;
    float angleIncrement = totalAngle / SpreadCount;
    float startAngle = direction.Angle() - (totalAngle - angleIncrement * ((SpreadCount + 1) / 2));
    
    for (int i = 0; i < SpreadCount; i++)
    {
      SpawnProjectile(GlobalPosition, Vector2.Right.Rotated(startAngle + (i * angleIncrement)));
    }

    AudioStream?.Play();
  }

  private void SpawnProjectile(Vector2 position, Vector2 direction)
  {
    Node node = ProjectileScene.Instantiate<Node>();
    if (node is IProjectile projectile)
    {
      projectile.Initialize(position, direction);
      
      if (projectile is IFactionable factionableProjectile)
      {
        factionableProjectile.SetFaction(faction);
      }

      GetTree().Root.CallDeferred("add_child", node);
    }
  }

  private void OnBurstTimerTimeout()
  {
    burstTotal++;
    SpawnSpread();

    if (burstTotal >= BurstCount)
    {
      BurstsFinished?.Invoke();
      burstTimer.Stop();
      return;
    }
  }

  public event Action BurstsFinished;
}
