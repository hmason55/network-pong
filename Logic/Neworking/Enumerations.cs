/// <summary>
/// Sent from the server to client.
/// </summary>
public enum ServerPackets
{
    ConnectPlayer = 1,
    SpawnPlayer = 2,
    SpawnBall = 3,
    UpdatePlayerPosition = 4,
    UpdateBallPosition = 5,
    DisconnectPlayer = 6
}

/// <summary>
/// Sent from client to server.
/// </summary>
public enum ClientPackets
{
    RequestConnection = 1,
    RequestPlayerMovement = 2,
    RequestDisconnect = 3,
}

public enum NetworkModes
{
    Server,
    Client
}