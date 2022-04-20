using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileIndicator
{
    public GameObject Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

    public Color Color { get => _color; private set => _color = value; }
    // public Renderer color; ?
    [SerializeField] private Collider _tileCollider;

    private Color _color;

    public TileIndicator()
    {
        _color = new Color(UnityEngine.Random.Range(0, 1.0f), UnityEngine.Random.Range(0, 1.0f), UnityEngine.Random.Range(0, 1.0f));
    }

    public TileIndicator(Color color, ColliderStat coll_model)
    {
        Color = color;
        //TileCollider = Instantiate<ColliderStat>(coll_model, Cube.transform);
    }
}
