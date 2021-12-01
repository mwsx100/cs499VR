using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bucket : MonoBehaviour
{
    public Texture2D texture;
    public Vector2 textureSize = new Vector2(x: 2048, y: 2048);
    public Color color;
    void Start()
    {
        var r = GetComponent<Renderer>();
        color = r.material.color;
        texture = new Texture2D(width: (int)textureSize.x, height: (int)textureSize.y);
        r.material.mainTexture = texture;
    }



}