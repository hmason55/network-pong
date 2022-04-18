using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class PrefabLoader
{
    public static T Load<T>(string path) where T : class
    {
        PackedScene prefab = ResourceLoader.Load<PackedScene>(path);
        return prefab.Instance<T>();
    }
}
