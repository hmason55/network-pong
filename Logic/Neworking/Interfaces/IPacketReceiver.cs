using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


internal interface IPacketReceiver
{
	internal NetworkStream Stream { get; set; }
	internal Packet ReceivedData { get; set; }
	internal byte[] ReceiveBuffer { get; set; }
}
