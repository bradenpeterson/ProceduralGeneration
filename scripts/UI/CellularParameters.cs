using Godot;

public partial class CellularParameters : Control
{
	private CellularAutomataController _controller;
	private SpinBox _widthSpinBox;
	private SpinBox _heightSpinBox;
	private SpinBox _numStepsSpinBox;
	private SpinBox _initialDensitySpinBox;
	private LineEdit _seedLineEdit;
	private Button _regenerateButton;

	public override void _Ready()
	{
		_controller = GetNode<CellularAutomataController>("../../CellularAutomataController");
		_widthSpinBox = GetNode<SpinBox>("ControlPanel/WidthBox/WidthInput");
		_heightSpinBox = GetNode<SpinBox>("ControlPanel/HeightBox/HeightInput");
		_numStepsSpinBox = GetNode<SpinBox>("ControlPanel/StepsBox/StepsInput");
		_initialDensitySpinBox = GetNode<SpinBox>("ControlPanel/DensityBox/DensityInput");
		_seedLineEdit = GetNode<LineEdit>("ControlPanel/SeedBox/SeedInput");
		_regenerateButton = GetNode<Button>("ControlPanel/RegenerateButton");

		_regenerateButton.Pressed += OnRegeneratePressed;
	}

	private void OnRegeneratePressed()
	{
		_controller.Width = (int)_widthSpinBox.Value;
		_controller.Height = (int)_heightSpinBox.Value;
		_controller.Iterations = (int)_numStepsSpinBox.Value;
		_controller.FillProbability = (float)_initialDensitySpinBox.Value;

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
