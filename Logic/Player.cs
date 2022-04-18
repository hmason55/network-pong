using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class Player : Area2D, IMovementController
{
    public int ClientID { get; set; }
    public string Username { get; private set; }
    private const float MoveSpeed = 17.5f;

    public Vector2 TargetPosition { get; set; }

    public bool Left { get; set; }
    public bool Right { get; set; }
    public bool Up { get; set; }
    public bool Down { get; set; }

    public void SetUsername(string username)
    {
        Username = username;
        GetNode<Label>("Label").Text = username;
    }

    public override void _Ready()
    {
        TargetPosition = Position;
    }

    public override void _Input(InputEvent e)
    {
        if(Client.Instance.ID != ClientID)
        {
            return;
        }

        if (e is InputEventKey)
        {
            ParseKeyPressed(e as InputEventKey);
        }
    }

    public void ParseKeyPressed(InputEventKey e)
    {
        switch ((KeyList)e.Scancode)
        {
            case KeyList.A:
                Left = e.IsPressed();
                break;

            case KeyList.D:
                Right = e.IsPressed();
                break;

            case KeyList.S:
                Down = e.IsPressed();
                break;

            case KeyList.W:
                Up = e.IsPressed();
                break;
        }
    }

    public override void _Process(float delta)
    {
        InterpolatePosition(delta);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Client.Instance.ID != ClientID)
        {
            return;
        }

        float x = 0;
        if (Left)
        {
            x -= 1;
        }

        if (Right)
        {
            x += 1;
        }

        float y = 0;
        if (Up)
        {
            y -= 1;
        }

        if (Down)
        {
            y += 1;
        }

        Vector2 direction = new Vector2(x, y).Normalized();

        if (direction != Vector2.Zero)
        {
            ClientSend.RequestPlayerMovement(direction);
        }
    }

    public void InterpolatePosition(float delta)
    {
        if(Position != TargetPosition)
        {
            Position = Position.LinearInterpolate(TargetPosition, delta * MoveSpeed);
        }
    }

    public void UpdatePosition(Vector2 direction)
    {
        Vector2 position = Position;

        // Move paddle.
        position += direction * MoveSpeed;

        if (ClientID % 2 == 1)
        {
            // Left side
            position.x = Mathf.Clamp(position.x, 32, (GetViewportRect().Size.x / 2) - 32);
        }
        else
        {
            // right side
            position.x = Mathf.Clamp(position.x, (GetViewportRect().Size.x / 2) + 32, GetViewportRect().Size.x - 32);
        }

        position.y = Mathf.Clamp(position.y, 16, GetViewportRect().Size.y - 16);

        if (position != Position)
        {
            ServerSend.UpdatePlayerPosition(ClientID, position);
        }
    }

    public void Destroy()
    {
        QueueFree();

        // Destroy any objects related to this player.
    }

    public void OnAreaEntered(Area2D area)
    {
        if (!NetworkManager.IsServer())
        {
            return;
        }

        if (area is Ball ball)
        {
            // Assign new direction
            ball.direction = new Vector2(ClientID % 2 == 1 ? 1 : -1, ((float)new Random().NextDouble()) * 2 - 1).Normalized();

            // Increase speed by 5%
            ball.Speed *= 1.05f;
        }
    }
}
