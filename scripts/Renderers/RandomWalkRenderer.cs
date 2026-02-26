using Godot;
using System;

public partial class RandomWalkRenderer : Node2D
{
	[Export] public int CellSizePx { get; set; } = 16;
	[Export] public float RoomSize { get; set; } = 3;
	[Export] public float GapSize { get; set; } = 1;
	private float StrideBlocks => RoomSize + GapSize;

	[Export] public Color EdgeColor { get; set; } = new Color("ffffff");
	[Export] public Color RoomColor { get; set; } = new Color("000000");

	[Signal] public delegate void GridGeneratedEventHandler(int width, int height, int cellSize);

	private RandomWalkGenerator.RandomWalkResult _result;

	public void Render(RandomWalkGenerator.RandomWalkResult result)
	{
		if (result.Grid == null || result.Grid.GetLength(0) == 0 || result.Grid.GetLength(1) == 0)
		{
			QueueRedraw();
			return;
		}

		_result = result;

		

		EmitSignal(SignalName.GridGenerated, _result.Grid.GetLength(0), _result.Grid.GetLength(1), CellSizePx * StrideBlocks);
		QueueRedraw();
	}

	public override void _Draw()
	{
		if (_result.Grid == null)
			return;

		int width = _result.Grid.GetLength(0);
		int height = _result.Grid.GetLength(1);
		if (width <= 0 || height <= 0)
			return;

		if (_result.Edges != null && _result.Edges.Count > 0)
		{
			float halfRoomSize = RoomSize / 2;
			float hallwayWidth = CellSizePx * GapSize / 2;

			foreach (var edge in _result.Edges)
			{
				var fromCenter = new Vector2((edge.FromX * StrideBlocks + halfRoomSize) * CellSizePx, (edge.FromY * StrideBlocks + halfRoomSize) * CellSizePx);
				var toCenter = new Vector2((edge.ToX * StrideBlocks + halfRoomSize) * CellSizePx, (edge.ToY * StrideBlocks + halfRoomSize) * CellSizePx);
				DrawLine(fromCenter, toCenter, EdgeColor, hallwayWidth);
				
			}
		}

		if (_result.Grid != null && _result.Grid.GetLength(0) > 0 && _result.Grid.GetLength(1) > 0)
		{
			for (int x = 0; x < _result.Grid.GetLength(0); x++)
			{
				for (int y = 0; y < _result.Grid.GetLength(1); y++)
				{
					if (_result.Grid[x, y])
						DrawRect(new Rect2(x * CellSizePx * StrideBlocks, y * CellSizePx * StrideBlocks, CellSizePx * RoomSize, CellSizePx * RoomSize), RoomColor);
				}
			}
		}
	}

}
