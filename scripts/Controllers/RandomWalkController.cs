using Godot;
using System;

public partial class RandomWalkController : Node
{
	[ExportGroup("Generation Parameters")]
	[Export] public int MinSteps { get; set; } = 100;
	[Export] public int MaxSteps { get; set; } = 100;
	[Export] public float StepChance { get; set; } = 0.5f;
	[Export] public bool AllowLoops { get; set; } = true;
	[Export] public bool AllowBranches { get; set; } = true;
	[Export] public bool AllowConnections { get; set; } = true;
	[Export] public float BranchChance { get; set; } = 0.5f;
	[Export] public int Seed { get; set; } = 0;

	private RandomWalkRenderer _renderer;
	private CameraController _camera;

	public override void _Ready()
	{
		var parent = GetParent();
		_renderer = parent?.GetNodeOrNull<RandomWalkRenderer>("RandomWalkRenderer");
		_camera = parent?.GetNodeOrNull<CameraController>("AlgorithmSceneCamera");
		if (_renderer == null)
		{
			GD.PrintErr("RandomWalkController: Could not find RandomWalkRenderer in scene tree");
			return;
		}

		if (_camera != null)
			_renderer.GridGenerated += _camera.CenterCameraOnGrid;

		Regenerate();
	}

	public void Regenerate()
	{
		var grid = RandomWalkGenerator.Generate(
			MinSteps, MaxSteps, StepChance, AllowLoops, AllowBranches, 
			AllowConnections, BranchChance, Seed > 0 ? Seed : (int?)null);
		_renderer.Render(grid);
	}
}
