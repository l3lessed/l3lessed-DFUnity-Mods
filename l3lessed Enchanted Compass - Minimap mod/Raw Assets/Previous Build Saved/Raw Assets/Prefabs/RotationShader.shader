// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Instanced/Unlit"
{
    Properties
    {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Rotation("Rotation Total", Float) = 0
        _LineLength ("Line Length", Float) = 1.33
        _Spacing ("Spacing", Float) = 0
    }

    SubShader
    {

        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
                    
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #pragma multi_compile_instancing


            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            struct Input {
                float2 uv_MainTex;
            };

            float _Rotation;
            half _LineLength;
            half _Spacing;

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color) // Make _Color an instanced property (i.e. an array)
            #define _Color_arr Props
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                v.uv.xy -= 0.5;
                float s = sin(_Rotation);
                float c = cos(_Rotation);
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                rotationMatrix *= 0.5;
                rotationMatrix += 0.5;
                rotationMatrix = rotationMatrix * 2 - 1;
                o.uv.xy = mul(v.uv.xy*_LineLength, rotationMatrix);
                o.uv.xy += 0.5;
                return o;               
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                i.uv.xy = saturate(i.uv.xy * (1+_Spacing));
                //discard border pixels to stop clamping artifacts.
                if(i.uv.x < .01 || i.uv.x > .99)
                    discard;
                if(i.uv.y < .01 || i.uv.y > .99)
                    discard;
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
        ENDCG
    }
}
    }