using Godot;




public partial class UiAudioSettings : CanvasLayer
{

    public Label volumeLabel;


    public HSlider slider;
    

    public override void _Ready()
    {
        slider = GetNode<HSlider>("VBoxContainer/VBoxContainer/HSlider");
        slider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(0));
        slider.ValueChanged += OnVolumeChanged;
        volumeLabel = GetNode<Label>("VBoxContainer/VBoxContainer/Volume");
        
      

    }

    public override void _Process(double delta)
    {
        volumeLabel.Text = $"Volume: {(int)(slider.Value * 100)}%";
    }

    public void OnVolumeChanged(double value)
    {
        GameManager.Instance.SetVolume((float)value); // Set the volume of the master bus to the value specified by the slider
        
    }




}
