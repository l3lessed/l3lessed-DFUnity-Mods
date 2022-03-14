// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

 Shader "Custom/linerender" {
     Properties
     {
         _MainTex ("Sprite Texture", 2D) = "white" {}
         _Color ("Tint", Color) = (1,1,1,1)
         _LineLength ("Line Length", Float) = 1
         _Spacing ("Spacing", Float) = 0.25
     }
 
     CGINCLUDE
     #include "UnityCG.cginc"
     struct appdata_t
     {
         float4 vertex   : POSITION;
         float4 color    : COLOR;
         float2 texcoord : TEXCOORD0;
     };
  
     struct v2f
     {
         float4 vertex   : SV_POSITION;
         fixed4 color    : COLOR;
         half2 texcoord  : TEXCOORD0;
     };
            
     fixed4 _Color;
     half _LineLength;
     half _Spacing;
  
     v2f vert(appdata_t IN)
     {
         v2f OUT;
         OUT.vertex = UnityObjectToClipPos(IN.vertex);
         OUT.texcoord = half2(IN.texcoord.x*_LineLength,IN.texcoord.y);
         OUT.color = IN.color * _Color; 
         return OUT;
     }
  
     sampler2D _MainTex;
 
     fixed4 frag(v2f IN) : COLOR
     {
         half2 uv = frac(IN.texcoord);
         uv.x = saturate(uv.x * (1+_Spacing));
         half4 tex_col = tex2D (_MainTex, uv);   
         return tex_col * IN.color;
     }
     ENDCG
  
     SubShader
     {
         Tags
         {
             "Queue"="Transparent"
             "IgnoreProjector"="True"
             "RenderType"="Transparent"
             "PreviewType"="Plane"
             "CanUseSpriteAtlas"="True"
         }
  
         Cull Off
         Lighting Off
         ZWrite Off
         Fog { Mode Off }
         Blend SrcAlpha OneMinusSrcAlpha
  
         Pass
         {
         CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
         ENDCG
         }
     }
     Fallback "Diffuse"
 }