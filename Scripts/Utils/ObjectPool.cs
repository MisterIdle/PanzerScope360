using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// I'm using object pooling because my phone struggles with on-the-fly instantiations
/// I'm using it to ensure smooth gameplay and for my own comfort during debugging
/// </summary>

public class ObjectPool
{
    private Queue<Node> pool = new Queue<Node>();
    private PackedScene scene;
    private Node parent;
    private Action<Node> onInstantiate;

    public ObjectPool(PackedScene pScene, int pInitialSize, Node pParent, Action<Node> pOnInstantiate = null)
    {
        scene = pScene;
        parent = pParent;
        onInstantiate = pOnInstantiate;

        for (int i = 0; i < pInitialSize; i++)
            CreateNewInstance();
    }

    private Node CreateNewInstance()
    {
        Node lInstance = scene.Instantiate();

        parent.AddChild(lInstance);
        onInstantiate?.Invoke(lInstance);
        pool.Enqueue(lInstance);

        return lInstance;
    }

    public Node Get()
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        Node lInstance = CreateNewInstance();
        return pool.Dequeue();
    }

    public void Release(Node pInstance)
    {
        pool.Enqueue(pInstance);
    }
}