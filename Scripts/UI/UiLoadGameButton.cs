using Godot;

public partial class UiLoadGameButton : Button
{
    private string savePath;
    private bool isSetup = false;

    public void Setup(string displayName, string path)
    {
        Text = displayName;
        savePath = path;

        if (!isSetup)
        {
            Pressed += OnPressed;
            isSetup = true;
        }
    }

    private void OnPressed()
    {
        GameManager.Instance.StopMusic();
        GameManager.Instance.LoadGame(savePath);
    }
}