using Godot;
using System;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	public override void _Ready()
	{
		Instance = this;
	}

	public void GoToCellularAutomata()
	{
		ChangeScene(ResourceLoader.Load<PackedScene>("res://scenes/CellularAutomata.tscn"));
	}

	public void GoToPerlinNoise()
	{
		ChangeScene(ResourceLoader.Load<PackedScene>("res://scenes/PerlinNoise.tscn"));
	}

	public void GoToWaveFunctionCollapse()
	{
		ChangeScene(ResourceLoader.Load<PackedScene>("res://scenes/WaveCollapse.tscn"));
	}

	public void ChangeScene(PackedScene scenePath)
	{
		GetTree().ChangeSceneToPacked(scenePath);
	}

	public void GoToMainMenu()
	{
		ChangeScene(ResourceLoader.Load<PackedScene>("res://scenes/MainMenu.tscn"));
	}


}
