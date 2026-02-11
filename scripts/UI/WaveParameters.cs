using Godot;
using System;

public partial class WaveParameters : Control
{
	private WaveCollapseController _controller;
	private SpinBox _widthSpinBox;
	private SpinBox _heightSpinBox;
	private OptionButton _tileTypeOptionButton;
	private LineEdit _seedLineEdit;
	private Button _regenerateButton;

	public override void _Ready()
	{
		_controller = GetNode<WaveCollapseController>("../../WaveCollapseController");
		_widthSpinBox = GetNode<SpinBox>("ControlPanel/WidthBox/WidthInput");
		_heightSpinBox = GetNode<SpinBox>("ControlPanel/HeightBox/HeightInput");
		_tileTypeOptionButton = GetNode<OptionButton>("ControlPanel/TileTypeBox/TileTypeInput");
		_seedLineEdit = GetNode<LineEdit>("ControlPanel/SeedBox/SeedInput");
		_regenerateButton = GetNode<Button>("ControlPanel/RegenerateButton");
	
		// Initialize UI from controller's current values
		_widthSpinBox.Value = _controller.Width;
		_heightSpinBox.Value = _controller.Height;
		_tileTypeOptionButton.Selected = (int)_controller.TileType;
		_seedLineEdit.Text = _controller.Seed.ToString();

		_regenerateButton.Pressed += OnRegeneratePressed;
	}

	private void OnRegeneratePressed()
	{
		_controller.Width = (int)_widthSpinBox.Value;
		_controller.Height = (int)_heightSpinBox.Value;
		_controller.TileType = (int)_tileTypeOptionButton.Selected;

		if (int.TryParse(_seedLineEdit.Text, out int seed))	
		{
			_controller.Seed = seed;
		}
		else
		{
			_controller.Seed = 0;
		}

		_controller.Regenerate();
	}

}
