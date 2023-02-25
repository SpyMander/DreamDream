#if TOOLS
using Godot;
using System;

[Tool]
public partial class dialogmakerplugin : EditorPlugin
{
	
	Control Dock;
	public override void _EnterTree()
	{
		GD.Print("Dialog Maker Started");
		// Initialization of the plugin goes here.
		Dock = GD.Load<PackedScene>("res://scenes/editor/DialogPannel.tscn").Instantiate<Control>();

		AddControlToBottomPanel(Dock,"DialogMaker");
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.

		RemoveControlFromBottomPanel(Dock);
	}
}
#endif
