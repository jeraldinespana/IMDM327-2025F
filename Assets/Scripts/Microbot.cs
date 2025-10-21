using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Microbot : MonoBehaviour
{
    public enum MicrobotState {Boids, Structure}
    public MicrobotState currState = MicrobotState.Boids;
    private GameObject[] body;
    BodyProperty[] bp;
    private int numberOfCaps = 15;
    public float fastforwardConst = 1f;
    TrailRenderer[] trailRenderer;
    private GameObject interactivePoint;
    public Vector3 interactPoint;// where to interact 
    private Vector3 previousInteractivePoint;
    public float interactiveMass; // how much to interact
    MediaPipeBodyTracker mp;
    public float maxVelocity;
    public float closeDistance;
    int frameCount;
    public float nearBy = 15f;
    public float separateConst = 10f;
    public float cohesConst = 5f;
    public float alignConst = 5f;
    public Vector3[] structures;
    private bool builtStructure = false;
    private bool[] inPlace;


    struct BodyProperty 
    {                   
        public float mass;
        public Vector3 velocity;
        public Vector3 acceleration;
    }


    void Start() // Start is called once before the first execution of Update after the MonoBehaviour is created

    {
        if (mp == null)
        {
            mp = FindObjectOfType<MediaPipeBodyTracker>();
            if (mp == null)
            {
                Debug.LogWarning("Nanobot could not locate a MediaPipeBodyTracker in the scene.");
            }
        }
        // init condition
        maxVelocity = 20f;
        interactiveMass = 30f;
        closeDistance = 16f; // sqrt value
        // interactive point 
        interactivePoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        interactivePoint.transform.position = new Vector3(0f, 0f, 0f);
        // Just like GO, computer should know how many room for struct is required:
        bp = new BodyProperty[numberOfCaps];
        body = new GameObject[numberOfCaps];
        trailRenderer = new TrailRenderer[numberOfCaps];
        // Loop generating the gameobject and assign initial conditions (type, position, (mass/velocity/acceleration)
        for (int i = 0; i < numberOfCaps; i++)
        {
            // Our gameobjects are created here:
            body[i] = GameObject.CreatePrimitive(PrimitiveType.Capsule); 

            // initial conditions
            float r = 100f;
            
            body[i].transform.position = new Vector3(r * Mathf.Sin(i * 2f * Mathf.PI / numberOfCaps),
                                                      r * Mathf.Cos(i * 2f * Mathf.PI / numberOfCaps),
                                                      180f + Random.Range(-10f, 10f));

            bp[i].velocity = new Vector3(0, 0, 0); // Try different initial condition
            bp[i].mass = Random.Range(1f, 5f); // Simplified. Try different initial condition
            body[i].GetComponent<MeshRenderer>().enabled = true;

            // + This is just pretty trails
            trailRenderer[i] = body[i].AddComponent<TrailRenderer>();
            // Configure the TrailRenderer's properties
            trailRenderer[i].time = 20.0f;  // Duration of the trail
            trailRenderer[i].startWidth = 0.7f;  // Width of the trail at the start
            trailRenderer[i].endWidth = 0.1f;    // Width of the trail at the end
            // a material to the trail
            trailRenderer[i].material = new Material(Shader.Find("Sprites/Default"));
            // Set the trail color
            Gradient gradient = new Gradient();
            float h = (i / (float)numberOfCaps) % 1f;
            float s = 0.45f;
            float v = 0.98f;
            Color c = Color.HSVToRGB(h, s, v);

            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(c, 0.0f),
                                        new GradientColorKey(c, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            trailRenderer[i].colorGradient = gradient;

        }
    }

    void Update() // Update is called once per frame
    {
        // Draw interactivePoint
        interactivePoint.transform.position = interactPoint;

        // initailize 
        for (int i = 0; i < numberOfCaps; i++)
        {
            bp[i].acceleration = Vector3.zero; 
        }
  
        for (int i = 0; i < numberOfCaps; i++)
        {
            if (currState == MicrobotState.Boids)
            {
                Vector3 separation = SeparationFunc(i);
                bp[i].acceleration += separation;

                Vector3 cohesion = CohesionFunc(i);
                bp[i].acceleration += cohesion;

                Vector3 align = AlignmentFunc(i);
                bp[i].acceleration += align;
            }

            else if (currState == MicrobotState.Structure)
            {
                bp[i].acceleration += CalculateStructure(i);
            }
            
        }

        // (Force) Hesitation: randomly hover the space for natural behavior  
        for (int i = 0; i < numberOfCaps; i++)
        {
            float randomScale = 10f;
            if (Random.Range(0f, 1.05f) > 1f)
            {
                bp[i].acceleration += new Vector3(randomScale * Random.Range(-1f, 1f), randomScale * Random.Range(-1f, 1f), randomScale * Random.Range(-1f, 1f));
            }
        }


        // (Force) Interactive Acceleration : reacts to the actuation of the interactive point
        if (mp == null)
        {
            mp = FindObjectOfType<MediaPipeBodyTracker>();
            if (mp == null)
            {
                return;
            }
        }

        Vector3 rightHandOffset = new Vector3(100f, 100f, 180f);
        interactPoint = -mp.RightHandPosition * 200f + rightHandOffset;
        interactivePoint.transform.position = interactPoint;

        Vector3 leftHandOffset = new Vector3(-100f, 100f, 180f);
        Vector3 structurePoint = -mp.LeftHandPosition * 200f  + leftHandOffset;

        float actuation = 1f + (previousInteractivePoint - interactPoint).sqrMagnitude;
       
        if (mp.RightHandPinch)
        {
            for (int i = 0; i < numberOfCaps; i++)
            {
                Vector3 dist = interactPoint - body[i].transform.position;
                Vector3 attract = dist.normalized * interactiveMass;

                bp[i].acceleration += attract;
                trailRenderer[i].time = 20f;
                Gradient gradient = new Gradient();
                float h = (i / (float)numberOfCaps) % 1f;
                float s = 0.45f + bp[i].acceleration.sqrMagnitude / 1000f;
                float v = 0.98f + bp[i].acceleration.sqrMagnitude / 1000f;
                Color c = Color.HSVToRGB(h, s, v);

                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(c, 0.0f),
                                            new GradientColorKey(c, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
                );
                trailRenderer[i].colorGradient = gradient;
            }
            previousInteractivePoint = interactPoint;
            
        }

        if (mp.LeftHandPinch && !builtStructure)
        {
            Vector3 center = Vector3.zero;
            
            for (int i = 0; i < numberOfCaps; i++)
            {
                center += body[i].transform.position;
                trailRenderer[i].time += 100f;

                Gradient gradient = new Gradient();
                float h = 0f;
                float s = 0f;
                float v = 0.32f;
                Color c = Color.HSVToRGB(h, s, v);

                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(c, 0.0f),
                                            new GradientColorKey(c, 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
                );
                trailRenderer[i].colorGradient = gradient;

            }

            center /= numberOfCaps;
            
            MakeStructure(center);
            builtStructure = true;
            currState = MicrobotState.Structure;

            for (int i = 0; i < numberOfCaps; i++)
            {
                bp[i].velocity = Vector3.zero;
            }
        }

        else if (mp.RightHandPinch && builtStructure) 
        {
            currState = MicrobotState.Boids;
            builtStructure = false;
        }

        if (currState == MicrobotState.Structure && !builtStructure)
        {
            int count = 0;

            for (int i = 0; i < numberOfCaps; i++)
            {
                if (!inPlace[i])
                {
                    float dist = (body[i].transform.position - structures[i]).magnitude;
                    if (dist < 1.0f)
                    {
                        inPlace[i] = true;
                        bp[i].velocity = Vector3.zero;
                    }
                }

                if (!inPlace[i])
                {
                    count++;
                }
            }
            
            if (count == numberOfCaps)
            {
                builtStructure = true;
                
                for (int i = 0; i < numberOfCaps; i++)
                {
                    body[i].transform.localScale = Vector3.one * 2f;
                    body[i].GetComponent<MeshRenderer>().enabled = true;
                    Material redColor = new Material(Shader.Find("Sprites/Default"));
                    redColor.color = Color.HSVToRGB(0f, 1f, 1f);
                    body[i].GetComponent<Renderer>().material = redColor;
                }
            }       
            
        }

        // Apply acceleration to velocity, to position
        for (int i = 0; i < numberOfCaps; i++)
        {
            if (currState != MicrobotState.Structure || !builtStructure)
            {
                bp[i].velocity += bp[i].acceleration * Time.deltaTime * fastforwardConst;
                body[i].transform.position += bp[i].velocity * Time.deltaTime * fastforwardConst;
                body[i].transform.LookAt(body[i].transform.position + bp[i].velocity);
                
                if (bp[i].velocity.magnitude > maxVelocity)
                {
                    bp[i].velocity = maxVelocity * bp[i].velocity.normalized;
                }
            }

        }

        // Color update
        //{
        //    for (int i = 0; i < numberOfCaps; i++)
        //    {
        //        // + This is just pretty trails
        //        Gradient gradient = new Gradient();
        //        float h = (i / (float)numberOfCaps) % 1f;
        //        float s = 0.45f + bp[i].acceleration.sqrMagnitude / 1000f;
        //        float v = 0.98f + bp[i].acceleration.sqrMagnitude / 1000f;
        //        Color c = Color.HSVToRGB(h, s, v);

        //        gradient.SetKeys(
        //            new GradientColorKey[] { new GradientColorKey(c, 0.0f),
        //                                    new GradientColorKey(c, 1f) },
        //            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        //        );
        //        trailRenderer[i].colorGradient = gradient;
        //    }
        //}

        frameCount++;
    }

    private Vector3 SeparationFunc(int curr)
    {
        Vector3 separation = Vector3.zero;
        int close = 0;
        Vector3 pos = body[curr].transform.position;

        for (int i = 0; i < numberOfCaps; i++)
        {
            if (i != curr)
            {
                Vector3 distance = pos - body[i].transform.position;
                float dist = distance.magnitude;

                if (dist < nearBy)
                {
                    separation += distance.normalized / dist;
                    close++;
                }
            }
        }

        if (close > 0)
        {
            separation /= close;
        }

        return separation.normalized * separateConst;
    }

    private Vector3 CohesionFunc(int curr)
    {
        Vector3 massPoint = Vector3.zero;
        int close = 0;
        Vector3 pos = body[curr].transform.position;

        for (int i = 0; i < numberOfCaps; i++)
        {
            if (i != curr)
            {
                float dist = (pos - body[i].transform.position).magnitude;
                
                if (dist < nearBy)
                {
                    massPoint += body[i].transform.position;
                    close++;
                }
            }
        }

        if (close > 0)
        {
            massPoint /= close;
            Vector3 cohesion = massPoint - pos;
            return cohesion.normalized * cohesConst;
        }

        return Vector3.zero;
    }

    private Vector3 AlignmentFunc(int curr)
    {
        Vector3 avgVel = Vector3.zero;
        int close = 0;
        Vector3 pos = body[curr].transform.position;

        for (int i = 0; i < numberOfCaps; i++)
        {
            if (i != curr)
            {
                float dist = (pos - body[i].transform.position).magnitude;
                
                if (dist < nearBy)
                {
                    avgVel += bp[i].velocity;
                    close++;
                }
            }
        }

        if (close > 0)
        {
            avgVel /= close;
            return (avgVel - bp[curr].velocity).normalized;
        }

        return Vector3.zero;
    }

    //honestly just couldnt make this work the way I wanted to
    private void MakeStructure(Vector3 point)
    {
        inPlace = new bool[numberOfCaps];
        structures = new Vector3[numberOfCaps];

        int dimension = Mathf.CeilToInt(numberOfCaps / 3f);
        float spacing = 3f;

        int index = 0;
        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension; j++)
            {
                for (int k = 0; k < dimension; k++)
                {
                    bool surface = i == 0 || i == dimension - 1 ||
                        j == 0 || j == dimension - 1 ||
                        k == 0 || k == dimension - 1;
                    if (surface && index < numberOfCaps)
                    {
                        Vector3 offset = new Vector3((i - (dimension - 1) / 2f) * spacing,
                            (j - (dimension - 1) / 2f) * spacing,
                            (k - (dimension - 1) / 2f) * spacing);

                        structures[index] = point + offset;
                        index++;
                    }
                }
            }
        }
    }

    private Vector3 CalculateStructure(int curr)
    {
        if (structures == null || curr >= structures.Length)
        {
            return Vector3.zero;
        }

        Vector3 targetPos = structures[curr];
        Vector3 currPos = body[curr].transform.position;
        Vector3 toTarg = targetPos - currPos; 
        float dist = toTarg.magnitude;

        float strength = Mathf.Clamp(dist * 0.5f, 0.5f, maxVelocity * 2f);
        return toTarg.normalized * strength;
    }

}
