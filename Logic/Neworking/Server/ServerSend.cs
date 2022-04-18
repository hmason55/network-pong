using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class ServerSend
{
    private static void TcpSend(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.Instance.Clients[toClient].TcpSend(packet);
    }

    private static void UdpSend(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.Instance.Clients[_toClient].UdpSend(_packet);
    }

    /// <summary>
    /// Sends packet to all clients except the ones listed in ignoreClients.
    /// </summary>
    public static void TcpBroadcast(Packet packet, params int[] ignoreClients)
    {
        packet.WriteLength();
        foreach (int clientID in Server.Instance.Clients.Keys)
        {
            if (ignoreClients.Contains(clientID))
            {
                continue;
            }

            Server.Instance.Clients[clientID].TcpSend(packet);
        }
    }

    /// <summary>
    /// Sends packet to all clients except the ones listed in ignoreClients.
    /// </summary>
    public static void UdpBroadcast(Packet packet, params int[] ignoreClients)
    {
        packet.WriteLength();
        foreach (int clientID in Server.Instance.Clients.Keys)
        {
            if (ignoreClients.Contains(clientID))
            {
                continue;
            }

            Server.Instance.Clients[clientID].UdpSend(packet);
        }
    }

    // Server data methods
    public static void ConnectPlayer(int clientID, string message)
    {
        Console.WriteLine($"Server (ConnectPlayer: {clientID}, {message})");

        using Packet packet = new((int)ServerPackets.ConnectPlayer);
        packet.WriteInt(clientID);
        packet.WriteString(message);

        TcpSend(clientID, packet);
    }

    public static void SpawnPlayer(int toClient, int clientID, string username, Vector2 position)
    {
        Console.WriteLine($"Server (SpawnPlayer: {clientID}, {username}, {position})");

        using Packet packet = new((int)ServerPackets.SpawnPlayer);
        packet.WriteInt(clientID);
        packet.WriteString(username);
        packet.WriteVector2(position);

        UdpSend(toClient, packet);
    }

    public static void SpawnBall(Vector2 position)
    {
        Console.WriteLine($"Server (SpawnBall: {position})");

        using Packet packet = new((int)ServerPackets.SpawnBall);
        packet.WriteVector2(position);

        UdpBroadcast(packet);
    }

    public static void UpdatePlayerPosition(int clientID, Vector2 position)
    {
        Console.WriteLine($"Server (UpdatePlayerPosition: {clientID}, {position})");

        using Packet packet = new((int)ServerPackets.UpdatePlayerPosition);
        packet.WriteInt(clientID);
        packet.WriteVector2(position);

        UdpBroadcast(packet);
    }

    public static void UpdateBallPosition(Vector2 position, int exceptClient, bool interpolate)
    {
        Console.WriteLine($"Server (UpdateBallPosition: {position})");

        using Packet packet = new((int)ServerPackets.UpdateBallPosition);
        packet.WriteVector2(position);
        packet.WriteBool(interpolate);

        UdpBroadcast(packet, exceptClient);
    }
}

