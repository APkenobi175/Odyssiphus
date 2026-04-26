using Godot;
using System;

public partial class PosiedonIcon : AnimatedSprite2D
{
    private Entity body;
    private bool isDead = false;

    public override void _Ready()
    {
        body = GetParent<Entity>();

        var health = body.GetNodeOrNull<Health>("Health");
        if (health != null)
            health.HealthDepleted += OnDied;
    }

    private async void OnDied()
    {
        if (isDead) return;
        isDead = true;

        // Play boss room closing cutscene
        var bossRoom = GetTree().GetFirstNodeInGroup("BossRoom") as BossRoom;
        if (bossRoom != null)
        {
            bossRoom.cutsceneAnimation.Play("death_cutscene");
            await ToSignal(bossRoom.cutsceneAnimation, AnimationPlayer.SignalName.AnimationFinished);

        }

        body.QueueFree();
    }
}