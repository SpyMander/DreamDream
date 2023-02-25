using Godot;
using Godot.Collections;
using System;

// [Tool]
public partial class Npc : CharacterBody3D
{

	[Export]
	Color NpcColor = new Color();
	// [Export] is broken
	Array<MeshInstance3D> RobotMeshes = new Array<MeshInstance3D>();

	[Export]
	String JsonDialogFile;
	[Export]
	public String DialogStartLine = "Start";
	Dictionary<String,Dictionary<String,Variant > > DialogText;

	// int CurrentDialogLine = 1;//starts from 1

	private bool InDialog = false;

	private Node3D InDialogWith = null;

	Dictionary<String,Variant> CurrentDialogLineData = new();

	StandardMaterial3D RobotMaterial = new();

	float Gravity = 24f;

	float Speed = 7f;

	float PushForce = 1f;
	float NavFreq = 12f;
	float PathGenerateFreq = 1f;
	bool IsFindPathLoopRunning = false;
	bool IsJumping = false;
	
	Vector3 NpcVelocity = new();

	Vector3 TargetLocation = new();

	Node3D TargetNode;

	NavigationAgent3D Navigation;

	AnimationTree RobotAnimationTree;

	private Label3D DialogLabel;

	private Node DialogTriggerer;

	private DialogHandler DialogObserver;

	enum NpcState{
		Idle,
		Targeting,
	}

	NpcState CurrentState;

	// [Signal]
	// delegate void NPCInteractedEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CurrentState = NpcState.Targeting;
		// RobotMaterial = GD.Load<StandardMaterial3D>("res://resources/Shaders/materials/RobotMetal.material");
		RobotAnimationTree = GetNode<AnimationTree>("Robot/AnimationTree");

		Navigation = GetNode<NavigationAgent3D>("NpcNavigation");

		DialogLabel = GetNode<Label3D>("DialogLabel");

		DialogTriggerer = GetNode<Node>("DialogTriggerer");

		DialogObserver = GetNode<DialogHandler>("/root/DialogHandler");



		RobotMaterial.AlbedoColor = NpcColor;

		DialogLabel.Modulate = NpcColor;

		SetNpcMaterial(RobotMaterial);

		TargetNode = GetTree().GetFirstNodeInGroup("Player") as CharacterBody3D;

		DialogText = _GetDialog(JsonDialogFile);

		

