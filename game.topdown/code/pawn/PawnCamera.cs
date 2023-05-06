using Sandbox;
using System;
using System.Linq;

namespace MyGame;

public partial class PawnCamera : EntityComponent<Pawn>, ISingletonComponent
{
	protected float WheelSpeed => 30f;
	protected Vector2 CameraDistance => new( 125, 1000 );
	protected Vector2 PitchClamp => new( 30, 60 );

	float OrbitDistance = 400f;
	float TargetOrbitDistance = 400f;
	Angles OrbitAngles = Angles.Zero;

	protected static Vector3 IntersectPlane( Vector3 pos, Vector3 dir, float z )
	{
		float a = (z - pos.z) / dir.z;
		return new( dir.x * a + pos.x, dir.y * a + pos.y, z );
	}

	protected static Rotation LookAt( Vector3 targetPosition, Vector3 position )
	{
		var targetDelta = (targetPosition - position);
		var direction = targetDelta.Normal;

		return Rotation.From( new Angles(
			((float)Math.Asin( direction.z )).RadianToDegree() * -1.0f,
			((float)Math.Atan2( direction.y, direction.x )).RadianToDegree(),
			0.0f ) );
	}

	public void Update()
	{
		var pawn = Entity;
		if ( !pawn.IsValid() )
			return;

		Camera.Position = pawn.Position;
		Vector3 targetPos;

		Camera.Position += Vector3.Up * (pawn.CollisionBounds.Center.z * pawn.Scale);
		Camera.Rotation = Rotation.From( OrbitAngles );

		targetPos = Camera.Position + Camera.Rotation.Backward * OrbitDistance;

		Camera.Position = targetPos;
		Camera.FieldOfView = 70f;
		Camera.FirstPersonViewer = null;

		Sound.Listener = new()
		{
			Position = pawn.AimRay.Position,
			Rotation = pawn.EyeRotation
		};
	}

	public void BuildInput()
	{
		var wheel = Input.MouseWheel;
		if ( wheel != 0 )
		{
			TargetOrbitDistance -= wheel * WheelSpeed;
			TargetOrbitDistance = TargetOrbitDistance.Clamp( CameraDistance.x, CameraDistance.y );
		}

		OrbitDistance = OrbitDistance.LerpTo( TargetOrbitDistance, Time.Delta * 10f );

		if ( Input.UsingController || Input.Down( "attack2" ) )
		{
			OrbitAngles.yaw += Input.AnalogLook.yaw;
			OrbitAngles.pitch += Input.AnalogLook.pitch;
			OrbitAngles = OrbitAngles.Normal;

			Entity.ViewAngles = OrbitAngles.WithPitch( 0f );
		}
		else
		{
			var direction = Screen.GetDirection( Mouse.Position, Camera.FieldOfView, Camera.Rotation, Screen.Size );
			var hitPos = IntersectPlane( Camera.Position, direction, Entity.EyePosition.z );

			Entity.ViewAngles = (hitPos - Entity.EyePosition).EulerAngles;
		}

		OrbitAngles.pitch = OrbitAngles.pitch.Clamp( PitchClamp.x, PitchClamp.y );

		Entity.InputDirection = Input.AnalogMove;
	}
}
