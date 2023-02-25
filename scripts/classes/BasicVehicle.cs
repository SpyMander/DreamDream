using Godot;
using Godot.Collections;
using System;

public partial class BasicVehicle : VehicleBody3D
{
	// Called when the node enters the scene tree for the first time.



	private bool IsHoodOpen = false;

	private bool IsTrunkOpen = false;

	private bool IsLeftDoorOpen = false;

	private bool IsRightDoorOpen = false;

	private bool IsDriving = false;

	private bool IsWomanInSeat = false; //lol

	private float Power = 2400;

	private float ThrotlleMult = 4f;

	private float SteerDelay = 0.4f;

	private float SteerAngle = 25f;

	private float AnimationSpeed = 0.6f;
	
	private float BrakePower = 40f;

	private float DoorOpenAmount = 60;

	private Node3D Hood;
	private Node3D Trunk;
	private Node3D LeftDoor;
	private Node3D RightDoor;

	private Node3D EntityInVehicle;

	private Node3D SitPosition;

	private RayCast3D LeftDoorRaycast;
	private RayCast3D RightDoorRaycast;	

	public override void _Ready()
	{

		Hood = GetNode<CollisionShape3D>("HoodCollision");

		Trunk = GetNode<CollisionShape3D>("TrunkCollision");

		RightDoor = GetNode<CollisionShape3D>("DoorRightCollision");

		LeftDoor = GetNode<CollisionShape3D>("DoorLeftCollision");

		SitPosition = GetNode<Node3D>("SitPosition");

		LeftDoorRaycast = GetNode<RayCast3D>("DoorLeftRaycast");
		
		RightDoorRaycast = GetNode<RayCast3D>("DoorRightRaycast");

		
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (IsDriving)
		{
			// SitPosition.Rotation = Vector3.Zero;
			 

			float ReltiveYRotation = RotationDegrees.Y - EntityInVehicle.RotationDegrees.Y; 

			Vector3 YRotation = new();

			YRotation.Y = ReltiveYRotation;
			
			EntityInVehicle.GlobalTransform = SitPosition.GlobalTransform;

			// EntityInVehicle.RotationDegrees += YRotation;

			

			// EntityInVehicle.Rotation += EntityRotationAfterRotation;

		}

	}

	public override void _Input(InputEvent ievent)
	{

		if (IsDriving & !IsWomanInSeat)
		{
			if ( Input.IsActionPressed("dr_left") )
			{
				Steering = Mathf.DegToRad( SteerAngle ) ;
			}
			else if ( Input.IsActionPressed("dr_right") )
			{
				Steering = Mathf.DegToRad( -SteerAngle );
			}
			else
			{
				Steering = 0;
			}

			if (Input.IsActionPressed("dr_sprint") && Input.IsActionPressed("dr_forward"))
				EngineForce = Power*ThrotlleMult;
			
			else if (Input.IsActionPressed("dr_forward"))
				EngineForce = Power;

			else if (Input.IsActionPressed("dr_backward"))
				EngineForce = -Power/1.8f; //reverse
			else
				EngineForce = 0;
			
			if (Input.IsActionPressed("dr_jump"))
				Brake = BrakePower;
			else
				Brake = 0;

			if (Input.IsActionJustPressed("dr_interact"))
			{
				_ExitVehicle();
			}
		}
	}

	public void _InteractDoorLeft(Node3D User)
	{
		if (IsLeftDoorOpen)
		{
			Animate(LeftDoor, "rotation:y", Mathf.DegToRad( -180 ) , AnimationSpeed);
			IsLeftDoorOpen = false;
			// close LeftDoor			
		}
		else
		{
			Animate(LeftDoor, "rotation:y", Mathf.DegToRad( -180 - DoorOpenAmount) , AnimationSpeed);
			IsLeftDoorOpen = true;
		}
			
	}
	public void _InteractDoorRight(Node3D User)
	
	{
		if (IsRightDoorOpen)
		{
			Animate(RightDoor, "rotation:y", Mathf.DegToRad(-180) , AnimationSpeed);
			IsRightDoorOpen = false;
			// close RightDoor
		}
		else
		{
			Animate(RightDoor, "rotation:y", Mathf.DegToRad( -180 + DoorOpenAmount ) , AnimationSpeed);
			IsRightDoorOpen = true;

		}
			
	} 

	
	public void _InteractHood(Node3D User)
	{
		

		if (IsHoodOpen)
		{
			
			// close trunk
			Animate(Hood, "rotation:x", Mathf.DegToRad( 0 ) , AnimationSpeed);

			IsHoodOpen = false;
		}
		else
		{
			//open trunk
			
			Animate(Hood, "rotation:x", Mathf.DegToRad( -60 ) , AnimationSpeed);

			IsHoodOpen = true;
		}
	}

	public void _InteractTrunk(Node3D User)
	{
		if (IsTrunkOpen)
		{
			Animate(Trunk, "rotation:x", Mathf.DegToRad( 0 ) , AnimationSpeed);
			IsTrunkOpen = false;
			// close trunk
		}
		else
		{
			Animate(Trunk, "rotation:x", Mathf.DegToRad( 60 ) , AnimationSpeed);
			IsTrunkOpen = true;
			//open trunk

		}
	
	}

	public void _EnterVehicle(Node3D User)
	{
		if (EntityInVehicle == null && IsDriving == false)
		{
			AddCollisionExceptionWith(User);
			EntityInVehicle = User;

			IsDriving = true;

			if (IsLeftDoorOpen)
				_InteractDoorLeft(this);
			if (IsRightDoorOpen)
				_InteractDoorRight(this);
			
			// DriverCamera.Current = true;
		}
	}

	public async void _ExitVehicle()
	{
		Transform3D ExitTransform = EntityInVehicle.GlobalTransform;
		
		Boolean CanExit = false;


		if (!LeftDoorRaycast.IsColliding() )
		{
			ExitTransform.Origin = ToGlobal( LeftDoorRaycast.TargetPosition * 2 );
			if (!IsLeftDoorOpen)
				_InteractDoorLeft(this);
			
			CanExit = true;
		}
		else if ( !RightDoorRaycast.IsColliding() )
		{
			ExitTransform.Origin = ToGlobal( RightDoorRaycast.TargetPosition * 2 );
			if (!IsRightDoorOpen)
				_InteractDoorRight(this);
			
			CanExit = true;
		}

		if (CanExit)
		{
			IsDriving = false;
			
			EntityInVehicle.Transform = ExitTransform;

			Vector3 WantedRotation = EntityInVehicle.Rotation;

			WantedRotation.Z = 0;

			EntityInVehicle.GlobalRotation = WantedRotation;

			await ToSignal( GetTree().CreateTimer(0.2), "timeout" );

			RemoveCollisionExceptionWith(EntityInVehicle);

			EntityInVehicle = null;
		}
	}

	private void Animate( Node3D TargetNode, String Property, float Amount, float Speed )
	{
		Tween AnimationTween;

		AnimationTween = CreateTween();
		AnimationTween.SetParallel(true);
		AnimationTween.SetEase( Tween.EaseType.Out );
		AnimationTween.SetTrans(Tween.TransitionType.Bounce);

		AnimationTween.TweenProperty(TargetNode, Property, Amount , Speed);

	}



	
}
