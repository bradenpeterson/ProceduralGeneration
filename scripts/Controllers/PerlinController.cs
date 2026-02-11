using Godot;
using System;

public partial class PerlinController : Node
{
	[ExportGroup("Generation Parameters")]
	[Export] public int Width { get; set; } = 1000;
	[Export] public int Height { get; set; } = 1000;
	[Export] public bool FBM { get; set; } = true;
	[Export] public int Octaves { get; set; } = 4;
	[Export] public float Persistence { get; set; } = 0.5f;
	[Export] public float Scale { get; set; } = 0.01f;
	[Export] public int Seed { get; set; } = 0;

	[ExportGroup("Visualization Parameters")]
	[Export] public float DeepWaterThreshold { get; set; } = -0.55f;
	[Export] public float ShallowWaterThreshold { get; set; } = -0.15f;
	[Export] public float BeachThreshold { get; set; } = 0.05f;
	[Export] public float GrassThreshold { get; set; } = 0.45f;
	[Export] public float MountainThreshold { get; set; } = 0.75f;

	private PerlinTileMapRenderer _renderer;
	private CameraController _camera;

	public override void _Ready()
	{
		// Find the renderer in the scene tree (could be sibling or child)
		_renderer = GetParent()?.GetNodeOrNull<PerlinTileMapRenderer>("PerlinTileMapRenderer");
		_camera = GetParent()?.GetNodeOrNull<CameraController>("AlgorithmSceneCamera");
		if (_renderer == null)
		{
			GD.PrintErr("PerlinController: Could not find PerlinTileMapRenderer in scene tree");
			return;
		}

		if (_camera != null)
			_renderer.GridGenerated += _camera.CenterCameraOnGrid;

		Regenerate();
	}

	// Generates a new noise map using the algorithm and passes it to the renderer.
	public void Regenerate()
	{
		float[,] noiseMap = PerlinGenerator.Generate(
			Width, Height, FBM, Octaves, Persistence, Scale,
			Seed > 0 ? Seed : (int?)null);
		_renderer.Render(noiseMap, DeepWaterThreshold, ShallowWaterThreshold, BeachThreshold, GrassThreshold, MountainThreshold);
	}

}
