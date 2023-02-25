using Godot;
using System;

public partial class DialogButtonHelper : Button
{
	// Called when the node enters the scene tree for the first time.
	private Callable StupidCallable;

	public Node3D Player;

	public override void _Ready()
	{
		StupidCallable = new Callable(this, nameof(OnPressed));
		var _x = Connect("pressed", new Callable(this, nameof(OnPressed)));
		GD.Print("READDY BUTTON, ", _x);
		
	}

	private void OnPressed()
	{

		Player.Call("_DialogChoicePicked", this.Text);
		this.Disabled = true;
		this.Disconnect(SignalName.Pressed, StupidCallable);
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	// public override void _Process(double delta)
	// {
	// }
}
