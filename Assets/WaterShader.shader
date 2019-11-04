

Shader "Custom/WaterShader"
{

    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Height("Height", float) = 1
        _Scale("Scale", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard forwardBased 
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            INTERNAL_DATA
            float3 worldPos;
        };
        float _Height;
        float _Scale;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _Color2;

        UNITY_INSTANCING_BUFFER_START(Props)

        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input input, inout SurfaceOutputStandard o)
        {
            float3 localPos = input.worldPos - mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
            float height = localPos.y / _Height;
            // Albedo comes from a texture tinted by color
            //fixed4 c = tex2D (_MainTex, input.uv_MainTex) * _Color;
            fixed4 c = lerp(_Color, _Color2, height * height);
            c.a = 1.0f;
            o.Albedo = c.rgb;
            o.Albedo *= tex2D(_MainTex, input.uv_MainTex).rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
