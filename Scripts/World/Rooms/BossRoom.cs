using Godot;
using System;
using System.Threading.Tasks;

public partial class BossRoom : Node2D
{

    private Marker2D spawnPoint;
    private bool hasSpawned = false;

    public float clearRoomDelay = 1.5f;
    private const float ClearCheckTime = 1.5f;

    public RandomWalkRoom RoomData { get; set; }

	public AnimationPlayer cutsceneAnimation;

	public bool custsceneFinished = false;

    public bool youWinTriggered = false;

    public Node2D Ocean;

    public ColorRect blackRectangle;

    public Label cutsceneLabel;



    public override void _Ready()
    {   
        blackRectangle = GetNode<ColorRect>("ColorRect");
        cutsceneLabel = GetNode<Label>("Label7");
        Ocean = GetNode<Node2D>("Background");

        blackRectangle.Visible = false;
        cutsceneLabel.Visible = false;
        Ocean.Visible = false;
        disablePosiedon();

        AddToGroup("BossRoom");
		cutsceneAnimation = GetNode<AnimationPlayer>("AnimationPlayer");

    }

    private async void onRoomEntered(Node2D body)
    {

        // pause scene
        
        GD.Print("Player Entered Boss Room!!");
        if (body is not Entity entity || hasSpawned) return;
        

        hasSpawned = true;

        var room = RoomData;
        if (room.IsCleared)
        {
            GD.Print("Room is already cleared, not spawning boss.");
            GetTree().Paused = true;
            return;
        }



        cutsceneAnimation.Play("cutscene");
		await ToSignal(cutsceneAnimation, AnimationPlayer.SignalName.AnimationFinished);
		custsceneFinished = true;
        CallDeferred("StartBossFight");
    }

    private void StartBossFight()
    {
        enablePosiedon();

        GD.Print("Changed music to boss music!");

		_ = GameManager.Instance.ChangeSong("Boss");

        GD.Print("Playing music!");
        GameManager.Instance.PlayMusic();
        


    }

    public override void _Process(double delta)
    {
		if(!custsceneFinished) return;
        if (!hasSpawned) return;
        if (RoomData == null) return;
        if (RoomData.IsCleared) return;

        bool hasLivingEnemies = GetTree().GetNodesInGroup("Boss").Count > 0;

        if (hasLivingEnemies)
        {
            clearRoomDelay = ClearCheckTime;
            return;
        }

        clearRoomDelay -= (float)delta;
        if (clearRoomDelay <= 0f && !youWinTriggered)
        {
            youWinTriggered = true;
            YouWin();
        }
    }

    public void disablePosiedon()
    {
        var posiedon = GetNodeOrNull<Node2D>("Posiedon");
        if (posiedon != null) posiedon.ProcessMode = ProcessModeEnum.Disabled;
        else GD.PrintErr("Poseidon not found!");
    }

    public void enablePosiedon()
    {
        var posiedon = GetNodeOrNull<Node2D>("Posiedon");
        if (posiedon != null) posiedon.ProcessMode = ProcessModeEnum.Inherit;
        else GD.PrintErr("Poseidon not found!");
    }

    private async void YouWin()
    {
        var player = GetTree().GetFirstNodeInGroup("Player");
        if (player != null) player.QueueFree();

        if (cutsceneAnimation != null && GodotObject.IsInstanceValid(cutsceneAnimation))
        {
            _ = GameManager.Instance.ChangeSong("Menu", 45f);
            
            cutsceneAnimation.Play("The End");
            await ToSignal(cutsceneAnimation, AnimationPlayer.SignalName.AnimationFinished);
        }

        GameManager.Instance.OnPlayerWon();
    }
}