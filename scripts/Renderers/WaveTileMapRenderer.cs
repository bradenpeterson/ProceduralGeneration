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
		if (_grid == null || grid.GetLength(0) <= 0 || grid.GetLength(1) <= 0)
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

				switch (grid[x, y])
				{
					case 0:
						_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(15, 4)); // Empty
						break;
					case 1:
						_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(15, 0)); // Top
						break;
					case 2:
						_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(15, 1)); // Right
						break;
					case 3:
						_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(12, 1)); // Top Right
						break;
					case 4:
						_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(14, 1)); // Bottom
						break;
					case 5:
						_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(12, 2)); // Top Bottom
						break;
					case 6:
						_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(12, 0)); // Right Bottom
						break;
					case 7:
						_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(14, 3)); // Top Right Bottom
						break;
					case 8:
						_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(14, 0)); // Left
						break;
					case 9:
						_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(13, 1)); // Top Left
						break;
					case 10:
						_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(12, 1)); // Right Left
						break;
					case 11:
						_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(14, 2)); // Top Right Left
						break;
					case 12:
						_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(13, 0)); // Bottom Left
						break;
					case 13:
						_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(15, 2)); // Top Left Bottom
						break;
					case 14:
						_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(15, 3)); // Right Bottom Left
						break;
					case 15:
						_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(13, 3)); // Top Right Bottom Left
						break;
					default:
						_pathLayer.SetCell(new Vector2I(x, y), TileSet, new Vector2I(15, 4)); // Empty
						break;
				}
			}
		}

		EmitSignal(SignalName.GridGenerated, w, h, CellSizePx);
	}
}
