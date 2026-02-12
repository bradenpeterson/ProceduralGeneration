using Godot;
using System;
using System.Numerics;

public partial class PerlinTileMapRenderer : Node2D
{
	[ExportGroup("TileMap Parameters")]
	[Export] public int CellSizePx { get; set; } = 16;
	[Export] public int DeepWaterTile = 0;
	[Export] public int ShallowWaterTile = 1;
	[Export] public int BeachTile = 2;
	[Export] public int GrassTile = 3;
	[Export] public int MountainTile = 4;
	[Export] public int SnowyPeakTile = 5;

	[ExportGroup("Visualization Parameters")]
	[Export] public float DeepWaterThreshold;
	[Export] public float ShallowWaterThreshold;
	[Export] public float BeachThreshold;
	[Export] public float GrassThreshold;
	[Export] public float MountainThreshold;

	private float[,] _grid;
	private TileMapLayer _tileMapLayer;
	[Signal] public delegate void GridGeneratedEventHandler(int width, int height, int cellSize);


		public override void _Ready()
	{
		_tileMapLayer = GetNode<TileMapLayer>("TileMapLayer");
	}

	public void Render(float[,] grid, float deepWaterThreshold, float shallowWaterThreshold, float beachThreshold, float grassThreshold, float mountainThreshold)
	{
		_grid = grid;
		if (_grid == null || _grid.GetLength(0) <= 0 || _grid.GetLength(1) <= 0)
		{
			_tileMapLayer?.Clear();
			return;
		}

		int w = _grid.GetLength(0);
		int h = _grid.GetLength(1);

		_tileMapLayer.Clear();

		for (int x = 0; x < w; x++)
		{
			for (int y = 0; y < h; y++)
			{
				Vector2I cellPos = new Vector2I(x, y);
				if (_grid[x, y] < deepWaterThreshold)
					_tileMapLayer.SetCell(cellPos, DeepWaterTile, new Vector2I(0, 3));
				else if (_grid[x, y] < shallowWaterThreshold)
					_tileMapLayer.SetCell(cellPos, ShallowWaterTile, new Vector2I(1, 1));
				else if (_grid[x, y] < beachThreshold)
					_tileMapLayer.SetCell(cellPos, BeachTile, new Vector2I(1, 1));
				else if (_grid[x, y] < grassThreshold)
					_tileMapLayer.SetCell(cellPos, GrassTile, new Vector2I(1, 1));
				else if (_grid[x, y] < mountainThreshold)
					_tileMapLayer.SetCell(cellPos, MountainTile, new Vector2I(1, 2));
				else
					_tileMapLayer.SetCell(cellPos, SnowyPeakTile, new Vector2I(1, 2));
			}
		}
		
		EmitSignal(SignalName.GridGenerated, w, h, CellSizePx);
	}

}
