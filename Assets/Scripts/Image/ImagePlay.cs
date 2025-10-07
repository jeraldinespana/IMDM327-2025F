// IMDM 327
// Import an image and plot into point clouds + interactive update
// Myungin Lee (2025)

using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public class ImagePlay : MonoBehaviour
{
    [SerializeField] private string imageFileName;
    [SerializeField, Range(1, 64)] private int decimate = 16;
    [SerializeField] private float depthOffset = 30f;
    [SerializeField, Range(0f, 1f)] private float alphaThreshold = 0.02f;

    private GameObject[] _cubes; // cubes built from the image pixels
    private int _columns, _rows; // how many samples across the image
    private float _time; // running time for animation
    public float timescale = 0.001f; // tweak animation speed in the Inspector
    private int width, height; // cached texture size
    Color32[] pixels; // cached pixel colors

    private void Start()
    {
        var texture = LoadTexture(imageFileName); // load texture data at startup
        if (texture == null)
        {
            Debug.LogWarning($"ImagePlay: texture '{imageFileName}' not found in Resources or Assets/Resources.", this);
            return;
        }

        pixels = texture.GetPixels32(); // grab all pixels once
        width = texture.width;
        height = texture.height;
        _columns = width / decimate;
        _rows = height / decimate;

        _cubes = new GameObject[_columns * _rows]; // allocate cube slots

        int index = 0;
        for (int x = 0; x < _columns; x++)
        {
            int sampleX = x * decimate; // sample column in the source image
            for (int y = 0; y < _rows; y++)
            {
                int sampleY = y * decimate; // sample row in the source image
                Color pixel = pixels[sampleY * width + sampleX];

                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube); // make a tiny cube
                cube.transform.SetParent(transform, false);
                cube.transform.localPosition = new Vector3(x, y, depthOffset); // grid placement
                float scaleZ = 1f + 30f * (pixel.r); // simple scale based on red channel
                cube.transform.localScale = new Vector3(1f, 1f, scaleZ);

                cube.GetComponent<MeshRenderer>().material.color = pixel; // color the cube
                _cubes[index] = cube; // remember this cube
                index++;
            }
        }
    }

    private void Update()
    {
        int index = 0;
        float scaledTime = _time * timescale; // slow down or speed up animation nicely
        for (int x = 0; x < _columns; x++)
        {
            int sampleX = x * decimate;
            for (int y = 0; y < _rows; y++)
            {
                var cube = _cubes[index]; // fetch the cube for this pixel
                int sampleY = y * decimate;
                Color pixel = pixels[sampleY * width + sampleX];

                float posX = x; // base grid position
                float posY = y;
                float posZ = depthOffset * (1f + Mathf.Sin(x * scaledTime)); // gentle z motion
                cube.transform.localPosition = new Vector3(posX, posY, posZ);
                // cube.transform.Rotate(pixel.r, pixel.g, pixel.b);
                index++;
            }
        }
        _time += Time.deltaTime; // advance shared timer
    }

    private static Texture2D LoadTexture(string baseName)
    {
        string fileName = Path.HasExtension(baseName) ? baseName : baseName + ".png"; // ensure extension
        string filePath = Path.Combine(Application.dataPath, "Resources", fileName); // look in Assets/Resources
        byte[] bytes = File.ReadAllBytes(filePath); // read the raw bytes
        var readableTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false); // create a readable copy
        readableTexture.LoadImage(bytes); // fill texture with image data
        return readableTexture;
    }
}
