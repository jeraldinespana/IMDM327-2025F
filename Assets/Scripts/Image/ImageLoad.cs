// IMDM 327
// Import an image and draw on a plane as a material
// Myungin Lee (2025)

using UnityEngine;

public class ImageLoad : MonoBehaviour
{
    private GameObject palette;

    private void Start()
    {
        var tex = Resources.Load<Texture2D>("UMD-logo");
        palette = GameObject.CreatePrimitive(PrimitiveType.Plane);

        var renderer = palette.GetComponent<MeshRenderer>();
        var material = new Material(Shader.Find("Standard"))
        {
            mainTexture = tex,
            color = Color.white
        };

        material.SetFloat("_Mode", 3f); // Standard shader transparent mode
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        renderer.material = material;
    }
}
