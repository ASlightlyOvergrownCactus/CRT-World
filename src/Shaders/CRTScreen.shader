Shader"Futile/CRTScreen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VertsColor("Verts fill color", Float) = 0
        _VertsColor2("Verts fill color 2", Float) = 0
        _VertsColor3("Verts fill color 3", Float) = 0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
Blend SrcAlpha

OneMinusSrcAlpha
Fog
{
    Color(0, 0, 0, 0)

}

Cull Off // Makes it a full shader (not half of one)

Lighting Off

ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

#include "UnityCG.cginc"

sampler2D _MainTex;
sampler2D _NoiseTex;
sampler2D _PalTex;
sampler2D _LevelTex;
sampler2D _GrabTexture : register(s0);

uniform float _VertsColor;
uniform float _VertsColor2;
uniform float _VertsColor3;

struct v2f
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 scrPos : TEXCOORD1;
    float4 clr : COLOR;
};

float4 _MainTex_ST;

v2f vert(appdata_full v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
    o.scrPos = ComputeScreenPos(o.pos);
    o.clr = v.color;
    return o;
}

half4 frag(v2f i) : SV_Target
{
    float2 uv = i.uv;
    float radius = _VertsColor3;
    float warp = length(float3(uv - 0.5, radius)) / length(float2(0.5, radius));
    warp += 0.05;
    uv = (uv - 0.5) * warp + 0.5;
    
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
        return float4(0, 0, 0, 0);
   
    float4 color = tex2D(_MainTex, uv);
    
    float2 ps = uv * _ScreenParams.xy / i.scrPos.w;
    
    int pp = (int) ps.x % 3;
    float3 muls = float3(1, 1, 1);
    
    if ((int) ps.y % 3 == 0)
        muls *= float4(_VertsColor, _VertsColor, _VertsColor, 1);
    
    if (pp == 1)
    {
        muls.r = 1;
        muls.g = _VertsColor2;
    }
    else if (pp == 2)
    {
        muls.g = 1;
        muls.b = _VertsColor2;
    }
    else
    {
        muls.b = 1;
        muls.r = _VertsColor2;
    }
    float4 outColor = float4(0, 0, 0, 1);
    outColor.rgb = color.rgb * muls;
    return outColor;
}
            ENDCG
        }
    }
}
