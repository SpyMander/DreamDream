using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class DialogPannel : ScrollContainer
{
	GraphEdit BlockGraphEditor;
	PackedScene LineBlock;

	Button AddLineBlockButton;

	Button SaveButton;
	Button LoadButton;

	LineEdit TargetFilePathLineEdit;

	Callable SaveCallable;
	Callable AddLineCallable;
	Callable LoadCallable;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		TargetFilePathLineEdit = GetNode<LineEdit>("VBoxContainer2/HBoxContainer/TargetFileLineEdit");
		BlockGraphEditor = GetNode<GraphEdit>("VBoxContainer2/GraphEdit");
		LineBlock = GD.Load<PackedScene>("res://scenes/editor/LineBlock.tscn");

		AddLineCallable = new Callable(this, nameof( AddLineBlock ));

		SaveCallable = new Callable(this, nameof(_SaveButtonPressed) );

		LoadCallable = new Callable(this, nameof(_LoadTargetFile) );

		AddLineBlockButton = GetNode<Button>("VBoxContainer2/HBoxContainer/AddLineButton");

		SaveButton = GetNode<Button>("VBoxContainer2/HBoxContainer/SaveButton");

		LoadButton = GetNode<Button>("VBoxContainer2/HBoxContainer/LoadButton");
		
		//C0nn3ct10ns connections

		AddLineBlockButton.Connect("pressed", AddLineCallable);

		SaveButton.Connect("pressed", SaveCallable);

		LoadButton.Connect("pressed", LoadCallable);

		// _GetJsonFormat();
	}

	private String ExtractJsonFormat()
	{
		Dictionary<String,Dictionary<String,Variant> > Data = new();
		foreach(Node Block in BlockGraphEditor.GetChildren() )
		{
			Data.Add( (String)Block.Call("_GetName"), (Dictionary<String,Variant>)Block.Call("_SaveLineData") );
		}

		// Json Parser = new();
		
		
		// GD.PrintErr("Real Stuff ", '\n' , JSON.Stringify(Data,"\t") );

		String OutputString = Json.Stringify(Data,"\t"); //makes it look better

		GD.PrintErr(OutputString);

		return OutputString;

	}

	private void SaveToStringFile(String FilePath, String Data)
	{
		FileAccess files = FileAccess.Open(FilePath, FileAccess.ModeFlags.Write);

		GD.Print("Data ", Data);

		files.StoreString(Data);

		files.Flush();

		// files.Free();
	}

	private void _SaveButtonPressed()
	{
		SaveToStringFile(TargetFilePathLineEdit.Text , ExtractJsonFormat() );
	}

	public void _LoadTargetFile()
	{
		FileAccess JsonFile = FileAccess.Open(TargetFilePathLineEdit.Text, FileAccess.ModeFlags.Read);

		Json JsonParser = new();

		Error RawParseResult = JsonParser.Parse( JsonFile.GetAsText() );

		Dictionary <String, Dictionary <String, Variant > > Joe = (Dictionary <String, Dictionary <String, Variant > >)JsonParser.Data;

		foreach(String BlockName in Joe.Keys)
		{
			GraphNode TargetBlock = AddLineBlock();

			TargetBlock.Call("_SetName",BlockName );

			TargetBlock.Call("_SetBlockParams", (Dictionary<String,Variant>)Joe[BlockName]);
		}


	}

	private GraphNode AddLineBlock()
	{
		GraphNode LineBlockInstance = LineBlock.Instantiate<GraphNode>();
		BlockGraphEditor.AddChild( LineBlockInstance );

		return LineBlockInstance;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
}
