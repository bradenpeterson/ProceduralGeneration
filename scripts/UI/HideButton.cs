using Godot;

public partial class HideButton : Button
{
	private BoxContainer _boxContainer;

	public override void _Ready()
	{
		_boxContainer = GetParent().GetNode<BoxContainer>("ControlPanel");
		UpdateButtonText();
	}

	/// When this button is pressed, hide the parameters panel it lives in.
	public override void _Pressed()
	{
		_boxContainer.Visible = !_boxContainer.Visible;
		UpdateButtonText();
	}

	private void UpdateButtonText()
	{
		Text = _boxContainer.Visible ? "Hide Controls" : "Show Controls";
	}
}
