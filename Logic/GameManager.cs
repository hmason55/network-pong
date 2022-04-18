using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public class GameManager : Node
{
    // Lazy singleton, there should only ever be one game manager.
    private static readonly Lazy<GameManager> _instance = new(() => new());
    public static GameManager Instance { get { return _instance.Value; } }

    public Dictionary<int, Player> Players = new();
    public Ball Ball;

    public void SpawnPlayer(int clientID, string username, Vector2 position)
    {
        Console.WriteLine("Spawning player in game manager");

        if(Players.ContainsKey(clientID))
        {
            return;
        }

        lock(Players)
        {
            Player player = PrefabLoader.Load<Player>("res://Prefabs/Player.tscn");
            player.ClientID = clientID;
            player.SetUsername(username);
            player.Position = position;

            GetNode<Node>(Nodes.TABLE).AddChild(player, true);
            Players.Add(clientID, player);
        }
    }

    public void DestroyPlayer(int clientID)
    {
        if(!Players.ContainsKey(clientID))
        {
            return;
        }

        lock (Players)
        {
            Players[clientID].Destroy();
            Players.Remove(clientID);
        }
    }

    public void SpawnBall(Vector2 position)
    {
        if(Ball == null)
        {
            Ball ball = PrefabLoader.Load<Ball>("res://Prefabs/Ball.tscn");
            ball.Position = position;
            ball.TargetPosition = position;
            GetNode<Node>(Nodes.TABLE).AddChild(ball, true);
            Ball = ball;
        }
    }

    public void DestroyBall()
    {
        Ball?.QueueFree();
    }
}