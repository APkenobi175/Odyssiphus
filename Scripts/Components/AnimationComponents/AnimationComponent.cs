using Godot;
using System;
using System.Collections.Generic;

public partial class AnimationComponent : Node2D
{
  [Export]
  public AnimationPlayer AnimationPlayer;

  private readonly Dictionary<string, int> animationRank = new()
  {
    {"death", 0},
    {"attack", 1},
    {"special", 1},
    {"move", 2},
    {"idle", 2},
    {"RESET", 9999},
  };

  public override void _Ready()
  {
    if (AnimationPlayer is null)
    {
      GD.PrintErr("AnimationComponent: No AnimationPlayer provided!");
      return;
    }

    AnimationPlayer.AnimationFinished += OnAnimationFinished;
  }


  public void Play(string animationType, Vector2 direction)
  {
    // Additional check for repeating animations?
    if (AnimationPlayer is null || animationType == "") return;

    (bool valid, string animation) = SelectAnimation(animationType, direction);

    if (!valid) return;

    int rank = animationRank[animationType];
    string currentAnimation = AnimationPlayer.CurrentAnimation;
    if (currentAnimation == animation)
    {
      // Restart attack animations
      if (rank == 1)
      {
        AnimationPlayer.Stop();
        AnimationPlayer.Play();
        //GD.Print("Repeating animation!");
        return;
      }
      else return;
    }

    //GD.Print($"Attempting change: {currentAnimation} --> {animation}");

    // Find cleaner way to avoid preempting important animations?
    string currentAnimationType = GetAnimationType(currentAnimation);
    if (animationRank.TryGetValue(currentAnimationType, out int value) && rank > value) return;

    AnimationPlayer.Play(animation);
    //GD.Print($"Playing animation: {animation}");
  }

  private (bool valid, string animation) SelectAnimation(string animationType, Vector2 direction)
  {
    string animationName = animationType;
    GD.Print($"Attempting base: {animationName}");
    if (AnimationPlayer.HasAnimation(animationName)) return (true, animationName);

    animationName += "_";
    string animationDirection;
    GD.Print($"Attempting direction: {animationName}");

    if (Mathf.Abs(direction.Y) >= Mathf.Abs(direction.X))
    {
      animationDirection = direction.Y >= 0 ? "down" : "up";
      GD.Print($"Attempting up/down: {animationName}");
      if (AnimationPlayer.HasAnimation(animationName + animationDirection)) return (true, animationName + animationDirection);
    }
    
    GD.Print($"Attempting left/right: {animationName}");
    animationDirection = direction.X >= 0 ? "right" : "left";
    if (AnimationPlayer.HasAnimation(animationName + animationDirection)) return (true, animationName + animationDirection);

    GD.Print($"Nothing found: {animationName}");

    return (false, "");
  }

  private string GetAnimationType(StringName animationName)
  {
    return animationName.ToString().Split("_")[0];
  }

  private void OnAnimationFinished(StringName animationName)
  {
    string type = GetAnimationType(animationName);
    if (type == "death" || type == "attack" || type == "special") KeyAnimationFinished?.Invoke(type);

    AnimationPlayer?.Play("RESET");
  }

  public event Action<string> KeyAnimationFinished;
}
