using Godot;
using Godot.Collections;
using System;

public partial class Interactable : Node
{
	// Called when the node enters the scene tree for the first time.
	// [Signal]
	// public delegate void Triggered();
	[Signal]
	public delegate void TriggerEventHandler();

	[Export]
	public Dictionary<String, Dictionary<NodePath, String> > Connections; /// node then method

	// [Export]
	// public string Method;
	
	[Export]
	public int UsableTimes = 3;

	[Export]
	public bool FiniteUse = false;

	[Export]
	public String InteractText = "Sample Interactable";

	[Export]
	public bool AccountForBots = false;

	[Export]
	public bool AccountForPlayers = true;

	[Export]
	public bool Usable = true;

	[Export]
	public String SignalUseCall = "OnUse";


	enum InteractionMode{
		None,
		InteractableArea,
		InteractableEntranceArea,
	}

	InteractionMode Mode;	

	public override void _Ready()
	{
		Mode = InteractionMode.None;
		if (this.GetClass() is "Area3D" & Usable) //wired but makes sense if you put this on an area it will say area, even though the script is for a generic node
		{
			Mode = InteractionMode.InteractableArea;
		}
	}

	public void _Trigger(Node3D User) //if i need the user ever
	{
		foreach (NodePath TargetNode in Connections[SignalUseCall].Keys )
		{
			GetNode(TargetNode).CallDeferred( Connections[ SignalUseCall ][TargetNode], User );
		}
	}

	public void _TriggerSpecific(Node3D User, String SignalName)
	{
		foreach (NodePath TargetNode in Connections[SignalName].Keys )
		{
			String MethodName = Connections[ SignalName ] [TargetNode];
			GetNode(TargetNode).CallDeferred(MethodName, User );
		}
	}
}
