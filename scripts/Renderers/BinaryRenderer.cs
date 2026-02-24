using Godot;
using System;
using System.Collections.Generic;

public partial class BinaryRenderer : Node2D
{
	[Export] public int CellSizePx { get; set; } = 16;
	[Export] public Color TileColor { get; set; } = new Color(0.6f, 0.6f, 0.6f);
	
	private List<Rect2I> _roomRects;
	private List<Rect2I> _corridorRects;

	[Signal] public delegate void GridGeneratedEventHandler(int width, int height, int cellSize);

	public void Render(BspResult result, int width, int height)
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

		EmitSignal(SignalName.GridGenerated, width, height, CellSizePx);
		QueueRedraw();
	}

	public override void _Draw()
	{
		// Nothing to draw yet
		if (_roomRects == null && _corridorRects == null)
			return;

		// Draw rooms
		if (_roomRects != null)
		{
			foreach (var r in _roomRects)
			{
				var pos = new Vector2(r.Position.X * CellSizePx, r.Position.Y * CellSizePx);
				var size = new Vector2(r.Size.X * CellSizePx, r.Size.Y * CellSizePx);
				DrawRect(new Rect2(pos, size), TileColor, filled: true);
			}
		}

		// Draw corridors
		if (_corridorRects != null)
		{
			foreach (var c in _corridorRects)
			{
				var pos = new Vector2(c.Position.X * CellSizePx, c.Position.Y * CellSizePx);
				var size = new Vector2(c.Size.X * CellSizePx, c.Size.Y * CellSizePx);
				DrawRect(new Rect2(pos, size), TileColor, filled: true);
			}
		}
	}
}
