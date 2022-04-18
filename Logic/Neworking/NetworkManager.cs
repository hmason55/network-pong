using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NetworkManager
{
    public static NetworkModes NetworkMode = NetworkModes.Server;
    public static bool IsServer() => NetworkMode == NetworkModes.Server;
}
