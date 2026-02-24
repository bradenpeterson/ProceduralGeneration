using Godot;
using System;
using System.Collections.Generic;

public partial class BinaryRenderer : Node2D
{
	[Export] public int CellSizePx { get; set; } = 16;
	[Export] public Color WallColor { get; set; } = new Color(0.1f, 0.1f, 0.1f);
	[Export] public Color RoomColor { get; set; } = new Color(0.8f, 0.8f, 0.8f);
	[Export] public Color CorridorColor { get; set; } = new Color(0.6f, 0.6f, 0.6f);
	
	private List<Rect2I> _roomRects;
	private List<Rect2I> _corridorRects;

	[Signal] public delegate void GridGeneratedEventHandler(int width, int height, int cellSize);

	public void Render(BspResult result)
	{
		_roomRects = new List<Rect2I>();
		_corridorRects = new List<Rect2I>();

		if (result.Rooms != null)
		{
			foreach (var r in result.Rooms)
				_roomRects.Add(new Rect2I(r.X, r.Y, r.Width, r.Height));
		}

		if (result.Corridors != null)
		{
			foreach (var c in result.Corridors)
				_corridorRects.Add(new Rect2I(c.X, c.Y, c.Width, c.Height));
		}

		EmitSignal(SignalName.GridGenerated, result.Width, result.Height, CellSizePx);
		QueueRedraw();
	}
}
