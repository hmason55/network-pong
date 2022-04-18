using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

public class Server : NetworkNode
{
    // Lazy singleton, there should only ever be one server.
    private static readonly Lazy<Server> _instance = new(() => new());
    public static Server Instance { get { return _instance.Value; } }

    public static int MaxPlayers = 2;

    public delegate void PacketHandler(int fromClient, Packet packet);
    public Dictionary<int, PacketHandler> PacketHandlers { get; private set; }

    protected TcpListener TcpListener;
    protected UdpClient UdpListener;

    public Dictionary<int, AuthClient> Clients { get; private set; } = new();

    public string Address = "127.0.0.1";
    public int Port = 50001;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Console.WriteLine("Starting server.");

        // Initialize authoritative clients.
        InitClients();

        // Packets that the server receives from clients.
        PacketHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.RequestConnection, ServerReceive.RequestConnection },
            { (int)ClientPackets.RequestPlayerMovement, ServerReceive.RequestPaddleMovement },
            { (int)ClientPackets.RequestDisconnect, ServerReceive.PlayerMouseMovement },
        };

        // Start listening on a port.
        InitListeners();
    }

    public override void _Process(float delta)
    {
        ExecuteNetworkActions();
    }


    public void InitClients()
    {
        for (int i = 1; i <= MaxPlayers; i++)
        {
            AuthClient client = new(i);
            client.Name = $"AuthClient {i}";
            GetNode(Nodes.NETWORK_MANAGER).AddChild(client, true);
            Clients.Add(i, client);
        }

        // Debug
        GetNode(Nodes.ROOT).PrintTreePretty();

        Console.WriteLine($"Initialized {Clients.Count()} clients.");
    }

    public void InitListeners()
    {
        TcpListener = new(IPAddress.Any, Port);
        TcpListener.Start();
        TcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);

        UdpListener = new(Port);
        UdpListener.BeginReceive(UdpReceiveCallback, null);

        Console.WriteLine($"Server is listening on port {Port}.");
    }

    /// <summary>Handles new TCP connections.</summary>
    private void TcpConnectCallback(IAsyncResult result)
    {
        try
        {
            TcpClient client = TcpListener.EndAcceptTcpClient(result);
            TcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (Clients[i].TcpClient == null)
                {
                    Clients[i].TcpConnect(client);
                    return;
                }
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server is full.");
        }
        catch
        {
            Console.WriteLine($"Server has been shut down.");
        }
    }

    private void UdpReceiveCallback(IAsyncResult result)
    {
        try
        {
            IPEndPoint endPoint = new(IPAddress.Any, 0);
            byte[] data = UdpListener.EndReceive(result, ref endPoint);
            UdpListener.BeginReceive(UdpReceiveCallback, null);

            if (data.Length < sizeof(int))
            {
                return;
            }

            using Packet packet = new(data);
            int clientID = packet.ReadInt();

            if (clientID == 0)
            {
                return;
            }

            if (Clients[clientID].EndPoint == null)
            {
                // If this is a new connection
                Clients[clientID].UdpConnect(endPoint);
                return;
            }

            if (Clients[clientID].EndPoint.ToString() == endPoint.ToString())
            {
                // Ensures that the client is not being impersonated by another by sending a false clientID
                Clients[clientID].UdpHandle(packet);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error receiving data: {e.Message}");
        }
    }

    /// <summary>Sends a packet to the specified endpoint via UDP.</summary>
    /// <param name="endPoint">The endpoint to send the packet to.</param>
    /// <param name="packet">The packet to send.</param>
    public void UdpSend(IPEndPoint endPoint, Packet packet)
    {
        try
        {
            if (endPoint != null)
            {
                UdpListener.BeginSend(packet.ToArray(), packet.Length(), endPoint, null, null);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error sending UDP data to {endPoint}: {e}");
        }
    }

    
}
