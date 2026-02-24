using Godot;
using System;

public partial class RandomWalkParameters : Control
{
	private RandomWalkController _controller;
	private SpinBox _minStepsSpinBox;
	private SpinBox _maxStepsSpinBox;
	private SpinBox _stepChanceSpinBox;
	private CheckButton _allowLoopsCheckBox;
	private CheckButton _allowBranchingCheckBox;
	private CheckButton _allowConnectingBranchesCheckBox;
	private SpinBox _branchingChanceSpinBox;
	private LineEdit _seedLineEdit;

	public override void _Ready()
	{
		_controller = GetNode<RandomWalkController>("../../RandomWalkController");
		_minStepsSpinBox = GetNode<SpinBox>("ControlPanel/MinStepsBox/MinStepsInput");
		_maxStepsSpinBox = GetNode<SpinBox>("ControlPanel/MaxStepsBox/MaxStepsInput");
		_stepChanceSpinBox = GetNode<SpinBox>("ControlPanel/StepChanceBox/StepChanceInput");
		_allowLoopsCheckBox = GetNode<CheckButton>("ControlPanel/AllowLoopsBox/AllowLoopsInput");
		_allowBranchingCheckBox = GetNode<CheckButton>("ControlPanel/AllowBranchingBox/AllowBranchingInput");
		_allowConnectingBranchesCheckBox = GetNode<CheckButton>("ControlPanel/AllowConnectingBranchesBox/AllowConnectingBranchesInput");
		_branchingChanceSpinBox = GetNode<SpinBox>("ControlPanel/BranchingChanceBox/BranchingChanceInput");
		_seedLineEdit = GetNode<LineEdit>("ControlPanel/SeedBox/SeedInput");
		_regenerateButton = GetNode<Button>("ControlPanel/RegenerateButton");

		_minStepsSpinBox.Value = _controller.MinSteps;
		_maxStepsSpinBox.Value = _controller.MaxSteps;
		_stepChanceSpinBox.Value = _controller.StepChance;
		_allowLoopsCheckBox.Pressed = _controller.AllowLoops;
		_allowBranchingCheckBox.Pressed = _controller.AllowBranching;
		_allowConnectingBranchesCheckBox.Pressed = _controller.AllowConnectingBranches;
		_branchingChanceSpinBox.Value = _controller.BranchingChance;
		_seedLineEdit.Text = _controller.Seed.ToString();

		_regenerateButton.Pressed += OnRegeneratePressed;
	}

	private void OnRegeneratePressed()
	{
		_controller.MinSteps = (int)_minStepsSpinBox.Value;
		_controller.MaxSteps = (int)_maxStepsSpinBox.Value;
		_controller.StepChance = (float)_stepChanceSpinBox.Value;
		_controller.AllowLoops = _allowLoopsCheckBox.Pressed;
		_controller.AllowBranching = _allowBranchingCheckBox.Pressed;
		_controller.AllowConnectingBranches = _allowConnectingBranchesCheckBox.Pressed;
		_controller.BranchingChance = (int)_branchingChanceSpinBox.Value;

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
