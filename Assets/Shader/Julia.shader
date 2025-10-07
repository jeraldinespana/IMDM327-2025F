//Mandelbrot and Julia Fractal Shader
//Author: Jamie Ngoc Dinh, Myungin Lee, 2024

Shader "IMDM327/Julia"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MaxIterations ("Max Iterations", Float) = 255
        _Zoom ("Zoom Level", Float) = 1.0
        _Offset ("Offset", Vector) = (0, 0, 0, 0)
        _JuliaConstant ("Julia Constant", Vector) = (0.355, 0.355, 0, 0)
        _Red("Red", range(0,10)) = .5
        _Green("Green", range(0,10)) = .5
        _Blue("Blue", range(0,10)) = .5
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

        float _MaxIterations;
        float _Zoom;
        float4 _Offset;
        // float2 _JuliaConstant;
        float _Red, _Green, _Blue;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            //Julia Set
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

            float2 z = (IN.uv_MainTex / _Zoom) + _Offset.xy;
            float2 c = _JuliaConstant.xy;

            // float2 c = (IN.uv_MainTex / _Zoom) + _Offset.xy;
            // float2 z;
            int iter;

            for (iter = 0; iter < _MaxIterations; iter++) {
                z = float2(z.x * z.x - z.y * z.y, 2 * z.x * z.y) + c;
                if (length(z) > 2) break; 
            }

            float4 color;
            if (iter == _MaxIterations) {
                color = float4(0,0,0,1);
            } else {
                color = float4(sin(iter / _Red), sin(iter / _Green), sin(iter / _Blue), 1) / 4 + 0.75;
            }

            // o.Albedo = color;

            o.Albedo = fixed4(color * 0.7, color * 0.5, sin(color * 6.2831), 1.0);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
