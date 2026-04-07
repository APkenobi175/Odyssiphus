using Godot;

public partial class UiPauseMenu : CanvasLayer
{
    private Button resumeButton;
    private Button teleportToShipButton;
    private Button saveAndExitButton;
    private Button settingsButton;

    private Button devButton;
    
    public override void _Ready()
    {
        resumeButton = GetNode<Button>("PauseMenu/Buttons/ResumeButton");
        teleportToShipButton = GetNode<Button>("PauseMenu/Buttons/TeleportToShipButton");
        saveAndExitButton = GetNode<Button>("PauseMenu/Buttons/Exit");
        settingsButton = GetNode<Button>("PauseMenu/Buttons/SettingsButton");
        devButton = GetNode<Button>("Exit2");

        resumeButton.Pressed += OnResumePressed;
        teleportToShipButton.Pressed += OnTeleportToShipPressed;
        saveAndExitButton.Pressed += OnSaveAndExitPressed;
        settingsButton.Pressed += OnSettingsPressed;
        devButton.Pressed += () => { CallDeferred(nameof(DoGoTo), "DevMapView"); GetTree().Paused = false; };

        // Show immediately if returning from settings
        bool returningFromSettings = GameManager.Instance.PreviousScene == "Settings";
        Visible = returningFromSettings;
        GetTree().Paused = returningFromSettings;

        RefreshButtons();
    }

    public override void _Input(InputEvent e)
    {
        if (e.IsActionPressed("ui_cancel"))
            TogglePause();
    }

    private void TogglePause()
    {
        Visible = !Visible;
        GetTree().Paused = Visible;
        if (Visible) RefreshButtons();
    }

    private void RefreshButtons()
    {
        bool onShip = GameManager.Instance.CurrentScene == "Ship";

        if (onShip)
        {
            teleportToShipButton.Text = "Return to Dungeon";
            bool hasDungeonProgress = GameManager.Instance.CurrentDungeonRooms != null
                && GameManager.Instance.CurrentDungeonRooms.Count > 0
                && GameManager.Instance.CurrentDungeonRooms.Exists(r => r.IsCleared);
            teleportToShipButton.Disabled = !hasDungeonProgress;
            teleportToShipButton.TooltipText = hasDungeonProgress ? "" : "Enter the dungeon first!";
        }
        else
        {
            // WIll not allow teleporting to the ship until the room is cleared.
            teleportToShipButton.Text = "Teleport to Ship";
            bool roomCleared = GameManager.Instance.currentRoom?.IsCleared ?? false;
            teleportToShipButton.Disabled = !roomCleared;
            teleportToShipButton.TooltipText = roomCleared ? "" : "Clear the room first!";
        }
    }

    private void OnResumePressed()
    {
        Visible = false;
        GetTree().Paused = false;
    }

    private void OnTeleportToShipPressed()
    {
        // If you are on the ship, go back to the dungeon
        // If you are in the dungeon, go to the ship
        bool onShip = GameManager.Instance.CurrentScene == "Ship";

        if (onShip)
        {
            CallDeferred(nameof(DoGoTo), GameManager.Instance.PreviousScene);
        }
        else
        {
            if (GameManager.Instance.currentRoom?.IsCleared != true) return;
            CallDeferred(nameof(DoGoTo), "Ship");
        }
        // GoTo already unpauses, but just in case
        GetTree().Paused = false;
    }

    private void DoGoTo(string scene)
    {
        GameManager.Instance.GoTo(scene);
    }

    private void OnSaveAndExitPressed()
    {
        GameManager.Instance.SaveGame();
        GameManager.Instance.ChangeSong("Menu");
        GameManager.Instance.PlayMusic();
        CallDeferred(nameof(DoGoTo), "HomeScreen");
    }

    private void OnSettingsPressed()
    {
        CallDeferred(nameof(DoGoTo), "Settings");
    }
}