using Godot;
using Godot.Collections;
using System;

public partial class Player : CharacterBody3D
{
	public float Speed = 8.5f;

	public const float WalkSpeed = 8.5f;
	public const float RunSpeed = 25f;
	public float AnimMultiplier = 1f;
	[Export]
	public float JumpVelocity = 7.5f;
	[Export]
	public float PushForce = 15f;
	[Export]
	public float Gravity = 24f;

	public bool ControllerMode = false;

	public float MouseSensitivity = 0.1f;

	public float Friction = 9f;
	

	public float ControllerSensitivity = 200f;

	private Node3D Head;

	private AnimationTree RobotAnimationTree;

	private RayCast3D InteratcionRay;
	private RayCast3D InteratcionCheckRay;
	private VBoxContainer ChoiceBox;
	

	private bool IsSprinting = false;
	private bool IsImobile = false;

	private bool IsHeadLocked = false;
	private bool IsInDialog = false;
	private Node3D InDialogWith = null;
	private Array<Button> ChoiceButtonList = new();
	private PackedScene DialogButton;
	Vector3 PlayerVelocity = new();

	public Dictionary<String,int> DialogueData = new();
	// data that npc's, dialouge files analize.
	// so if you did somthing bad or good, the npc would know.
	// ofc this needs to be written down somewhere, so you know 
	// what is what.

	public override void _Ready()
	{
		Head = GetNode<Node3D>("Head");
		RobotAnimationTree = GetNode<AnimationTree>("Robot/AnimationTree");
		InteratcionRay = GetNode<RayCast3D>("Head/Camera3D/InteractRay");
		InteratcionCheckRay = GetNode<RayCast3D>("Head/Camera3D/InteractCheckRay");

		DialogButton = GD.Load<PackedScene>("res://scripts/objects/ui/DialogButtonObject.tscn");

		InteratcionRay.AddException(this);
		InteratcionCheckRay.AddException(this);


		Input.MouseMode = Input.MouseModeEnum.Captured; //catches the mouse

		ChoiceBox = GetNode<VBoxContainer>("UI/Control/ChoiceBox");
		ChangeSpeedTo(WalkSpeed);
	}

