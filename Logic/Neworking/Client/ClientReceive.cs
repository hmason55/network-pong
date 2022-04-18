using Godot;
using System;
using System.Net;

public class ClientReceive
{
	public static void ConnectPlayer(Packet packet)
	{
		Console.WriteLine("ClientHandle (ConnectPlayer)");

		int id = packet.ReadInt();
		string message = packet.ReadString();

		Console.WriteLine($"Message from server: {message}");
		Client.Instance.ID = id;

		ClientSend.RequestConnection();

		// Now that we have the client's id, connect UDP
		Client.Instance.UdpConnect(((IPEndPoint)Client.Instance.TcpClient.Client.LocalEndPoint).Port);
	}

	#region Player
	public static void SpawnPlayer(Packet packet)
	{
		Console.WriteLine($"Spawning player for client");
		int id = packet.ReadInt();
		string username = packet.ReadString();
		Vector2 position = packet.ReadVector2();

		GameManager.Instance.SpawnPlayer(id, username, position);
	}

	public static void SpawnBall(Packet packet)
	{
		Console.WriteLine($"Spawning ball for client");
		Vector2 position = packet.ReadVector2();

		GameManager.Instance.SpawnBall(position);
	}

	public static void UpdatePlayerPosition(Packet packet)
	{
		int id = packet.ReadInt();
		Vector2 position = packet.ReadVector2();

		if (GameManager.Instance.Players.TryGetValue(id, out Player player))
		{
			player.TargetPosition = position;
		}
	}

	public static void UpdateBallPosition(Packet packet)
	{
		Vector2 position = packet.ReadVector2();
		bool interpolate = packet.ReadBool();

		if(GameManager.Instance.Ball == null)
        {
			return;
        }

		GameManager.Instance.Ball.Position = position;
	}

	public static void DisconnectPlayer(Packet packet)
	{
		int id = packet.ReadInt();

		//Destroy(GameManager.players[_id].gameObject);
		//GameManager.players.Remove(_id);
	}
	#endregion
}
