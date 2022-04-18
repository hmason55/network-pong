using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Godot;

public abstract class NetworkNode : Node, INetworkExecutor
{
	public Queue<Action> NetworkActions { get; } = new();
	public Queue<Action> BufferedNetworkActions { get; } = new();

	public void ExecuteNetworkActions()
    {
		if (NetworkActions.Count < 1)
		{
			return;
		}

		BufferedNetworkActions.Clear();

		lock (NetworkActions)
		{
			foreach (Action action in NetworkActions)
			{
				BufferedNetworkActions.Enqueue(action);
			}

			NetworkActions.Clear();
		}

		foreach (Action action in BufferedNetworkActions.ToList())
		{
			BufferedNetworkActions.Dequeue().Invoke();
		}
	}

    public void QueueNetworkAction(Action action)
    {
		if (action == null)
		{
			return;
		}

		lock (NetworkActions)
		{
			Console.WriteLine("Adding networking action to queue.");
			NetworkActions.Enqueue(action);
		}
	}
}
