using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Godot;

public abstract class ClientNode : NetworkNode, IPacketReceiver
{
	public delegate void PacketHandler(Packet packet);
	public Dictionary<int, PacketHandler> PacketHandlers { get; protected set; }

	public NetworkStream Stream { get; set; }
	public Packet ReceivedData { get; set; }
	public byte[] ReceiveBuffer { get; set; }

	public int ID;

	public IPEndPoint EndPoint;
	public TcpClient TcpClient { get; set; }
	public UdpClient UdpClient { get; protected set; }

	public const int BUFFER_SIZE = 4096;
	
	protected void Disconnect()
	{
		TcpClient?.Close();
		Stream = null;
		ReceivedData = null;
		ReceiveBuffer = null;

		EndPoint = null;

		QueueFree();
	}
}

