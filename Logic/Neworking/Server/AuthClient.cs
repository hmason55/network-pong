using Godot;
using System;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// This class is how the server interfaces with each client.
/// </summary>
public class AuthClient : ClientNode
{
    public AuthClient()
    {
    }

    public AuthClient(int clientID)
    {
        ID = clientID;
    }

    public override void _Process(float delta)
    {
        ExecuteNetworkActions();
    }

    /// <summary>Initializes the newly connected client's TCP-related info.</summary>
    /// <param name="_socket">The TcpClient instance of the newly connected client.</param>
    public void TcpConnect(TcpClient tcpClient)
    {
        TcpClient = tcpClient;
        TcpClient.ReceiveBufferSize = BUFFER_SIZE;
        TcpClient.SendBufferSize = BUFFER_SIZE;

        Stream = TcpClient.GetStream();

        ReceivedData = new();
        ReceiveBuffer = new byte[BUFFER_SIZE];

        Stream.BeginRead(ReceiveBuffer, 0, BUFFER_SIZE, ReceiveCallback, null);

        ServerSend.ConnectPlayer(ID, "Hello");
    }

    /// <summary>Initializes the newly connected client's UDP-related info.</summary>
    /// <param name="_endPoint">The IPEndPoint instance of the newly connected client.</param>
    public void UdpConnect(IPEndPoint endPoint)
    {
        EndPoint = endPoint;
    }

    /// <summary>Sends data to the client via TCP.</summary>
    /// <param name="packet">The packet to send.</param>
    public void TcpSend(Packet packet)
    {
        try
        {
            if (TcpClient != null)
            {
                Console.WriteLine($"AuthClient (TcpSend)");
                Stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null); // Send data to appropriate client
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error sending TCP data to player {ID}: {e}");
        }
    }

    public void UdpSend(Packet packet)
    {
        Server.Instance.UdpSend(EndPoint, packet);
    }

    /// <summary>Reads incoming data from the stream.</summary>
    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            int bytes = Stream.EndRead(result);
            if (bytes <= 0)
            {
                Server.Instance.Clients[ID]?.Disconnect();
                return;
            }

            byte[] data = new byte[bytes];
            Array.Copy(ReceiveBuffer, data, bytes);

            ReceivedData.Reset(TcpHandle(data)); // Reset receivedData if all data was handled
            Stream.BeginRead(ReceiveBuffer, 0, BUFFER_SIZE, ReceiveCallback, null);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error receiving data: {e}");
            Server.Instance.Clients[ID]?.Disconnect();
        }
    }

    /// <summary>
    /// Prepares received data to be used by the appropriate packet handler methods.
    /// Used for TCP.
    /// </summary>
    /// <param name="data">The recieved data.</param>
    /// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
    /// <param name="_data">The recieved data.</param>
    private bool TcpHandle(byte[] _data)
    {
        int _packetLength = 0;

        ReceivedData.SetBytes(_data);

        if (ReceivedData.UnreadLength() >= 4)
        {
            // If client's received data contains a packet
            _packetLength = ReceivedData.ReadInt();
            if (_packetLength <= 0)
            {
                // If packet contains no data
                return true; // Reset receivedData instance to allow it to be reused
            }
        }

        while (_packetLength > 0 && _packetLength <= ReceivedData.UnreadLength())
        {
            // While packet contains data AND packet data length doesn't exceed the length of the packet we're reading
            byte[] _packetBytes = ReceivedData.ReadBytes(_packetLength);
            Console.WriteLine($"Packet bytes: {_packetBytes.Length}");
            Console.WriteLine($"data bytes: {_data.Length}");
            QueueNetworkAction(() =>
            {
                using Packet _packet = new(_packetBytes);
                int _packetId = _packet.ReadInt();
                Server.Instance.PacketHandlers[_packetId](ID, _packet); // Call appropriate method to handle the packet
            });

            _packetLength = 0; // Reset packet length
            if (ReceivedData.UnreadLength() >= 4)
            {
                // If client's received data contains another packet
                _packetLength = ReceivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    // If packet contains no data
                    return true; // Reset receivedData instance to allow it to be reused
                }
            }
        }

        if (_packetLength <= 1)
        {
            return true; // Reset receivedData instance to allow it to be reused
        }

        return false;
    }

    /// <summary>
    /// Prepares received data to be used by the appropriate packet handler methods.
    /// Used for UDP.
    /// </summary>
    /// <param name="packet">The packet containing the recieved data.</param>
    public void UdpHandle(Packet packet)
    {
        int length = packet.ReadInt();
        byte[] bytes = packet.ReadBytes(length);
        Console.WriteLine("AuthClient (UdpHandle)");

        QueueNetworkAction(() =>
        {
            using Packet packet = new(bytes);
            int id = packet.ReadInt();

            // Handle the packet by calling the corresponding method.
            Server.Instance.PacketHandlers[id](ID, packet);
        });
    }

    public void SendIntoGame(string username)
    {
        // Spawn player
        Vector2 position = new(640 - (ID == 1 ? 560 : 80), 200);
        GameManager.Instance.SpawnPlayer(ID, username, position);
        Console.WriteLine(Server.Instance.Clients.Values.Count);

        foreach (AuthClient client in Server.Instance.Clients.Values)
        {
            if(!GameManager.Instance.Players.ContainsKey(client.ID))
            {
                continue;
            }

            if (client.ID != ID)
            {
                Player player = GameManager.Instance.Players[client.ID];

                // Send all players to the new player
                ServerSend.SpawnPlayer(ID, client.ID, player.Username, player.Position);
            }

            // Send the new player to all players (including himself)
            ServerSend.SpawnPlayer(client.ID, ID, username, position);

            
        }

        //if (GameManager.Instance.Players.Count > 1)
        //{
            ServerSend.SpawnBall(new(320, 200));
        //}

        GetNode<Node>(Nodes.ROOT).PrintTreePretty();
    }
}
