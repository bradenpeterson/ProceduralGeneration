using Godot;
using System;

public partial class RandomWalkParameters : Control
{
	private RandomWalkController _controller;
	private SpinBox _minStepsSpinBox;
	private SpinBox _maxStepsSpinBox;
	private SpinBox _stepChanceSpinBox;
	private CheckButton _allowLoopsCheckBox;
	private CheckButton _allowBranchesCheckBox;
	private CheckButton _allowConnectionsCheckBox;
	private SpinBox _branchChanceSpinBox;
	private LineEdit _seedLineEdit;
	private Button _regenerateButton;

	public override void _Ready()
	{
		_controller = GetNode<RandomWalkController>("../../RandomWalkController");
		_minStepsSpinBox = GetNode<SpinBox>("ControlPanel/MinStepsBox/MinStepsInput");
		_maxStepsSpinBox = GetNode<SpinBox>("ControlPanel/MaxStepsBox/MaxStepsInput");
		_stepChanceSpinBox = GetNode<SpinBox>("ControlPanel/StepChanceBox/StepChanceInput");
		_allowLoopsCheckBox = GetNode<CheckButton>("ControlPanel/AllowLoopsBox/AllowLoopsInput");
		_allowBranchesCheckBox = GetNode<CheckButton>("ControlPanel/AllowBranchesBox/AllowBranchesInput");
		_allowConnectionsCheckBox = GetNode<CheckButton>("ControlPanel/AllowConnectionsBox/AllowConnectionsInput");
		_branchChanceSpinBox = GetNode<SpinBox>("ControlPanel/BranchChanceBox/BranchChanceInput");
		_seedLineEdit = GetNode<LineEdit>("ControlPanel/SeedBox/SeedInput");
		_regenerateButton = GetNode<Button>("ControlPanel/RegenerateButton");

		_minStepsSpinBox.Value = _controller.MinSteps;
		_maxStepsSpinBox.Value = _controller.MaxSteps;
		_stepChanceSpinBox.Value = _controller.StepChance;
		_allowLoopsCheckBox.ButtonPressed = _controller.AllowLoops;
		_allowBranchesCheckBox.ButtonPressed = _controller.AllowBranches;
		_allowConnectionsCheckBox.ButtonPressed = _controller.AllowConnections;
		_branchChanceSpinBox.Value = _controller.BranchChance;
		_seedLineEdit.Text = _controller.Seed.ToString();

		_regenerateButton.Pressed += OnRegeneratePressed;
	}

	private void OnRegeneratePressed()
	{
		_controller.MinSteps = (int)_minStepsSpinBox.Value;
		_controller.MaxSteps = (int)_maxStepsSpinBox.Value;
		_controller.StepChance = (float)_stepChanceSpinBox.Value;
		_controller.AllowLoops = _allowLoopsCheckBox.ButtonPressed;
		_controller.AllowBranches = _allowBranchesCheckBox.ButtonPressed;
		_controller.AllowConnections = _allowConnectionsCheckBox.ButtonPressed;
		_controller.BranchChance = (float)_branchChanceSpinBox.Value;

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
