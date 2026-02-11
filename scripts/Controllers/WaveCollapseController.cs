using Godot;
using System;

public partial class WaveCollapseController : Node
{
	[ExportGroup("Generation Parameters")]
	[Export] public int Width { get; set; } = 100;
	[Export] public int Height { get; set; } = 100;
	[Export] public int TileType { get; set; } = 0;
	[Export] public int Seed { get; set; } = 0;

	private WaveCollapseRenderer _renderer;
	private CameraController _camera;

	public override void _Ready()
	{
		var parent = GetParent();
		_renderer = parent?.GetNodeOrNull<WaveCollapseRenderer>("WaveCollapseTileMapRenderer");
		_camera = parent?.GetNodeOrNull<CameraController>("AlgorithmSceneCamera");
		if (_renderer == null)
		{
			GD.PrintErr("WaveCollapseController: Could not find WaveCollapseTileMapRenderer in scene tree");
			return;
		}
		
		if (_camera != null)
			_renderer.GridGenerated += _camera.CenterCameraOnGrid;
	
		// Run synchronously so centering happens before first frame (no visible jump)
		Regenerate();
	}

	public void Regenerate()
	{
		int[,] grid = WaveCollapseGenerator.Generate(Width, Height, TileType, Seed > 0 ? Seed : (int?)null);
		_renderer.Render(grid);
	}
}
