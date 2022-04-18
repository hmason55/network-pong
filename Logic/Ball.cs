using Godot;

public class Ball : Area2D
{
	private const int DefaultSpeed = 150;

	public Vector2 direction = Vector2.Left;

	public Vector2 TargetPosition = Vector2.Zero;

	private Vector2 _initialPos;
	public float Speed = DefaultSpeed;

	public override void _Ready()
	{
		_initialPos = Position;
		TargetPosition = Position;
	}

	public override void _PhysicsProcess(float delta)
	{
		if (!NetworkManager.IsServer())
		{
			return;
		}

		if (Menu.IsPaused)
		{
			return;
		}

		// Server-only movement
		Vector2 position = Position + (Speed * delta * direction);

		if(position != Position)
        {
			Position = position;
			ServerSend.UpdateBallPosition(Position, Client.Instance.ID, true);
		}
	}

	public void Reset()
	{
		direction = Vector2.Left;
		Position = _initialPos;
		TargetPosition = _initialPos;
		Speed = DefaultSpeed;

		if (NetworkManager.IsServer())
		{
			ServerSend.UpdateBallPosition(Position, Client.Instance.ID, false);
		}
	}
}
