using Godot;
using System;

public partial class BinaryController : Node
{
	[ExportGroup("Generation Parameters")]
	[Export] public int Width { get; set; } = 100;
	[Export] public int Height { get; set; } = 100;
	[Export] public int MinDepth { get; set; } = 1;
	[Export] public int MaxDepth { get; set; } = 10;
	[Export] public float SplitChance { get; set; } = 0.5f;
	[Export] public int Seed { get; set; } = 0;

	private BinaryRenderer _renderer;
	private CameraController _camera;

	public override void _Ready()
	{
		var parent = GetParent();
		_renderer = parent?.GetNodeOrNull<BinaryRenderer>("BinaryRenderer");
		_camera = parent?.GetNodeOrNull<CameraController>("CameraController");
	
		if (_renderer == null)
		{
			GD.PrintErr("BinaryController: Could not find BinarySpacePartitioningRenderer in scene tree");
			return;
		}
		
		if (_camera != null)
			_renderer.GridGenerated += _camera.CenterCameraOnGrid;

		Regenerate();
	}
	
	public void Regenerate()
	{
		int[,] grid = BinarySpacePartitioningGenerator.Generate(Width, Height, MinDepth, MaxDepth, SplitChance, Seed > 0 ? Seed : (int?)null);
		_renderer.Render(grid);
	}
	
	
}
