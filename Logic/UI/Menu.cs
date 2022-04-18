using Godot;
using System;
using System.Diagnostics;

public class Menu : Control
{
	public static bool IsPaused { get; private set; } = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		RandomizeUsername();
	}

	public void RandomizeUsername()
    {
		GetNode<LineEdit>("Center Container/Grid Container/Username").Text = Guid.NewGuid().ToString("N").Substring(0, 6);
	}

	public override void _Process(float delta)
	{
		UpdateFps();
	}

	public void UpdateFps()
    {
		GetNode<Label>($"{Nodes.MAIN}/FPS").Text = Engine.GetFramesPerSecond().ToString();
    }

	public void OnHost()
	{
		Console.WriteLine("Match created.");
		GetNode<Node>(Nodes.MAIN).AddChild(GameManager.Instance, true);
		SetupServer();
		SetupClient();
		StartGame();
	}

	public void OnJoin()
	{
		Console.WriteLine("Joining a match...");
		GetNode<Node>(Nodes.MAIN).AddChild(GameManager.Instance, true);
		NetworkManager.NetworkMode = NetworkModes.Client;
		SetupClient();
		StartGame();
	}
	
	private void OnQuit()
	{
		Console.WriteLine("Exiting the game...");
		GetTree().Notification(MainLoop.NotificationWmQuitRequest);
	}

	public void ShowTable()
	{
		GetNode<Control>(Nodes.TABLE).Show();
	}

	public void HideTable()
	{
		GetNode<Control>(Nodes.TABLE).Hide();
	}

	public void TogglePause()
	{
		// Not implemented yet.
		return;

		IsPaused = !IsPaused;

		if (IsPaused)
		{
			GetNode<Control>("Center Container").Show();
		}
		else
		{
			GetNode<Control>("Center Container").Hide();
		}
	}

	public override void _Input(InputEvent e)
	{
		if (e is InputEventKey)
		{
			ParseKeyPressed(e as InputEventKey);
		}
	}

	public void ParseKeyPressed(InputEventKey e)
	{
		if(!e.IsPressed())
		{
			return;
		}

		switch((KeyList)e.Scancode)
		{
			case KeyList.Escape:
				if (!e.Echo)
				{
					TogglePause();
				}
				return;

			default:
				return;
		}
	}

	public void SetupClient()
	{
		GetNode<Node>(Nodes.NETWORK_MANAGER).AddChild(Client.Instance, true);
		Client.Instance.Username = GetNode<LineEdit>("Center Container/Grid Container/Username").Text;

		TimeSpan timeout = TimeSpan.FromSeconds(5);
		Stopwatch stopwatch = new();
		stopwatch.Start();
		Client.Instance.Online = false;
		while(!Client.Instance.Online && stopwatch.Elapsed < timeout)
        {
			GetTree().CreateTimer(1);
        }
	}

	public void SetupServer()
	{
		GetNode<Node>(Nodes.NETWORK_MANAGER).AddChild(Server.Instance, true);
	}

	public void StartGame()
	{
		IsPaused = false;
		GetNode<Control>("Center Container").Hide();
		ShowTable();
	}
}
