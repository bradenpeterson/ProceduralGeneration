using Godot;
using System;

public partial class CellularTileMapRenderer : Node2D
{
	[ExportGroup("Draw Style (testing)")]
	[Export] public int CellSizePx { get; set; } = 8;
	[Export] public Color WaterColor { get; set; } = new Color(0.2f, 0.4f, 0.8f);   // true = wall/water
	[Export] public Color GroundColor { get; set; } = new Color(0.4f, 0.6f, 0.3f);  // false = floor/ground

	private bool[,] _grid;
	[Signal] public delegate void GridGeneratedEventHandler(int width, int height, int cellSize);

	/// Renders the grid using the current draw style. Pure visualization - no generation logic.
	public void Render(bool[,] grid)
	{
		_grid = grid;
		if (_grid != null && _grid.GetLength(0) > 0 && _grid.GetLength(1) > 0)
		{
			EmitSignal(SignalName.GridGenerated, _grid.GetLength(0), _grid.GetLength(1), CellSizePx);
		}
		else
		{
			GD.PrintErr("CellularTileMapRenderer: Received empty grid to render");
		}
		QueueRedraw();
	}

	public override void _Draw()
	{
		if (_grid == null || _grid.GetLength(0) == 0 || _grid.GetLength(1) == 0)
			return;

		int w = _grid.GetLength(0);
		int h = _grid.GetLength(1);

		for (int x = 0; x < w; x++)
		{
			for (int y = 0; y < h; y++)
			{
				float px = x * CellSizePx;
				float py = y * CellSizePx;
				Color c = _grid[x, y] ? WaterColor : GroundColor;
				DrawRect(new Rect2(px, py, CellSizePx, CellSizePx), c);
			}
		}
	}
}
