
Shader "Arcaea/TransverseChromaticAberration"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TextureBounds ("Texture Bounds", Vector) = (0, 0, 1, 1) // Viewpoint Rect
        _MinEffectIntensity ("Min Effect Intensity", range(1, 15)) = 6
        _MaxEffectIntensity ("Max Effect Intensity", range(1, 15)) = 6 // Range between 6.0(on) and 15.0(off)
        _OverallEffectStrength ("Overrall Effect Strength", range(0, 1)) = 0
        _SampleCount ("Sample Count", int) = 12
    }
    SubShader
    {
        
		Tags { "Queue" = "Transparent"  "RenderType"="Transparent" "CanUseSpriteAtlas"="true"  }
        Cull Off 
        ZWrite Off 
        ZTest Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
            
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION; 
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
            half4 _MainTex_ST;
            
            half4 _TextureBounds;

            half _MinEffectIntensity;
            half _MaxEffectIntensity;

            half _OverallEffectStrength;
            int _SampleCount;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			half4 frag(v2f i) : SV_Target
			{
                half2 uv = i.uv;

                half effectIntensity = lerp(_MinEffectIntensity, _MaxEffectIntensity, _OverallEffectStrength);
                half2 xOnlyTextureCord = half2(uv.x, uv.y * 0.667);
                half2 direction = -(normalize(xOnlyTextureCord - 0.5));

                half2 velocity = half2(direction * 1.75 * pow(length(xOnlyTextureCord - 0.5), effectIntensity));
                float inverseSampleCount = 1.0 / float(_SampleCount);

                half3x3 increments;
                increments[0] = half3(velocity * 1.0 * inverseSampleCount, 0);
                increments[1] = half3(velocity * 2.0 * inverseSampleCount, 0);
                increments[2] = half3(velocity * 6.0 * inverseSampleCount, 0);

                half3 accumulator = half3(0,0,0);
                half3x3 offsets;
                
                for (int i = 0; i < _SampleCount; i++) 
                {
                    accumulator.r += tex2D(_MainTex, clamp(uv + offsets[0].xy, _TextureBounds.xy, _TextureBounds.zw)).r;
                    accumulator.g += tex2D(_MainTex, clamp(uv + offsets[1].xy, _TextureBounds.xy, _TextureBounds.zw)).g;
                    accumulator.b += tex2D(_MainTex, clamp(uv + offsets[2].xy, _TextureBounds.xy, _TextureBounds.zw)).b;
                    
                    offsets[0] -= increments[0];
                    offsets[1] -= increments[1];
                    offsets[2] -= increments[2];
                }

                return half4(accumulator / float(_SampleCount), 1.0);
			}

			ENDCG
		}
    }
}