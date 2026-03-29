using Godot;
using System;

public partial class UiLoadGame : CanvasLayer
{

    private VBoxContainer gameSlot;
    private Button backButton;

    private PackedScene saveSlotScene = GD.Load<PackedScene>("Scenes/UI/UI_LoadGameButton.tscn");

    public override void _Ready()
    {
        gameSlot = GetNode<VBoxContainer>("LoadGame/ScrollContainer/Buttons");
        backButton = GetNode<Button>("LoadGame/ResumeButton");
        backButton.Pressed += onBackButtonPressed;
        PopulateGameSlots();
    }

    private void onBackButtonPressed()
    {
        GameManager.Instance.GoTo("HomeScreen");
    }

    private void PopulateGameSlots()
    {
        // Clear existing slots
        foreach (Node child in gameSlot.GetChildren())
        {
            gameSlot.RemoveChild(child);
            child.Free();
        }

        // Get saved games from GameManager
        var savedGames = GameManager.Instance.GetSaveFiles();

        if (savedGames.Count == 0)
        {
            var empty = new Label();
            empty.Text = "No saved games found.";
            gameSlot.AddChild(empty);
            return;
        }

        foreach (var path in savedGames)
        {
            string displayName = GetDisplayName(path);
            string captured = path;

            var slot = saveSlotScene.Instantiate<UiLoadGameButton>();
            slot.Setup(displayName, captured);
            gameSlot.AddChild(slot);
            GD.Print($"Added button for save file: {displayName} at path: {captured}");
        }
    }

    private string GetDisplayName(string path)
    {
        if (!FileAccess.FileExists(path))
            return path;

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var json = new Json();
        json.Parse(file.GetAsText());
        var data = json.Data.AsGodotDictionary();

        if (data.ContainsKey("savedAt"))
        {
            return "Save - " + data["savedAt"].ToString();

        }
        return System.IO.Path.GetFileNameWithoutExtension(path);
    }

}
