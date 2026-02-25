using Godot;
using System;

public partial class RandomWalkRenderer : Node2D
{
	[Export] public int CellSizePx { get; set; } = 16;

	[Signal] public delegate void GridGeneratedEventHandler(int width, int height, int cellSize);


	public void Render(bool[,] grid)
	{


		EmitSignal(SignalName.GridGenerated, grid.GetLength(0), grid.GetLength(1), CellSizePx);
	}
}
