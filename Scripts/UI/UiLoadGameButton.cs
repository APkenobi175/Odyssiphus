using Godot;

public partial class UiLoadGameButton : Button  
{
    public void Setup(string displayName, string path)
    {
        Text = displayName; 
        Pressed += () => OnPressed(path);
    }

    private void OnPressed(string path)
    {
        GameManager.Instance.LoadGame(path);
        GameManager.Instance.GoTo("Dungeon");
    }
}