	public override void _PhysicsProcess(double delta)
	{

		float CurrentFriction = Friction;
		if (Input.IsActionJustPressed("ui_cancel"))
			Input.MouseMode = Input.MouseModeEnum.Visible;

		// Vector3 PlayerVelocity = new();

		
		// Add the gravity.

		PlayerVelocity = AddGravity (Gravity, PlayerVelocity, delta);

		if (Input.IsActionJustPressed("dr_jump") && IsOnFloor())
			PlayerVelocity.Y = JumpVelocity;

		
		// Handle Jump.


		if (IsOnFloor() )
			AnimMultiplier = 1f;
		else
		{
			CurrentFriction = 2f;
			AnimMultiplier = 0.1f;
		}
		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector3 direction = GetMovementDirection();

		if (direction != Vector3.Zero)
		{
			PlayerVelocity.X = direction.X * Speed;
			PlayerVelocity.Z = direction.Z * Speed;
		}
		else
		{
			PlayerVelocity.X = Mathf.MoveToward(PlayerVelocity.X, 0, Speed);
			PlayerVelocity.Z = Mathf.MoveToward(PlayerVelocity.Z, 0, Speed);
		}

		Vector3 SmoothedVelocity = new();

		SmoothedVelocity.X = Velocity.Lerp(PlayerVelocity, CurrentFriction * (float)delta).X;
		SmoothedVelocity.Z = Velocity.Lerp(PlayerVelocity, CurrentFriction * (float)delta).Z;

		SmoothedVelocity.Y = PlayerVelocity.Y;
		
		
		if (!IsImobile)
			


			Velocity = SmoothedVelocity;

		MoveAndSlide();

		RigidBodyInteraction( delta );
		
	}
	
	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent is InputEventJoypadButton || inputEvent is InputEventJoypadMotion)
			if(!ControllerMode)
				SetToControllerMode();
		if (inputEvent is InputEventKey & ControllerMode)
			SetToKeyboardMode();
		//lookingAround
		// if (inputEvent is InputEventJoypadMotion)
		// { you will find this in the process function, it does not work here
		// 	Vector2 LookDirection = Input.GetVector("dr_joycon_2_left", "dr_joycon_2_right", "dr_joycon_2_up", "dr_joycon_2_down");
		// 	HeadMovement(LookDirection.x, LookDirection.y);
		// }
		if (Input.IsActionJustPressed("dr_sprint"))
		{
			if( inputEvent is InputEventKey)
				ChangeSpeedTo(RunSpeed);
			if (inputEvent is InputEventJoypadButton)
			{
				if (Speed == RunSpeed)
					ChangeSpeedTo(WalkSpeed);
				else
					ChangeSpeedTo(RunSpeed);
			}

		}
		if (Input.IsActionJustReleased ("dr_sprint"))
		{
			if( inputEvent is InputEventKey)
				ChangeSpeedTo(WalkSpeed);
		}


		if (inputEvent is InputEventMouseMotion MouseEvent)
		{
			PlayerAndHeadRotataion(MouseEvent.Relative.X * MouseSensitivity, MouseEvent.Relative.Y * MouseSensitivity);
		}
	}

	public override void _Process(double delta)
	{
		//its here because, reasons

		PersistentInputs(delta);

		AnimationController();

		_DialogButtonPressDetector();
	}

	public void RigidBodyInteraction( double delta )
	{

		if (GetLastSlideCollision() != null){
			RigidBody3D CollidedObject = new();
			
			if (GetLastSlideCollision().GetCollider() is RigidBody3D){
				// 
				Vector3 ProperVelocity = PlayerVelocity;
				// if (ProperVelocity == )
				
				CollidedObject = GetLastSlideCollision().GetCollider() as RigidBody3D;
				
				Vector3 _PushAmount = (CollidedObject.GlobalTransform.Origin - GlobalTransform.Origin).Normalized() * ProperVelocity*PushForce;

				CollidedObject.ApplyForce(_PushAmount , GlobalTransform.Origin);
			}
		}
	}

	private void PlayerAndHeadRotataion(float ReletiveX, float ReletiveY)
	{// looking around, thats what this is fors

		if (!IsHeadLocked)
		{
			RotateY( -Mathf.DegToRad(ReletiveX) );
			Head.RotateX( -Mathf.DegToRad(ReletiveY));

			Vector3 HeadCapTransfrom = new();

			HeadCapTransfrom.X = Mathf.RadToDeg(Head.Rotation.X);
			HeadCapTransfrom.X = Mathf.Clamp(HeadCapTransfrom.X, -90, 90);
			HeadCapTransfrom.X = Mathf.DegToRad(HeadCapTransfrom.X);

			Head.Rotation = HeadCapTransfrom;
		}
	}
	private Vector3 GetMovementDirection()
	{
		Vector2 InputDirection = new Vector2();
		InputDirection =  Input.GetVector("dr_left", "dr_right", "dr_forward", "dr_backward");
		if (!ControllerMode)
		{
			return (Transform.Basis * new Vector3(InputDirection.X, 0, InputDirection.Y)).Normalized();
		}
		else
		{
			return (Transform.Basis * new Vector3(InputDirection.X, 0, InputDirection.Y) );
		} //transform.basis is to put the direction in the right rotation
		
	}
	private Vector3 AddGravity( float GravityAmount, Vector3 PlayerVelocity, double delta)
	{
		if (!IsOnFloor())
			PlayerVelocity.Y -= GravityAmount * (float) delta;
		else
			PlayerVelocity.Y = 0;
		return PlayerVelocity;
	}

	public void SetToControllerMode()
	{
		GD.Print("set to controller");
		ControllerMode = true;
	}
	public void SetToKeyboardMode()
	{
		GD.Print("set to keyboard");
		ControllerMode = false;
	}

	public void PersistentInputs(double Delta)
	{// inputs that need constant updating. things that dont work in _input()
		Vector2 LookDirection = Input.GetVector("dr_joycon_2_left", "dr_joycon_2_right", "dr_joycon_2_up", "dr_joycon_2_down");
		PlayerAndHeadRotataion(LookDirection.X * (float)Delta * ControllerSensitivity , LookDirection.Y * (float)Delta * ControllerSensitivity);

		if (Input.IsActionJustPressed("dr_interact"))
		{
			if (InteratcionRay.IsColliding() )
			{
				// GD.Print(  InteratcionCheckRay.GetCollisionPoint() - InteratcionRay.GetCollisionPoint()  );
				// if (InteratcionRay.GetCollider().IsClass("MeshInstance") ||  )
				Interact( InteratcionRay.GetCollider() as Node);
				// if (!InteratcionCheckRay.IsColliding())
				// 	Interact( InteratcionRay.GetCollider() as Node);
				// else if( ( InteratcionCheckRay.GetCollisionPoint() - InteratcionRay.GetCollisionPoint() ) >  InteratcionCheckRay.GetCollisionPoint() )
				// 	Interact( InteratcionRay.GetCollider() as Node);

			}
		}
		
		
	}

	public void AnimationController()
	{
		Vector3 BlendPosition3 = GetMovementDirection() * Transform.Basis;
		Vector2 BlendPosition = new Vector2(BlendPosition3.X,-BlendPosition3.Z);
		// RobotAnimationTree.Set("parameters/blend_position:x", Velocity.Normalized().x);
		
		RobotAnimationTree.Set("parameters/BlendSpace2D/blend_position", BlendPosition );
		RobotAnimationTree.Set("parameters/TimeScale/scale", (Speed /8.5)*AnimMultiplier);
		// that blend position value that we got out of the animation tree is a vector2

		// GD.Print( RobotAnimationTree.Set("parameters/blend_position", )  );
	}

	public void ChangeSpeedTo(float NewSpeed)
	{
		Speed = NewSpeed;
	}

	public void _StartDialogModeWith(Node3D NPC)
	{
		IsInDialog = true;
		IsImobile = true;
		IsHeadLocked = true;
		InDialogWith = NPC;
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	public void _EndDialogModeWith(Node3D NPC)
	{
		GD.Print("ended");
		IsInDialog = false;
		IsImobile = false;
		IsHeadLocked = false;
		InDialogWith = null;
		Input.MouseMode = Input.MouseModeEnum.Captured;

		_DeleteDialogOptions();
	}

	public void _GiveDialogOptions(Node3D NPC, Array<String> Choices)
	{
		GD.Print("what does the choices look like, ",Choices );
		if (IsInDialog)
		{	
			foreach( String ChoiceText in Choices )
			{
				AddChoiceButtonToUi(ChoiceText);
				
			}
		}
		
	}

	private void AddChoiceButtonToUi(String ChoiceString)
	{
		Button DialogButtonInstance = DialogButton.Instantiate<Button>();
		// 
		// ChoiceButton.SetScript(DialogButtonScript);

		ChoiceBox.AddChild(DialogButtonInstance);

		DialogButtonInstance.Set("Player", this);

		DialogButtonInstance.Text = ChoiceString;
		

		ChoiceButtonList.Add( DialogButtonInstance );
		
	}

	private void _DialogChoicePicked(String ChoiceString)
	{

		_DeleteDialogOptions();
		InDialogWith.Call("_PickChoice",ChoiceString );
		GD.Print("Picked, ", ChoiceString);
		
	}

	private void _DeleteDialogOptions()
	{
		GD.Print("CLEARED BUTTONS");
		ChoiceButtonList.Clear();
		foreach(Button child in ChoiceBox.GetChildren())
		{
			child.QueueFree();
		}
	}

	private void _DialogButtonPressDetector()
	{
		// if (IsInDialog)
		// {
			
		// 	foreach(Button ChoiceButton in ChoiceButtonList)
		// 	{
				
		// 		if(ChoiceButton.Disabled)
		// 		{
		// 			GD.Print("yep pressed");
		// 			_DialogChoicePicked( ChoiceButton.Text );
		// 			_DeleteDialogOptions();
		// 		}
		// 	}
		// }
	}

	private void _AddDialougeData(String name, int value,  int mode)
	{

		if (DialogueData.ContainsKey(name))
		{
			if (mode == 0)
			{

				// careless override
				// DialougeData.Remove(name);
				// DialougeData.Add(name, value);
				DialogueData[name] = value;
			}
			if (mode ==  1)
			{
				// addition
				// laid out this way to be easy to deal with
				// int storeValue = DialougeData[name];
				DialogueData[name] += value;
			}
		}
		
		else
		{
			DialogueData.Add(name, value);
		}
		
	}

	private void _EndDialog()
	{
		GD.Print("ended dialog");
		IsInDialog = false;
		IsImobile = false;
		InDialogWith = null;
	}



	public void Interact( Node InteractNode )
	{
		if ( InteractNode.HasMethod("_Trigger") )
		{
			
			GD.Print("interacted with ", InteractNode);
			InteractNode.Call("_Trigger", this);
		}
		else
		{
			GD.Print( "Yo interaction with, ",InteractNode, " not possible, from player" );
		}
		
	}
}
