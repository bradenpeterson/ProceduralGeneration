using Godot;

public partial class PopUpMenuController : Node2D
{
	public override void _Ready()
	{
		Hide();
		GetNode<Button>("VBoxContainer/MenuButton").Pressed += OnMainMenuPressed;
		GetNode<Button>("VBoxContainer/ExitButton").Pressed += OnExitPressed;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!Visible)
			return;
		if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
		{
			Vector2 mousePos = GetViewport().GetMousePosition();
			Rect2 menuRect = GetNode<Control>("VBoxContainer").GetGlobalRect();
			if (!menuRect.HasPoint(mousePos))
			{
				Hide();
				GetViewport().SetInputAsHandled();
			}
		}
	}

	public void ShowAt(Vector2 viewportPosition)
	{
		Position = viewportPosition;
		Show();
	}

	private void OnMainMenuPressed()
	{
		GameManager.Instance.GoToMainMenu();
	}

	private void OnExitPressed()
	{
		GetTree().Quit();
	}
}
