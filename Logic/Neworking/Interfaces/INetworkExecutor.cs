using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

internal interface INetworkExecutor
{
	internal Queue<Action> NetworkActions { get; }
	internal Queue<Action> BufferedNetworkActions { get; }

	internal void QueueNetworkAction(Action action);
    internal void ExecuteNetworkActions();
}