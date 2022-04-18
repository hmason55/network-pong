using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;

public class Client : ClientNode
{
	// Lazy singleton, there should only ever be one client.
	private static readonly Lazy<Client> _instance = new(() => new());
	public static Client Instance { get { return _instance.Value; } }

	public string Username = "Username";
	public string Address = "127.0.0.1";
	public int RemotePort = 50001;

	public bool Online = false;

	public override void _Ready()
	{
		EndPoint = new(IPAddress.Parse(Address), RemotePort);

		PacketHandlers = new()
		{
			{ (int)ServerPackets.ConnectPlayer, ClientReceive.ConnectPlayer },
			{ (int)ServerPackets.SpawnPlayer, ClientReceive.SpawnPlayer },
			{ (int)ServerPackets.SpawnBall, ClientReceive.SpawnBall },
			{ (int)ServerPackets.UpdatePlayerPosition, ClientReceive.UpdatePlayerPosition },
			{ (int)ServerPackets.UpdateBallPosition, ClientReceive.UpdateBallPosition },
			{ (int)ServerPackets.DisconnectPlayer, ClientReceive.DisconnectPlayer }
		};

		Console.WriteLine("Init client self");
		TcpConnect();
	}

	public override void _Process(float delta)
	{
		ExecuteNetworkActions();
	}

	public void TcpConnect()
	{
		Console.WriteLine($"Client (TcpConnect)");

		TcpClient = new()
		{
			ReceiveBufferSize = BUFFER_SIZE,
			SendBufferSize = BUFFER_SIZE
		};

		ReceiveBuffer = new byte[BUFFER_SIZE];
		TcpClient.BeginConnect(Address, RemotePort, TcpConnectCallback, TcpClient);
	}

	public bool UdpConnect(int localPort)
	{
		Console.WriteLine($"Client (UdpConnect)");

		if (UdpClient != null)
		{
			return false;
		}

		UdpClient = new(localPort);

		try
		{
			UdpClient.Connect(EndPoint);
			UdpClient.BeginReceive(UdpReceiveCallback, null);

			using Packet packet = new();
			UdpSend(packet);
		} 
		catch (Exception e)
		{
			Console.WriteLine($"Connection failed: {e.Message}");
			return false;
		}

		return true;
	}

	/// <summary>Sends data to the client via TCP.</summary>
	/// <param name="packet">The packet to send.</param>
	public void TcpSend(Packet packet)
	{
		try
		{
			if (TcpClient != null)
			{
				Stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
			}
		}
		catch (Exception e)
		{
			Console.WriteLine($"Error sending TCP data to server: {e}");
		}
	}

	public void UdpSend(Packet packet)
	{
		try
		{
			if (UdpClient != null)
			{
				packet.InsertInt(ID);
				UdpClient.BeginSend(packet.ToArray(), packet.Length(), null, null);
			}
		}
		catch (Exception e)
		{
			Console.WriteLine($"Error sending data to server via UDP: {e.Message}");
		}
	}

	/// <summary>Initializes the newly connected client's TCP-related info.</summary>
	private void TcpConnectCallback(IAsyncResult result)
	{
		TcpClient.EndConnect(result);

		if (!TcpClient.Connected)
		{
			return;
		}

		Online = true;
		ReceivedData = new();
		Stream = TcpClient.GetStream();

		Stream.BeginRead(ReceiveBuffer, 0, BUFFER_SIZE, TcpReceiveCallback, null);
	}

	/// <summary>Reads incoming data from the stream.</summary>
	private void TcpReceiveCallback(IAsyncResult result)
	{
		try
		{
			int bytes = Stream.EndRead(result);
			if (bytes <= 0)
			{
				Disconnect();
				return;
			}

			byte[] data = new byte[bytes];
			Array.Copy(ReceiveBuffer, data, bytes);

			ReceivedData.Reset(TcpHandle(data)); // Reset receivedData if all data was handled
			Stream.BeginRead(ReceiveBuffer, 0, BUFFER_SIZE, TcpReceiveCallback, null);
		}
		catch
		{
			Disconnect();
		}
	}

	/// <summary>Receives incoming UDP data.</summary>
	private void UdpReceiveCallback(IAsyncResult result)
	{
		try
		{
			byte[] data = UdpClient.EndReceive(result, ref EndPoint);
			UdpClient.BeginReceive(UdpReceiveCallback, null);

			if (data.Length < sizeof(int))
			{
				Disconnect();
				return;
			}

			UdpHandle(data);
		}
		catch
		{
			Disconnect();
		}
	}

	/// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
	/// <param name="data">The recieved data.</param>
	private bool TcpHandle(byte[] _data)
	{
		int _packetLength = 0;

		ReceivedData.SetBytes(_data);

		if (ReceivedData.UnreadLength() >= sizeof(int))
		{
			// If client's received data contains a packet
			_packetLength = ReceivedData.ReadInt();
			Console.WriteLine($"_packetLength: {_packetLength}");
			if (_packetLength <= 0)
			{
				// If packet contains no data
				return true; // Reset receivedData instance to allow it to be reused
			}
		}

		while (_packetLength > 0 && _packetLength <= ReceivedData.UnreadLength())
		{
			// While packet contains data AND packet data length doesn't exceed the length of the packet we're reading
			byte[] bytes = ReceivedData.ReadBytes(_packetLength);
			QueueNetworkAction(() =>
			{
                using Packet packet = new(bytes);
                int id = packet.ReadInt();
                PacketHandlers[id](packet); // Call appropriate method to handle the packet
            });

			_packetLength = 0; // Reset packet length
			if (ReceivedData.UnreadLength() >= sizeof(int))
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

	private void UdpHandle(byte[] data)
    {
		using Packet packet = new(data);

		int _packetLength = packet.ReadInt();
		data = packet.ReadBytes(_packetLength);

		QueueNetworkAction(() =>
		{
			using Packet packet = new(data);

			int id = packet.ReadInt();
			PacketHandlers[id](packet);
		});
	}
}
