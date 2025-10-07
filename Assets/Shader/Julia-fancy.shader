// Julia Fractal Shader

Shader "IMDM327/Julia-fancy"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MaxIterations ("Max Iterations", Range(1, 1024)) = 256
        _Zoom ("Zoom", Float) = 1.0
        _Offset ("Offset (X,Y)", Vector) = (0, 0, 0, 0)
        _JuliaConstant ("Julia Constant (c)", Vector) = (0.355, 0.355, 0, 0)
        _Rotation ("Plane Rotation (Radians)", Float) = 0
        _InsideColor ("Inside Color", Color) = (0, 0, 0, 1)
        _OutsideColor ("Outside Color", Color) = (0.07, 0.28, 0.75, 1)
        _AccentColor ("Accent Color", Color) = (0.95, 0.85, 0.35, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        int _MaxIterations;
        float _Zoom;
        float4 _Offset;
        float4 _JuliaConstant;
        float _Rotation;
        fixed4 _InsideColor;
        fixed4 _OutsideColor;
        fixed4 _AccentColor;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float3 Palette(float t)
        {
            t = saturate(t);
            float accentBlend = smoothstep(0.0, 1.0, sqrt(t));
            float insideBlend = smoothstep(0.25, 1.0, t);

            float3 baseColor = lerp(_OutsideColor.rgb, _AccentColor.rgb, accentBlend);
            return lerp(baseColor, _InsideColor.rgb, insideBlend);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float aspect = _ScreenParams.x / _ScreenParams.y;
            float2 centeredUV = IN.uv_MainTex - 0.5;
            centeredUV.x *= aspect;

            float s = sin(_Rotation);
            float c = cos(_Rotation);
            float2 rotatedUV = float2(
                c * centeredUV.x - s * centeredUV.y,
                s * centeredUV.x + c * centeredUV.y
            );

            float zoom = max(_Zoom, 1e-4);
            float2 z = rotatedUV / zoom + _Offset.xy;
            float2 constant = _JuliaConstant.xy;

            int iter;
            const float escapeRadius = 4.0;

            for (iter = 0; iter < _MaxIterations; ++iter)
            {
                float zx = z.x;
                float zy = z.y;
                z = float2(zx * zx - zy * zy, 2.0 * zx * zy) + constant;

                if (dot(z, z) > escapeRadius)
                {
                    break;
                }
            }

            float iterSmooth = iter;
            if (iter < _MaxIterations)
            {
                float log_zn = log(dot(z, z)) * 0.5;
                float nu = log(log_zn / log(2.0)) / log(2.0);
                iterSmooth = max(0.0, iter + 1 - nu);
            }

            float t = saturate(iterSmooth / _MaxIterations);
            float3 color = Palette(t);

            o.Albedo = color;
            o.Emission = color;
            o.Metallic = 0;
            o.Smoothness = 0;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
