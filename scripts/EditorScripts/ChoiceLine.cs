using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class ChoiceLine : HBoxContainer
{
	// Called when the node enters the scene tree for the first time.

	Button DeleteButton;

	SpinBox NumberCounter;
	LineEdit ChoiceTextLine;
	LineEdit CommandLine;



	[Export]
	private Boolean NumbeMode = false;

	Callable DeleteLineCallable;
	public override void _Ready()
	{
		NumberCounter = GetNode<SpinBox>("SpinBox");

		DeleteButton = GetNode<Button>("DeleteButton");

		ChoiceTextLine = GetNode<LineEdit>("ChoiceTextLine");

		CommandLine = GetNode<LineEdit>("CommandLine");

		DeleteLineCallable = new Callable(this, nameof(DeleteLine) );
	
		DeleteButton.Connect("pressed", DeleteLineCallable);

		if (NumbeMode)
		{
			ChoiceTextLine.Hide();
		}
	}

	private void DeleteLine()
	{
		QueueFree();
	}

	public Dictionary<String,String> _SaveSingleChoiceDict()
	{
		if (NumbeMode)
		{
			GD.PrintErr("Tried to get a string from a num mode choiceline");
			return null;
		}
		Dictionary<string, string> RtDict = new Dictionary<string, string>();
		RtDict.Add(ChoiceTextLine.Text, CommandLine.Text);
		
		return RtDict;
	}

	public Dictionary<String,int> _FetchDataNumMode()
	{
		if (!NumbeMode)
		{
			GD.PrintErr("Tried to fetch a number from a non num mode choiceline");
			return null;
		}
		Dictionary<String, int> ReturnDict = new();

		ReturnDict.Add(ChoiceTextLine.Text, (int)NumberCounter.Value);

		return ReturnDict;
	}

	public void _SetChoiceParams(String Text, String Command)
	{
		ChoiceTextLine.Text = Text;
		CommandLine.Text = Command;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
}
