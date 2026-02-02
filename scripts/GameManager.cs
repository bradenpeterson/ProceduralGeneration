using Godot;
using System;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	public override void _Ready()
	{
		if (Instance != null)
		{
			GD.PrintErr("Multiple instances of GameManager detected! There should only be one instance.");
			QueueFree();
			return;
		}

		Instance = this;
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
