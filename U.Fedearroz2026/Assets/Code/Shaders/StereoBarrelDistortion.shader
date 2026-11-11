Shader "FedeArroz/StereoBarrelDistortion"
{
    Properties
    {
        _Strength ("Fuerza (+ centro gordo / - bordes)", Float) = 0.30
        _Curvature ("Curvatura extra", Float) = 0.10
        _Zoom ("Zoom (menor = menos borde negro)", Float) = 0.70
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "BarrelDistortion"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Strength;
            float _Curvature;
            float _Zoom;

            float4 Frag(Varyings input) : SV_Target
            {
                float2 p = input.texcoord * 2.0 - 1.0;
                float r2 = dot(p, p);
                float f = 1.0 + _Strength * r2 + _Curvature * r2 * r2;
                float2 suv = p * f * _Zoom;
                suv = suv * 0.5 + 0.5;

                if (suv.x < 0.0 || suv.x > 1.0 || suv.y < 0.0 || suv.y > 1.0)
                    return float4(0.0, 0.0, 0.0, 1.0);

                return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, suv, 0.0);
            }
            ENDHLSL
        }
    }
}
