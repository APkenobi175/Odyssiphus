using Godot;
using System;

public partial class FactionManager
{
  public enum Faction { Neutral, Player, Enemy, Minion, Boss }

  private static readonly uint LayerWorld = 1 << 0; // Physical Collision
  private static readonly uint LayerEnvironment = (1 << 1) | (1 << 2) | (1 << 3); // Environment triggers, player pickup, etc.
  private static readonly uint LayerNeutral = 1 << 4; // Neutral items (e.g. explosive barrel)
  private static readonly uint LayerPlayer = 1 << 5;  // Player and allies
  private static readonly uint LayerEnemy = 1 << 6;   // Basic enemies
  private static readonly uint LayerMinion = 1 << 7;  // Minions of boss characters
  private static readonly uint LayerBoss = 1 << 8;    // Boss characters

  public static uint GetHurtboxLayer(Faction faction)
  {
    return faction switch
    {
      Faction.Neutral => LayerNeutral,
      Faction.Player => LayerPlayer,
      Faction.Enemy => LayerEnemy,
      Faction.Minion => LayerMinion,
      Faction.Boss => LayerBoss,
      _ => 0,
    };
  }

  public static uint GetHitboxMask(Faction faction, bool isProjectile)
  {
    uint mask = faction switch
    {
      Faction.Neutral => uint.MaxValue ^ LayerEnvironment,  // Hits everything but environment
      Faction.Player => ~(LayerPlayer | LayerEnvironment),  // Everything but self and environment
      Faction.Enemy => ~(LayerEnemy | LayerEnvironment),
      Faction.Minion => ~(LayerMinion | LayerBoss | LayerEnvironment),  // Can't hit boss
      Faction.Boss => ~(LayerBoss | LayerEnvironment),  // Can hit minions
      _ => 0,
    };

    // Possibly irrelevant? Give projectiles a separate shape for collision?
    if (isProjectile) return mask |= LayerWorld;  // Projectile attacks stopped by walls, etc.
    else return mask &= ~LayerWorld;
  }
}
