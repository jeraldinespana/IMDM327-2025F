Shader "Unlit/Raymarch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define MAX_STEPS 100
            #define MAX_DIST 100
            #define SURF_DIST 0.001

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ro : TEXCOORD1;
                float3 hitPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.ro = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1)); 
                o.hitPos = v.vertex; // object space
                return o;
            }

            //signed distance function 
            float GetDist(float3 p) 
            {
                //the scene, here is the sphere
                //float d = length(p) - 0.5; //length(p) is distance from origin to p
                //return d;
                
                //torus
                //float2 t = float2(0.5, 0.1);
                //float2 q = float2(length(p.xz) - t.x, p.y);
                //return length(q)-t.y;

                //hollow sphere
                float r = 0.5; //radius of outer sphere
                float h = 0.2; //inner sphere
                float t = 0.05; //thickness

                float w = sqrt(r*r - h*h);

                float2 q = float2(length(p.xz), p.y);
                return ((h*q.x < w*q.y) ? length(q-float2(w,h)) : abs(length(q)-r)) - t;
            }
            
            float Raymarch(float3 ro, float3 rd) 
            {
                //starting position of raymarch
                float d0 = 0;
                float dS; //distance from the scene or the surface
                for (int i = 0; i<MAX_STEPS; i++) {
                    float3 p = ro + d0 * rd; //calculate the raymarching position
                    dS = GetDist(p); //distance from point p to the scene
                    d0 += dS;
                    if (dS < SURF_DIST || d0 > MAX_DIST) break; //check if hit the scene or pass the scene
                }

                return d0;
            }

            float3 GetNormal(float3 p) {
                float2 e = float2(0.001, 0);
                float3 n = GetDist(p) - float3(
                    GetDist(p-e.xyy),
                    GetDist(p-e.yxy),
                    GetDist(p-e.yyx)
                    );
                return normalize(n);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv - 0.1;

                //ray origin
                float3 ro = i.ro;
                //ray direction
                float3 rd = normalize(i.hitPos - ro);

                float d = Raymarch(ro, rd);
                fixed4 col = 0;

                if (d < MAX_DIST) {
                    float3 p = ro + rd * d;
                    float3 n = GetNormal(p);

                    //normal color
                    // col.rgb = n;

                    //add shading with a light source
                    float3 lightDir = normalize(float3(1, 1, -1));
                    float diff = max(dot(n, lightDir), 0.0);
                    col.rgb = diff * float3(1.0, 0.5, 0.2);
                } 
                else
                    discard;

                return col;
            }
            ENDCG
        }
    }
}
