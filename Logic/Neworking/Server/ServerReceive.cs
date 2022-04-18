using Godot;
using System;

public class ServerReceive
{
    public static void RequestConnection(int clientID, Packet packet)
    {
        Console.WriteLine($"Server (RequestConnection: {clientID})");

        int actualID = packet.ReadInt();
        string username = packet.ReadString();

        Console.WriteLine($"{Server.Instance.Clients[clientID].TcpClient.Client.RemoteEndPoint} ({username}) connected successfully and is now player {clientID}.");
        if (clientID != actualID)
        {
            Console.WriteLine($"Player \"{username}\" (ID: {clientID}) has provided the wrong ID ({actualID})");
        }

        Server.Instance.Clients[clientID].SendIntoGame(username);
    }

    public static void RequestPaddleMovement(int clientID, Packet packet)
    {
        Vector2 direction = packet.ReadVector2();
        Console.WriteLine("Updating player position");
        GameManager.Instance.Players[clientID].UpdatePosition(direction);
    }

    public static void PlayerMouseMovement(int clientID, Packet packet)
    {
        /*
        Vector3 destination = _packet.ReadVector3();

        Player player = Server.clients[_fromClient].player;
        player.useDirectionalMovement = false;
        player.SetDestination(destination);
        player.castWhenAble = false;
        */
    }
}