		SetCurrentDialogueLine( DialogStartLine );
	}

	public void SetNpcMaterial( StandardMaterial3D Mat) //sets the robots material
	{
		foreach(Node Child in GetNode("Robot/Robot/RobotBones/Skeleton3D").GetChildren(false) )
		{
			if (Child.GetChild<MeshInstance3D>(0) is MeshInstance3D Mesh)
			{
				Mesh.MaterialOverlay = Mat;
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		switch (CurrentState) {
			case NpcState.Idle:
				break;
			case NpcState.Targeting:

				PathLoop(TargetNode);

				// MoveToNavigationPoint();

				AnimationController();

				RigidBodyInteraction(delta);

				
				// SetPathToNode(TargetNode);
				break;
		}
	}

	private async void PathLoop(Node3D Target)
	{
		if (!IsFindPathLoopRunning)
		{
			IsFindPathLoopRunning = true;
			while(CurrentState == NpcState.Targeting)
			{
				
				// if (Navigation.IsNavigationFinished()) make this an option? bad thing though
				SetPathToNode(Target);
				// GD.Print("find path");
				LookAtFromPosition(GlobalPosition,Navigation.GetFinalPosition() * 1.1f, Vector3.Up);
				
				await ToSignal(GetTree().CreateTimer(PathGenerateFreq), "timeout");
				
				// Jump();

			}
			IsFindPathLoopRunning = false; //when its done turn it off
		}
		
	}
	public void SetPathToNode(Node3D Target)
	{
		Navigation.TargetPosition = (Target).GlobalTransform.Origin;
	}

	// public void MoveToNavigationPoint()
	// {
	// 	// if (Navigation.IsTargetReached())
	// 	Vector3 Point;
		
	// 	// Point = Navigation.GetNavPath()[ Navigation.GetNavPathIndex() ];
	// 	Point = Navigation.GetNextPathPosition();
		
	// 	// LookAtFromPosition(GlobalPosition,Point, Vector3.Up);
	// 	// Vector3 RobotRotation = this.Rotation.AngleTo( (Point - Transform.origin) );



	// 	Rotation =  new Vector3(0,Rotation.Y,0);
		
	// 	Vector3 Direction = new();

	// 	float HeightDiffrence = (TargetNode.Transform.Origin.Y - this.Transform.Origin.Y);

	// 	if (HeightDiffrence < 1 & HeightDiffrence > -1)
	// 	{
	// 		Direction = (Point - Transform.Origin);
	// 		Direction.Y = 0; // it will be jittery if this isnt done
	// 	}
	// 	else
	// 	{
	// 		Direction = (TargetNode.Transform.Origin - Transform.Origin);
	// 		Direction.Y = 0;
	// 		// if(IsOnFloor())
	// 		// 	Jump();//jumps towards him
	// 	}

	// 	Direction = (Point - Transform.Origin);
	// 	Direction.Y = 0; // it will be jittery if this isnt done

	// 	Direction = Direction.Normalized();
		
	// 	// GD.Print(Navigation.Get);

	// 	NpcVelocity = Direction * Speed; //plus equals because we might have edited the velocety somewhere else. so we add it

				

	// 	if (!IsOnFloor() )
	// 		NpcVelocity.Y -= Gravity;


	// 	if (IsOnFloor() ){
			
	// 		if(IsJumping)
	// 		{
	// 			NpcVelocity.Y += 300;
	// 			IsJumping = false;
	// 			GD.Print("Jimp");
	// 		}
	// 	}
	// 	Velocity = NpcVelocity;

		

	// 	MoveAndSlide();


	// 	// just looks where its going

	// }

	public void RigidBodyInteraction( double delta )
	{

		if (GetLastSlideCollision() != null){
			RigidBody3D CollidedObject = new();
			
			if (GetLastSlideCollision().GetCollider() is RigidBody3D){
				// 
				Vector3 ProperVelocity = NpcVelocity;
				// if (ProperVelocity == )
				
				CollidedObject = GetLastSlideCollision().GetCollider() as RigidBody3D;
				
				Vector3 _PushAmount = (CollidedObject.GlobalTransform.Origin - GlobalTransform.Origin).Normalized() * ProperVelocity*PushForce;
				
				CollidedObject.ApplyForce(_PushAmount , GlobalTransform.Origin);
			}
		}
	}
	public void AnimationController()
	{
		Vector3 BlendPosition3 = Velocity.Normalized() * Transform.Basis;
		Vector2 BlendPosition = new Vector2(BlendPosition3.X,-BlendPosition3.Z);
		// RobotAnimationTree.Set("parameters/blend_position:x", Velocity.Normalized().x);
		
		RobotAnimationTree.Set("parameters/BlendSpace2D/blend_position", BlendPosition );
		RobotAnimationTree.Set("parameters/TimeScale/scale", (Speed /12) );
		// that blend position value that we got out of the animation tree is a vector2

		// GD.Print( RobotAnimationTree.Set("parameters/blend_position", )  );
	}

	public void Jump()
	{
		IsJumping = true;
	}

	public void _TalkTo(Node3D User)
	{
		
		// EmitSignal( nameof(NPCInteracted) );
		// GD.Print("Hi");
		if (InDialog)
			_EndDialog();
		else{
			User.Call("_StartDialogModeWith", this);
			InDialogWith = User;
			InDialog = true;
			
			SayLine(CurrentDialogLineData);
		}

	}

	private async void  SayLine(Dictionary<String, Variant> DialogLine)
	{
		GD.PrintErr("lolzasdasdsaz3t, " + DialogText);
		// contains logic of the dialouge
		// also gives 

		DialogLabel.Text = (String)DialogLine["Text"];
		

		if (CurrentDialogLineData.ContainsKey("Trigger"))
		{
			DialogTriggerer.Call("_TriggerSpecific", (String)CurrentDialogLineData["Trigger"] );
		}
		if (CurrentDialogLineData.ContainsKey("AddDialougeData"))
		{
			foreach(Godot.Collections.Array data in (Godot.Collections.Array)CurrentDialogLineData["AddDialougeData"])
			{
				// data[0] is name
				// data[1] is value
				// data[2] is write mode
				//write mode 0 is overide
				//write mode 1  is add
				//else simple add to dict, this means there could be two of the same key. ikr

				InDialogWith.Call("_AddDialougeData", data[0], data[1], data[2]);
			}
		}


		if (CurrentDialogLineData.ContainsKey("Choice"))
		{
			if (GetCurrentChoices().ContainsKey("")) //when its empty it means no choice, only a little delay
			{
				// just a simple delay
				await ToSignal(GetTree().CreateTimer( ( (String)DialogLine["Text"]  ).Length * 0.05 + 1), "timeout");
				_PickChoice(""); //auto do it
			}
			else
			{
				Array<String> Choices = (Array<String>)GetCurrentChoices().Keys;
				foreach (string choice in Choices)
				{
					//remove ones that the player does not have the requirments of.
					Dictionary<String, Variant> targetLine = getToCommandLineData(GetCurrentChoices()[choice]);
					
					Dictionary<String, int> playerReq = (Dictionary<String,int>)InDialogWith.Get("DialogueData");
					Dictionary<String,int> requirements = (Dictionary<String,int>)targetLine["Requirements"];
					GD.Print(requirements);
					GD.Print( targetLine );
					foreach (string requirement in requirements.Keys)
					{
						if (playerReq.ContainsKey(requirement) )
						{
							if (playerReq[requirement] != requirements[requirement])
							{
								Choices.Remove(choice);
							}
						}
						else
							Choices.Remove(choice);
					}
					
				}
				InDialogWith.Call("_GiveDialogOptions", this, Choices );
				// else if there are choices, give them to the player.
			}
		}

	}


	private void SetCurrentDialogueLine(String DialogLine)
	{
		if(DialogText.ContainsKey(DialogLine))
		{
			CurrentDialogLineData = DialogText[DialogLine];

		}
	}

	private Dictionary<String,Variant > GetNextDialogLine(String Request)
	{
		
		return ( Dictionary<String,Variant >)DialogText[ Request ];
	}

	private void ExecuteDialogCommand(String Command)
	{
		if (Command.StartsWith("#TO "))
		{
			Command = Command.Split()[1]; //real shot in the dark
			SetCurrentDialogueLine( Command );
			SayLine(CurrentDialogLineData);
		}
		else if (Command.StartsWith("#END"))
		{
			_EndDialog();
		}
		else
		{
			GD.PrintErr("File, ", JsonDialogFile, " has incorrect Command, ", Command, " Command does not exist");
		}
	}

	private Dictionary<String,Variant> getToCommandLineData(String command)
	{
		//returns name the line.
		if (command.StartsWith("#TO ")){
			command = command.Split()[1];
			return DialogText[command];
		}
		else
			return null;
	}

	public void _EndDialog()
	{
		DialogLabel.Text = "";
		SetCurrentDialogueLine(DialogStartLine); //resets so the next time it starts over
		InDialogWith.Call("_EndDialogModeWith", this);
		InDialog = false;
	}

	public void _PickChoice(String PlayerChoice)
	{
		Dictionary<String,String> ChoicesDict = GetCurrentChoices();
		if(ChoicesDict.ContainsKey( PlayerChoice ))
		{
			// GD.Print( ChoicesDict[PlayerChoice] );
			ExecuteDialogCommand( ChoicesDict[PlayerChoice] );

		}
		else
		{
			GD.PrintErr("BOY YOU TRIPPIN, answer not found to: ", PlayerChoice);
		}

		

	}

	private Dictionary<String,String> GetCurrentChoices()
	{
		return (Dictionary<String,String>)CurrentDialogLineData["Choice"];
	}

	public Dictionary<String,Dictionary<String,Variant > > _GetDialog( String JsonFilePath )
	{

		return ( Dictionary<String,Dictionary<String,Variant > >) DialogObserver.Call("GetDialog", JsonFilePath);
	}
}
