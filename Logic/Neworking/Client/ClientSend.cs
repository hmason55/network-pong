using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ClientSend
{
    /// <summary>Sends a packet to the server via TCP.</summary>
    /// <param name="packet">The packet to send to the sever.</param>
    private static void TcpSend(Packet packet)
    {
        packet.WriteLength();
        Client.Instance.TcpSend(packet);
    }

    /// <summary>Sends a packet to the server via UDP.</summary>
    /// <param name="packet">The packet to send to the sever.</param>
    private static void UdpSend(Packet packet)
    {
        packet.WriteLength();
        if (Client.Instance.UdpClient == null) { return; }

        Client.Instance.UdpSend(packet);
    }

    /// <summary>Lets the server know that the welcome message was received.</summary>
    public static void RequestConnection()
    {
        using Packet packet = new((int)ClientPackets.RequestConnection);
        packet.WriteInt(Client.Instance.ID);
        packet.WriteString(Client.Instance.Username);

        TcpSend(packet);
    }

    /// <summary>Sends player input to the server.</summary>
    /// <param name="_inputs"></param>
    public static void RequestPlayerMovement(Vector2 direction)
    {
        using Packet packet = new((int)ClientPackets.RequestPlayerMovement);
        packet.WriteVector2(direction);

        UdpSend(packet);
    }
}
