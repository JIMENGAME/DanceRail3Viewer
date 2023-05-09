Shader "Custom/Test"
{
    Properties
    {
        _MainColor("主颜色",Color)=(0,0,0,1)
 
        _InnerGlowColor("内发光颜色",Color)=(1,1,1,1)
        _InnerGlowPow("内发光等级",Range(0,5))=2
        _InnerGlowStrength("内发光强度",Range(0,4))=1
 
 
        _HaloColor("外光晕颜色",Color)=(1,1,1,1)
        _HaloArea("外光晕范围",Range(0,2))=1
        _HaloPow("外光晕等级",Range(0,3))=1
        _HaloStrength("外光晕强度",Range(0,4))=1
    }
    SubShader
    {
        //第一个Pass实现内发光,或者说边缘发光
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 _MainColor;
            fixed4 _InnerGlowColor;
            fixed _InnerGlowPow;
            fixed _InnerGlowStrength;
 
            struct a2v
            {
                float4 vertex:POSITION;
                float3 normal:NORMAL;
            };
 
            struct v2f
            {
                float4 pos:SV_POSITION;
                float3 normal:TEXCOORD0;
                float3 worldPos:TEXCOORD1;
            };
 
 
            v2f vert(a2v v)
            {
                v2f o;
 
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
 
                return o;
            }
 
            fixed4 frag(v2f i):SV_Target
            {
                i.normal = normalize(i.normal);
 
                float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos)); normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
                //越边缘，值越大
                float fresnel = pow(1 - saturate(dot(i.normal, viewDir)), _InnerGlowPow) * _InnerGlowStrength;
 
                fixed3 color = lerp(_MainColor, _InnerGlowColor, fresnel);
 
                return fixed4(color, 1);
            }
            ENDCG
        }
        Pass
        {
            //剔除正面，防止外边缘遮住原模型，并不是所有的都需要剔除正面，比如袖子什么的
            //那个需要开启深度模板什么的，暂时还没学到
            Cull Front
            //混合用于控制外边缘强度
            Blend SrcAlpha OneMinusSrcAlpha
            //ZWrite off  这个用于外边缘叠加和其他物体叠加时可能会产生的透明边界的情形
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _HaloColor;
            fixed _HaloPow;
            fixed _HaloArea;
            fixed _HaloStrength;
 
            struct a2v
            {
                float4 vertex:POSITION;
                float3 normal:NORMAL;
            };
 
            struct v2f
            {
                float4 pos:SV_POSITION;
                float3 normal:TEXCOORD0;
                float4 worldPos:TEXCOORD1;
            };
 
 
            v2f vert(a2v v)
            {
                v2f o;
                o.normal = UnityObjectToWorldNormal(v.normal);
                v.vertex.xyz += v.normal * _HaloArea;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }
 
            fixed4 frag(v2f i):SV_Target
            {
                i.normal = normalize(i.normal);
                //和Pass一不一样，原因是我们要把背面当正面渲染，所以视角方向要相反
                float3 viewDir = normalize(i.worldPos.xyz - _WorldSpaceCameraPos.xyz);
                //边缘仍然使用菲涅尔判断，只是和检测方式内边缘不同
                float fresnel = pow(saturate(dot(i.normal, viewDir)), _HaloPow) * _HaloStrength;
                return fixed4(_HaloColor.rgb, fresnel);
            }
            ENDCG
        }
    }
}