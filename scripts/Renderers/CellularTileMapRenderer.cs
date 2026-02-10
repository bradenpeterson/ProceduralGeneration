using Godot;
using System;

public partial class CellularTileMapRenderer : Node2D
{
	[Export] public int TerrainSet { get; set; } = 0;
	[Export] public int TerrainId { get; set; } = 0;
	[Export] public int WaterSourceId { get; set; } = 0;
	[Export] public Vector2I WaterSourceAtlasCoords { get; set; } = new Vector2I(1, 1);
	[Export] public int CellSizePx { get; set; } = 16;

	private TileMapLayer _tileMapLayer;
	private bool[,] _grid;
	[Signal] public delegate void GridGeneratedEventHandler(int width, int height, int cellSize);

	public override void _Ready()
	{
		_tileMapLayer = GetNode<TileMapLayer>("TileMapLayer");
	}

	/// Renders the grid using the current draw style. Pure visualization - no generation logic.
	public void Render(bool[,] grid)
	{
		_grid = grid;
		if (_grid != null && _grid.GetLength(0) <= 0 && _grid.GetLength(1) <= 0)
		{
			_tileMapLayer?.Clear();
			return;
		}

		int w = _grid.GetLength(0);
		int h = _grid.GetLength(1);

		_tileMapLayer.Clear();

		// Fill a one-cell water border so autotiling at edges has water neighbors (no missing tiles)
		for (int x = -1; x <= w; x++)
		{
			_tileMapLayer.SetCell(new Vector2I(x, -1), WaterSourceId, WaterSourceAtlasCoords, 0);
			_tileMapLayer.SetCell(new Vector2I(x, h), WaterSourceId, WaterSourceAtlasCoords, 0);
		}
		for (int y = 0; y < h; y++)
		{
			_tileMapLayer.SetCell(new Vector2I(-1, y), WaterSourceId, WaterSourceAtlasCoords, 0);
			_tileMapLayer.SetCell(new Vector2I(w, y), WaterSourceId, WaterSourceAtlasCoords, 0);
		}

		Godot.Collections.Array<Vector2I> landCells = new Godot.Collections.Array<Vector2I>();
		for (int x = 0; x < w; x++)
		{
			for (int y = 0; y < h; y++)
			{
				if (_grid[x, y])
					landCells.Add(new Vector2I(x, y));
				else
					_tileMapLayer.SetCell(new Vector2I(x, y), WaterSourceId, WaterSourceAtlasCoords);
			}
		}

		_tileMapLayer.SetCellsTerrainConnect(landCells, TerrainSet, TerrainId, false);
		EmitSignal(SignalName.GridGenerated, w, h, CellSizePx);
	}
}
