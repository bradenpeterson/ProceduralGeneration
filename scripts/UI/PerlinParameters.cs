using Godot;
using System;

public partial class PerlinParameters : Control
{
	private PerlinController _controller;
	private SpinBox _widthSpinBox;
	private SpinBox _heightSpinBox;
	private CheckBox _fbmCheckBox;
	private SpinBox _octavesSpinBox;
	private LineEdit _seedLineEdit;
	private SpinBox _deepWaterSpinBox;
	private SpinBox _shallowWaterSpinBox;
	private SpinBox _beachSpinBox;
	private SpinBox _grassSpinBox;
	private SpinBox _mountainSpinBox;
	private Button _regenerateButton;

	public override void _Ready()
	{
		_controller = GetNode<PerlinController>("../../PerlinController");	
		_widthSpinBox = GetNode<SpinBox>("ControlPanel/WidthBox/WidthInput");
		_heightSpinBox = GetNode<SpinBox>("ControlPanel/HeightBox/HeightInput");
		_fbmCheckBox = GetNode<CheckBox>("ControlPanel/FBMBox/FBMInput");
		_octavesSpinBox = GetNode<SpinBox>("ControlPanel/OctavesBox/OctavesInput");
		_seedLineEdit = GetNode<LineEdit>("ControlPanel/SeedBox/SeedInput");
		_deepWaterSpinBox = GetNode<SpinBox>("ControlPanel/DeepWaterBox/DeepWaterInput");
		_shallowWaterSpinBox = GetNode<SpinBox>("ControlPanel/ShallowWaterBox/ShallowWaterInput");
		_beachSpinBox = GetNode<SpinBox>("ControlPanel/BeachBox/BeachInput");
		_grassSpinBox = GetNode<SpinBox>("ControlPanel/GrassBox/GrassInput");
		_mountainSpinBox = GetNode<SpinBox>("ControlPanel/MountainBox/MountainInput");
		_regenerateButton = GetNode<Button>("ControlPanel/RegenerateButton");

		// Initialize UI from controller's current values
		_widthSpinBox.Value = _controller.Width;
		_heightSpinBox.Value = _controller.Height;
		_fbmCheckBox.ButtonPressed = _controller.FBM;
		_octavesSpinBox.Value = _controller.Octaves;
		_seedLineEdit.Text = _controller.Seed.ToString();
		_deepWaterSpinBox.Value = _controller.DeepWaterThreshold;
		_shallowWaterSpinBox.Value = _controller.ShallowWaterThreshold;
		_beachSpinBox.Value = _controller.BeachThreshold;
		_grassSpinBox.Value = _controller.GrassThreshold;
		_mountainSpinBox.Value = _controller.MountainThreshold;

		_regenerateButton.Pressed += OnRegeneratePressed;
	}

	private void OnRegeneratePressed()
	{
		_controller.Width = (int)_widthSpinBox.Value;
		_controller.Height = (int)_heightSpinBox.Value;
		_controller.FBM = _fbmCheckBox.ButtonPressed;
		_controller.Octaves = (int)_octavesSpinBox.Value;

		if (int.TryParse(_seedLineEdit.Text, out int seed))	
		{
			_controller.Seed = seed;
		}
		else
		{
			_controller.Seed = 0;
		}

		_controller.DeepWaterThreshold = (float)_deepWaterSpinBox.Value;
		_controller.ShallowWaterThreshold = (float)_shallowWaterSpinBox.Value;
		_controller.BeachThreshold = (float)_beachSpinBox.Value;
		_controller.GrassThreshold = (float)_grassSpinBox.Value;
		_controller.MountainThreshold = (float)_mountainSpinBox.Value;

		_controller.Regenerate();
	}
}
