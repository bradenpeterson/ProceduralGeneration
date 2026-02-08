using Godot;
using System;

public partial class CameraController : Camera2D
{
	[Export] public float ZoomMin { get; set; } = 0.25f;
	[Export] public float ZoomMax { get; set; } = 4.0f;
	[Export] public float ZoomStep { get; set; } = 1.15f;

	private bool _isDragging;
	private Vector2 _dragStartCameraPos;
	private Vector2 _dragStartMousePos;

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left)
			{
				if (mouseButton.Pressed)
				{
					_isDragging = true;
					_dragStartCameraPos = Position;
					_dragStartMousePos = GetGlobalMousePosition();
					GetViewport().SetInputAsHandled();
				}
				else
				{
					_isDragging = false;
					GetViewport().SetInputAsHandled();
				}
			}
			else if (mouseButton.ButtonIndex == MouseButton.WheelUp && mouseButton.Pressed)
			{
				ZoomTowardPoint(GetGlobalMousePosition(), ZoomStep);
				GetViewport().SetInputAsHandled();
			}
			else if (mouseButton.ButtonIndex == MouseButton.WheelDown && mouseButton.Pressed)
			{
				ZoomTowardPoint(GetGlobalMousePosition(), 1.0f / ZoomStep);
				GetViewport().SetInputAsHandled();
			}
		}
		else if (@event is InputEventMouseMotion && _isDragging)
		{
			Vector2 currentMouseWorld = GetGlobalMousePosition();
			Position = _dragStartCameraPos - (currentMouseWorld - _dragStartMousePos);
			GetViewport().SetInputAsHandled();
		}
		else if (@event is InputEventMagnifyGesture magnify)
		{
			ZoomTowardPoint(GetGlobalMousePosition(), magnify.Factor);
			GetViewport().SetInputAsHandled();
		}
	}

	private void ZoomTowardPoint(Vector2 worldPoint, float zoomFactor)
	{
		Vector2 oldZoom = Zoom;
		Vector2 newZoom = (oldZoom * zoomFactor).Clamp(Vector2.One * ZoomMin, Vector2.One * ZoomMax);
		if (newZoom == oldZoom)
			return;

		// Keep worldPoint under the cursor
		float ratio = oldZoom.X / newZoom.X;
		Position = worldPoint - (worldPoint - Position) * ratio;
		Zoom = newZoom;
	}
}