using Godot;
using System;

public partial class WaveTileMapRenderer : Node2D
{
	[Export] public int TileSet { get; set; } = 0;
	[Export] public int TileId { get; set; } = 0;
	[Export] public int CellSizePx { get; set; } = 16;

	private TileMapLayer _grassLayer;
	private TileMapLayer _pathLayer;
	private int[,] _grid;
	[Signal] public delegate void GridGeneratedEventHandler(int width, int height, int cellSize);

	public override void _Ready()
	{
		_grassLayer = GetNode<TileMapLayer>("GrassLayer");
		_pathLayer = GetNode<TileMapLayer>("PathLayer");
	}

	public void Render(int[,] grid)
	{
		_grid = grid;
		if (_grid != null && _grid.GetLength(0) <= 0 && _grid.GetLength(1) <= 0)
		{
			_grassLayer?.Clear();
			_pathLayer?.Clear();
			return;
		}

		int w = _grid.GetLength(0);
		int h = _grid.GetLength(1);

		_grassLayer.Clear();
		_pathLayer.Clear();

		for (int x = 0; x < w; x++)
		{
			for (int y = 0; y < h; y++)
			{
				_grassLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(1, 1));
				_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(12, 0));     // CHANGE TO THE CORRECT TILE ID
			}
		}

		EmitSignal(SignalName.GridGenerated, w, h, CellSizePx);
	}
}
