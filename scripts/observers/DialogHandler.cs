using Godot;
using Godot.Collections;
using System;

public partial class DialogHandler : Node
{
	// Called when the node enters the scene tree for the first time.

	public Dictionary<String, Dictionary<String, Dictionary<String, Variant> > > LoadedDialog = new();
	public override void _Ready()
	{

		foreach(String path in GetDialogPaths() )
		{
			LoadedDialog.Add(path, Parse(path) );
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.

	private Dictionary<String, Dictionary<String, Variant> > Parse(String path)
	{
		FileAccess JsonFile = FileAccess.Open(path, FileAccess.ModeFlags.Read);

		Json JsonParser = new();

		Error RawParseResult = JsonParser.Parse( JsonFile.GetAsText() );

		Dictionary <String, Dictionary <String, Variant > > Joe = (Dictionary <String, Dictionary <String, Variant > >)JsonParser.Data;

		return Joe;
	}

	private Array<String> GetDialogPaths()
	{
		Array<String> FilePaths = new();

		DirAccess DialogDirectory = DirAccess.Open("res://resources/dialog/"); //hard coding it make it easy, so lets keep it that way

		DialogDirectory.ListDirBegin();

		String DialogFilePath = DialogDirectory.GetNext();

		while (DialogFilePath != "")
		{
			FilePaths.Add("res://resources/dialog/" + DialogFilePath);
			DialogFilePath = DialogDirectory.GetNext();
		}

		return FilePaths;
	}

	public Dictionary<String, Dictionary<String, Variant> > GetDialog( String pathRequest )
	{
		if (LoadedDialog.ContainsKey(pathRequest))
		{
			return LoadedDialog[pathRequest];

		}
		
		return null;
	}
}
