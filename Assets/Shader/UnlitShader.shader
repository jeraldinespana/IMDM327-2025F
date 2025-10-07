// Unlit shader that bends vertices and distorts UVs over time.
// author : Myungin Lee
Shader "IMDM327/UnlitShader"
{
    Properties
    {
        // Texture the shader will sample from.
        _MainTex ("Texture", 2D) = "white" {}
    }
    // Everything that defines how the shader is rendered lives inside SubShader.
    SubShader
    {
        // Basic setup so Unity knows when to render this shader.
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // Allow Unity's built-in fog to work.
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                // Position of each vertex in object space.
                float4 vertex : POSITION;
                // UV coordinate coming from the mesh.
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                // UV passed to the fragment shader.
                float2 uv : TEXCOORD0;
                // Stores fog data so the fragment shader can fade into the fog.
                UNITY_FOG_COORDS(1)
                // Clip space position so the GPU knows where to draw on screen.
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            // Unity packs tiling and offset into this helper variable.
            float4 _MainTex_ST;

            // Vertex shader: runs once per vertex.
            v2f vert (appdata v)
            {
                v2f o;
                // Gently move the vertex up and down using a sine wave.
                v.vertex.y += sin(v.vertex.x + _Time.y) * 0.6;
                // Convert from object space to clip space so the GPU can draw it.
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Apply texture tiling/offset from the material.
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            // Fragment shader: runs for every pixel that will be drawn.
            fixed4 frag (v2f i) : SV_Target
            {
                // Center the UVs so 0 is the middle of the screen.
                float2 uv = i.uv - 0.5; 
                float a = _Time.y;
                // Move the distortion point in a circular path.
                float2 p = float2(sin(a), cos(a)) * 0.5;
                // Calculate how far the current pixel is from the moving point.
                float2 distort = uv-p;
                float d = length(distort);
                float m = smoothstep(.2, .02, d);
                // Stretch the UVs based on the distance to create a wobble.
                distort = distort*1*m;
                // Sample the texture using the wobbly UVs.
                fixed4 col = tex2D(_MainTex, i.uv + distort);
                
                return col;
            }
            ENDCG
        }
    }
}
