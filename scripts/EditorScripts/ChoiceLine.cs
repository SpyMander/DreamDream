using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class ChoiceLine : HBoxContainer
{
	// Called when the node enters the scene tree for the first time.

	Button DeleteButton;

	LineEdit ChoiceTextLine;
	LineEdit CommandLine;

	Callable DeleteLineCallable;
	public override void _Ready()
	{
		DeleteButton = GetNode<Button>("DeleteButton");

		ChoiceTextLine = GetNode<LineEdit>("ChoiceTextLine");

		CommandLine = GetNode<LineEdit>("CommandLine");

		DeleteLineCallable = new Callable(this, nameof(DeleteLine) );
	
		DeleteButton.Connect("pressed", DeleteLineCallable);
	}

	private void DeleteLine()
	{
		QueueFree();
	}

	public Dictionary<String,String> _SaveSingleChoiceDict()
	{
		Dictionary<string, string> RtDict = new Dictionary<string, string>();
		RtDict.Add(ChoiceTextLine.Text, CommandLine.Text);
		
		return RtDict;
	}

	public void _SetChoiceParams(String Text, String Command)
	{
		ChoiceTextLine.Text = Text;
		CommandLine.Text = Command;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
}
