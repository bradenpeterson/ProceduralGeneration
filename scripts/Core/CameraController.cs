using Godot;
using System;

public partial class CameraController : Camera2D
{
	[Export] public float ZoomMin { get; set; } = 0.25f;
	[Export] public float ZoomMax { get; set; } = 4.0f;
	[Export] public float ZoomStep { get; set; } = 1.15f;

	private bool _isDragging;
	private Vector2 _lastMouseWorld;

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left)
			{
				if (mouseButton.Pressed)
				{
					_isDragging = true;
					_lastMouseWorld = GetGlobalMousePosition();
				}
				else
				{
					_isDragging = false;
				}
			}
			else if (mouseButton.ButtonIndex == MouseButton.WheelUp && mouseButton.Pressed)
			{
				ZoomTowardPoint(GetGlobalMousePosition(), ZoomStep);
			}
			else if (mouseButton.ButtonIndex == MouseButton.WheelDown && mouseButton.Pressed)
			{
				ZoomTowardPoint(GetGlobalMousePosition(), 1.0f / ZoomStep);
			}
		}
		else if (@event is InputEventMouseMotion motion && _isDragging)
		{
			Vector2 currentMouseWorld = GetGlobalMousePosition();
			Position -= currentMouseWorld - _lastMouseWorld;
			_lastMouseWorld = currentMouseWorld;
		}
		else if (@event is InputEventMagnifyGesture magnify)
		{
			// Mac trackpad pinch-to-zoom sends this instead of WheelUp/WheelDown
			ZoomTowardPoint(GetGlobalMousePosition(), magnify.Factor);
		}
	}

	private void ZoomTowardPoint(Vector2 worldPoint, float zoomFactor)
	{
		Vector2 oldZoom = Zoom;
		Vector2 newZoom = (oldZoom * zoomFactor).Clamp(Vector2.One * ZoomMin, Vector2.One * ZoomMax);
		if (newZoom == oldZoom)
			return;

		Position = worldPoint - (worldPoint - Position) * (newZoom.X / oldZoom.X);
		Zoom = newZoom;
	}
}
