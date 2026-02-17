using Godot;
using System;

public partial class WaveTileMapRenderer : Node2D
{
	[Export] public int TileSet { get; set; } = 0;
	[Export] public int CellSizePx { get; set; } = 16;

	private TileMapLayer _grassLayer;
	private TileMapLayer _pathLayer;
	private int[,] _grid;
	[Signal] public delegate void GridGeneratedEventHandler(int width, int height, int cellSize);

	// WFC tile index (0-15) -> atlas coords (source column, source row) for path layer
	private static readonly Vector2I[] TileIndexToAtlas = new Vector2I[]
	{
		new Vector2I(15, 4), // 0 Empty
		new Vector2I(15, 0), // 1 Top
		new Vector2I(15, 1), // 2 Right
		new Vector2I(12, 1), // 3 Top Right
		new Vector2I(14, 1), // 4 Bottom
		new Vector2I(12, 2), // 5 Top Bottom
		new Vector2I(12, 0), // 6 Right Bottom
		new Vector2I(14, 3), // 7 Top Right Bottom
		new Vector2I(14, 0), // 8 Left
		new Vector2I(13, 1), // 9 Top Left
		new Vector2I(13, 2), // 10 Right Left
		new Vector2I(14, 2), // 11 Top Right Left
		new Vector2I(13, 0), // 12 Bottom Left
		new Vector2I(15, 2), // 13 Top Left Bottom
		new Vector2I(15, 3), // 14 Right Bottom Left
		new Vector2I(13, 3), // 15 Top Right Bottom Left
	};

	public override void _Ready()
	{
		_grassLayer = GetNode<TileMapLayer>("GrassLayer");
		_pathLayer = GetNode<TileMapLayer>("PathLayer");
	}

	// Renders the WFC output grid: grass under every cell, path layer from tile indices.
	public void Render(int[,] grid)
	{
		if (grid == null || grid.GetLength(0) <= 0 || grid.GetLength(1) <= 0)
		{
			_grassLayer?.Clear();
			_pathLayer?.Clear();
			return;
		}

		int w = grid.GetLength(0);
		int h = grid.GetLength(1);
		_grassLayer.Clear();
		_pathLayer.Clear();

		for (int x = 0; x < w; x++)
		{
			for (int y = 0; y < h; y++)
			{
				Vector2I cell = new Vector2I(x, y);
				_grassLayer.SetCell(cell, TileSet, new Vector2I(1, 1));
				int tileIndex = grid[x, y];
				Vector2I atlas = tileIndex >= 0 && tileIndex < TileIndexToAtlas.Length
					? TileIndexToAtlas[tileIndex]
					: TileIndexToAtlas[0];
				_pathLayer.SetCell(cell, TileSet, atlas);
			}
		}

		EmitSignal(SignalName.GridGenerated, w, h, CellSizePx);
	}
}
