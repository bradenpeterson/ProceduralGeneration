using Godot;
using System;

/// Controller that orchestrates cellular automata generation and rendering.
/// Keeps algorithm logic separate from visualization.
public partial class CellularAutomataController : Node
{
	[ExportGroup("Generation Parameters")]
	[Export] public int Width { get; set; } = 1000;
	[Export] public int Height { get; set; } = 1000;
	[Export] public float FillProbability { get; set; } = 0.5f;
	[Export] public int Iterations { get; set; } = 6;
	[Export] public int BirthThreshold { get; set; } = 5;
	[Export] public int SurvivalThreshold { get; set; } = 4;
	[Export] public int Seed { get; set; } = -1;

	private CellularTileMapRenderer _renderer;

	public override void _Ready()
	{
		// Find the renderer in the scene tree (could be sibling or child)
		_renderer = GetParent()?.GetNodeOrNull<CellularTileMapRenderer>("CellularTileMapRenderer");
		if (_renderer == null)
		{
			GD.PrintErr("CellularAutomataController: Could not find CellularTileMapRenderer in scene tree");
			return;
		}
		Regenerate();
	}

	/// Generates a new grid using the algorithm and passes it to the renderer.
	public void Regenerate()
	{
		bool[,] grid = CellularAutomataGenerator.Generate(
			Width, Height, FillProbability, Iterations, BirthThreshold, SurvivalThreshold,
			Seed >= 0 ? Seed : (int?)null);
		_renderer.Render(grid);
	}
}
