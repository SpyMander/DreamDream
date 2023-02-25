using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class LineBlock : HBoxContainer
{
	// Called when the node enters the scene tree for the first time.

	
	private Button DeleteButton;

	private Button CollapseButton;
	private Button AddChoiceButton;
	private Button RemoveChoice;

	private VBoxContainer ChoicesBox;

	private Callable DeleteBlockCallable;
	private Callable ToggleCollapseCallable;
	private Callable AddChoiceCallable;

	private LineEdit LineNameEditor;

	private TextEdit LineTextEdit;

	private const int MaxYSize = 70;
	private const int MinYSize = 30;

	private bool Collapsed = false;

	// public String LineName = "UnDone";


	// public String LineText = "";

	private PackedScene ChoiceLine;

	public override void _Ready()
	{
		DeleteButton = GetNode<Button>("DeleteButton");

		CollapseButton = GetNode<Button>("CollapseButton");

		LineNameEditor = GetNode<LineEdit>("LineNameEditor");

		ChoicesBox = GetNode<VBoxContainer>("ChoicePannel/ChoicesBoxContainer/ChoicesBox");

		AddChoiceButton = GetNode<Button>("ChoicePannel/ChoicesBoxContainer/ChoicesBox/ChoiceEditButtons/AddChoice");

		LineTextEdit = GetNode<TextEdit>("LineTextEdit");

		ChoiceLine = GD.Load<PackedScene>("res://scenes/editor/ChoiceLine.tscn");


		DeleteBlockCallable = new Callable(this, nameof(DeleteBlock) );

		ToggleCollapseCallable = new Callable(this, nameof(ToggleCollapse));


		AddChoiceCallable = new Callable(this, nameof(AddChoice) );

		CollapseButton.Connect("pressed", ToggleCollapseCallable);

		DeleteButton.Connect("pressed", DeleteBlockCallable);

		AddChoiceButton.Connect("pressed", AddChoiceCallable);
	}
	
	private Dictionary<String, Variant> _SaveLineData()
	{

		Dictionary<String, Variant> ReturnValue = new();

		
		String UnLineBrackedText =  LineTextEdit.Text.Replace('\n', ' ');

		ReturnValue.Add( "Text", UnLineBrackedText);

		Dictionary<String, String> ChoiceDict = new();

		foreach (Node Child in ChoicesBox.GetChildren() )
		{
			if (Child.HasMethod("_SaveSingleChoiceDict"))
			{
				Dictionary<String,String> SingleChoiceDict = (Dictionary<String,String> )Child.Call("_SaveSingleChoiceDict" );
				foreach (String key in  SingleChoiceDict.Keys)
				{
					ChoiceDict.Add ( key, SingleChoiceDict[key]  );
				}
			}
		}

		ReturnValue.Add("Choice", ChoiceDict);

		// GD.Print(ReturnValue);

		return ReturnValue;
	}

	public void _SetName(String Name)
	{
		LineNameEditor.Text = Name;
	}

	public void _SetBlockParams(Dictionary<String,Variant> Information)
	{
		if ( Information.ContainsKey("Text") )
		{
			LineTextEdit.Text = (String) Information["Text"];
		}
		if (Information.ContainsKey("Choice"))
		{
			Dictionary<String, String> ChoicesDict = (Dictionary<String,String>)Information["Choice"];
			foreach(String ChoiceText in ChoicesDict.Keys  )
			{
				HBoxContainer ChoiceLineInstance = AddChoice();

				ChoiceLineInstance.Call("_SetChoiceParams", ChoiceText, ChoicesDict[ChoiceText] );
			}
		}
	}

	private String _GetName()
	{
		return LineNameEditor.Text;
	}

	private HBoxContainer AddChoice()
	{
		HBoxContainer ChoiceLineInstance = ChoiceLine.Instantiate<HBoxContainer>();
		ChoicesBox.AddChild(  ChoiceLineInstance );

		return ChoiceLineInstance;
	}

	private void DeleteBlock()
	{
		QueueFree();
	}

	private void ToggleCollapse()
	{
		if (Collapsed)
			ForceExpand();
		else
			ForceCollapse();
	}

	private void ForceCollapse()
	{
		Collapsed = true;
		this.CustomMinimumSize = new Vector2(0,MinYSize);
	}
	private void ForceExpand()
	{
		Collapsed = false;
		this.CustomMinimumSize = new Vector2(0,MaxYSize);
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
}
