using Godot;
using System;

public partial class Polyphemus3 : AnimatedSprite2D
{
    private CharacterBody2D body;
    private string facing = "right";

    public bool isDead = false;

    public Health health;

    public bool isAttacking = false;
   
    public override void _Ready()
    {
        body = GetParent<CharacterBody2D>();
        health = body.GetNode<Health>("Health");
        if (health != null)
            health.HealthDepleted += OnDied;

        // Watch his AnimationPlayer for attack triggers
        var attackNode = body.GetNodeOrNull("Attack");
        if (attackNode != null)
        {
            var animPlayer = attackNode.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
            // path might differ — could be attackNode.GetChildren() search if not a fixed name
            if (animPlayer != null)
                animPlayer.AnimationStarted += OnHisAttackStarted;
        }

        AnimationFinished += OnAnimationFinished;
        Play("idle_right");
    }

    public override void _Process(double delta)
    {

        if (isDead) return;
        if (isAttacking) return;
        if (body.Velocity.X > 1f)
        {
            facing = "right";
        }
        else if (body.Velocity.X < -1f)
        {
            facing = "left";
        }

        if (body.Velocity.Length() > 1f)
        {
            Play("move_" + facing);
        }
        else
        {
            Play("idle_" + facing);
        }
    }

    public void OnDied()
    {
        if(isDead) return;
        isDead = true;
        Play("death_" + facing);

    }

    private void OnAnimationFinished()
    {
        if (Animation == "death_" + facing)
        {
            body.QueueFree();
            GameManager.Instance.OnRoomCleared(GameManager.Instance.PlayerCurrentRoom);
        }
        else if (Animation == "attack_" + facing)
        {
            isAttacking = false;
        }
    }

    private void OnHisAttackStarted(StringName animName)
    {
        if (animName == "Attack")
            OnAttackActivated();
    }

    private void OnAttackActivated()
    {
        if (isDead || isAttacking) return;
        isAttacking = true;
        Play("attack_" + facing);
    }
}
