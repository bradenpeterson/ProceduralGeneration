using Godot;

public partial class AlgorithmScene : Node2D
{
	private PopUpMenuController _popUpMenu;

	public override void _Ready()
	{
		_popUpMenu = GetNode<PopUpMenuController>("ContextMenuLayer/PopUpMenu");
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton &&
		    mouseButton.ButtonIndex == MouseButton.Right &&
		    mouseButton.Pressed)
		{
			_popUpMenu.ShowAt(GetViewport().GetMousePosition());
			GetViewport().SetInputAsHandled();
		}
	}
}
