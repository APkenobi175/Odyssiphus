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


  public void Play(string animationType, Vector2 direction, bool directional = true)
  {
    // Additional check for repeating animations?
    if (AnimationPlayer is null || animationType == "") return;

    string animation = SelectAnimation(animationType, direction, directional);

    if (!AnimationPlayer.HasAnimation(animation)) return;

    string currentAnimation = AnimationPlayer.CurrentAnimation;
    if (currentAnimation == animation) return;

    //GD.Print($"Attempting change: {currentAnimation} --> {animation}");

    // Find cleaner way to avoid preempting important animations?
    string currentAnimationType = GetAnimationType(currentAnimation);
    if (animationRank.TryGetValue(currentAnimationType, out int value) && animationRank[animationType] > value) return;

    AnimationPlayer.Play(animation);
    //GD.Print($"Playing animation: {animation}");
  }

  private string SelectAnimation(string animationType, Vector2 direction, bool directional)
  {
    string animationName = animationType;

    if (directional)
    {
      animationName += "_";
      if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
      {
        animationName += direction.X >= 0 ? "right" : "left";
      }
      else
      {
        animationName += direction.Y >= 0 ? "down" : "up";
      }
    }

    return animationName;
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
