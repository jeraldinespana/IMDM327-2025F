using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderAnimator : MonoBehaviour
{
    public Material mat;
    public Vector2 offset = new Vector2(0.0f, 0.0f);
    public float zoom = 1f;
    private float zoomSpeed = 0.5f;

    // Update is called once per frame
    void Update()
    {
        zoom += zoomSpeed * Time.deltaTime;
        
        mat.SetVector("_Offset", new Vector4(offset.x, offset.y, 0, 0));
        mat.SetFloat("_Zoom", zoom);
    }
}
