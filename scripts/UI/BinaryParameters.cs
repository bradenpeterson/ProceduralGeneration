using Godot;
using System;

public partial class BinaryParameters : Control
{
	private BinaryController _controller;
	private SpinBox _widthSpinBox;
	private SpinBox _heightSpinBox;
	private SpinBox _minDepthSpinBox;
	private SpinBox _maxDepthSpinBox;
	private SpinBox _splitChanceSpinBox;
	private LineEdit _seedLineEdit;
	private Button _regenerateButton;

	public override void _Ready()
	{
		_controller = GetNode<BinaryController>("../../BinaryController");
		_widthSpinBox = GetNode<SpinBox>("ControlPanel/WidthBox/WidthInput");
		_heightSpinBox = GetNode<SpinBox>("ControlPanel/HeightBox/HeightInput");
		_minDepthSpinBox = GetNode<SpinBox>("ControlPanel/MinDepthBox/MinDepthInput");
		_maxDepthSpinBox = GetNode<SpinBox>("ControlPanel/MaxDepthBox/MaxDepthInput");
		_splitChanceSpinBox = GetNode<SpinBox>("ControlPanel/SplitChanceBox/SplitChanceInput");
		_seedLineEdit = GetNode<LineEdit>("ControlPanel/SeedBox/SeedInput");
		_regenerateButton = GetNode<Button>("ControlPanel/RegenerateButton");
	
		// Initialize UI from controller's current values
		_widthSpinBox.Value = _controller.Width;
		_heightSpinBox.Value = _controller.Height;
		_minDepthSpinBox.Value = _controller.MinDepth;
		_maxDepthSpinBox.Value = _controller.MaxDepth;
		_splitChanceSpinBox.Value = _controller.SplitChance;
		_seedLineEdit.Text = _controller.Seed.ToString();
	
		_regenerateButton.Pressed += OnRegeneratePressed;
	}

	private void OnRegeneratePressed()
	{
		_controller.Width = (int)_widthSpinBox.Value;
		_controller.Height = (int)_heightSpinBox.Value;
		_controller.MinDepth = (int)_minDepthSpinBox.Value;
		_controller.MaxDepth = (int)_maxDepthSpinBox.Value;
		_controller.SplitChance = (float)_splitChanceSpinBox.Value;

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
