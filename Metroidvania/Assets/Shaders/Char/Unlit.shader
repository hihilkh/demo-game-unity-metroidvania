﻿Shader "Char/Unlit" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _alpha ("Alpha", Range(0, 1)) = 1
    }

    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Stencil {
             Ref 1
             Comp Equal
             Pass DecrSat
             Fail DecrSat
        }

        Pass {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _alpha;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos (v.vertex);
                o.uv = TRANSFORM_TEX (v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed3 col = tex2D (_MainTex, i.uv);
                return fixed4 (col, _alpha);
            }
            ENDCG
        }
    }
}