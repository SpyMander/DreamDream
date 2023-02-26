using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class DialougeDataTab : TabBar
{
	// Called when the node enters the scene tree for the first time.

	private VBoxContainer ListContainer;

	private Callable AddButtonCallable;

	private Button PlusButton;

	[Export]
	private int MaxChildren = 300;

	[Export]
	private int TypeOfTab = 0;

	private PackedScene ChoiceBox;

	private PackedScene DialogueDataBox;

	//ok here we go
	// 0 = regular
	//so string - string dicts

	// 1 = number
	//so string - int/float

	// 2 = Dialogue datamode
	// stuff, check the scene to see

	// 3 = 

	public override void _Ready()
	{
		

		ListContainer = GetNode<VBoxContainer>("ThingyBoxContainer/ListBox");

		PlusButton = GetNode<Button>("ThingyBoxContainer/ListBox/AddChoice");

		AddButtonCallable = new Callable(this, nameof(_AddLine) ); // DONT FORGET TO CHANGEd

		PlusButton.Connect("pressed", AddButtonCallable);
		// GD.Print(_FetchData());


	}

	public Dictionary<String, Dictionary<String, Variant>> _FetchData()
	{
		Dictionary<String, Dictionary<String, Variant> > metaDict = new();

		Dictionary<String, Variant> values = new Dictionary<string, Variant>();

		foreach (Control slot in ListContainer.GetChildren())
		{
			if (slot.HasMethod("_SaveSingleChoiceDict") && TypeOfTab == 0)
			{ 
				Dictionary<String, String> slotValue = (Dictionary<String,String>)slot.Call("_SaveSingleChoiceDict");
				foreach (String slotKey in slotValue.Keys) // this gets the first k/v pair 
				{
					values.Add(slotKey, slotValue[slotKey]);
					break;
				}
			}
			if (slot.HasMethod("_FetchDataNumMode") && TypeOfTab == 1)
			{
				//stuff, i got tired/
			}
		}
		//name makes it easy
		metaDict.Add(this.Name, values);
		return metaDict;

	}


	private void _AddLine()
	{
		if (TypeOfTab == 0)
		{
			HBoxContainer choiceBoxInstance =  ChoiceBox.Instantiate<HBoxContainer>();
			AddChild(choiceBoxInstance);
		}
		if (TypeOfTab == 1)
		{
			HBoxContainer choiceBoxInstance =  ChoiceBox.Instantiate<HBoxContainer>();
			AddChild(choiceBoxInstance);

			choiceBoxInstance.Set("NumbeMode", true);
		}
		if (TypeOfTab == 2)
		{
			HBoxContainer dialogueDataBoxInstance = DialogueDataBox.Instantiate<HBoxContainer>();
			AddChild(dialogueDataBoxInstance);
		}
	}

	private void PrintData()
	{
		GD.Print(_FetchData());
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
