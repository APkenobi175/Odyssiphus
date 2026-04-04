using Godot;
using System;

public partial class Camera2d : Camera2D
{
    private const float ZoomSpeed = 0.1f;
    private const float PanSpeed = 400f;

    public override void _Process(double delta)
    {
        Vector2 pan = Vector2.Zero;
        if (Input.IsKeyPressed(Key.Left) || Input.IsKeyPressed(Key.A))  pan.X -= 1;
        if (Input.IsKeyPressed(Key.Right) || Input.IsKeyPressed(Key.D)) pan.X += 1;
        if (Input.IsKeyPressed(Key.Up) || Input.IsKeyPressed(Key.W))    pan.Y -= 1;
        if (Input.IsKeyPressed(Key.Down) || Input.IsKeyPressed(Key.S))  pan.Y += 1;

        Position += pan * PanSpeed * (float)delta;
    }

    public override void _Input(InputEvent e)
    {
        if (e is InputEventMouseButton mouse)
        {
            if (mouse.ButtonIndex == MouseButton.WheelUp)
                Zoom += new Vector2(ZoomSpeed, ZoomSpeed);
            else if (mouse.ButtonIndex == MouseButton.WheelDown)
                Zoom -= new Vector2(ZoomSpeed, ZoomSpeed);

            Zoom = Zoom.Clamp(new Vector2(0.1f, 0.1f), new Vector2(5f, 5f));
        }
    }
}
