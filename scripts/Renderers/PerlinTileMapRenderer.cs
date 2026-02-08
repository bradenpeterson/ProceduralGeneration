using Godot;
using System;

public partial class PerlinTileMapRenderer : Node2D
{
	[ExportGroup("Draw Style (testing)")]
	[Export] public int CellSizePx { get; set; } = 8;
	[Export] public Color DeepWaterColor { get; set; } = new Color(0.1f, 0.2f, 0.5f);
	[Export] public Color ShallowWaterColor { get; set; } = new Color(0.2f, 0.4f, 0.8f);
	[Export] public Color BeachColor { get; set; } = new Color(0.9f, 0.8f, 0.6f);
	[Export] public Color GrassColor { get; set; } = new Color(0.4f, 0.6f, 0.3f);
	[Export] public Color MountainColor { get; set; } = new Color(0.5f, 0.5f, 0.5f);

	[ExportGroup("Visualization Parameters")]
	[Export] public float DeepWaterThreshold;
	[Export] public float ShallowWaterThreshold;
	[Export] public float BeachThreshold;
	[Export] public float GrassThreshold;
	[Export] public float MountainThreshold;

	[Signal] public delegate void GridGeneratedEventHandler(int width, int height, int cellSize);

	private float[,] _grid;
	public void Render(float[,] grid, float deepWaterThreshold, float shallowWaterThreshold, float beachThreshold, float grassThreshold, float mountainThreshold)
	{
		if (_grid != null && _grid.GetLength(0) > 0 && _grid.GetLength(1) > 0)
		{
			EmitSignal(SignalName.GridGenerated, _grid.GetLength(0), _grid.GetLength(1), CellSizePx);
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
				Color c;
				if (_grid[x, y] < DeepWaterThreshold)
					c = DeepWaterColor;
				else if (_grid[x, y] < ShallowWaterThreshold)
					c = ShallowWaterColor;
				else if (_grid[x, y] < BeachThreshold)
					c = BeachColor;
				else if (_grid[x, y] < GrassThreshold)
					c = GrassColor;
				else
					c = MountainColor;
				DrawRect(new Rect2(px, py, CellSizePx, CellSizePx), c);
			}
		}
	}

}
