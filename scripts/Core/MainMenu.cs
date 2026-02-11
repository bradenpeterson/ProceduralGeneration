using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<Button>("VBoxContainer/CellularAutomataButton").Pressed += GameManager.Instance.GoToCellularAutomata;
		GetNode<Button>("VBoxContainer/PerlinNoiseButton").Pressed += GameManager.Instance.GoToPerlinNoise;
		GetNode<Button>("VBoxContainer/WaveCollapseButton").Pressed += GameManager.Instance.GoToWaveFunctionCollapse;
		GetNode<Button>("VBoxContainer/Exit").Pressed += () => GetTree().Quit();
	}
}
