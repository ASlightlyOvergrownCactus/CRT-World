Shader"Futile/CRTScreen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VertsColor("Verts fill color", Float) = 0
        _VertsColor2("Verts fill color 2", Float) = 0
        _VertsColor3("Verts fill color 3", Float) = 0
        _ScreenDist("Bool Screen Distortion", Float) = 1
        _Smear("VCR Smear", Float) = 0
        _Wiggle("VCR Wiggle Intensity", Float) = 0
        _BlurSamples("VCR Blur Sample Count", Int) = 15
        _VCRBlur("Bool VCR Blur", Float) = 1
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
// VHS Section is based off of this godot shader: https://godotshaders.com/shader/vhs/

sampler2D _MainTex;
sampler2D _NoiseTex;
sampler2D _PalTex;
sampler2D _LevelTex;
sampler2D _GrabTexture : register(s0);

uniform float _VertsColor;
uniform float _VertsColor2;
uniform float _VertsColor3;
uniform float _ScreenDist;

uniform float _Wiggle;
uniform int _BlurSamples;
uniform float _Smear;
uniform float _VCRBlur;

float mod(float x, float y)
{
    return (x - y * floor(x / y));
}

float onOff(float a, float b, float c, float framecount)
{
    return step(c, sin((framecount * 0.001) + a * cos((framecount * 0.001) * b)));
}

float2 jumpy(float2 uv, float framecount)
{
    float2 look = uv;
    float window = 1.0 / (1.0 + 80.0 * (look.y - mod(framecount / 4.0, 1.0)) * (look.y - mod(framecount / 4.0, 1.0)));
    look.x += 0.05 * sin(look.y * 10.0 + framecount) / 20.0 * onOff(4.0, 4.0, 0.3, framecount) * (0.5 + cos(framecount * 20.0)) * window;
    float vShift = (0.1 * _Wiggle) * 0.4 * onOff(2.0, 3.0, 0.9, framecount) * (sin(framecount) * sin(framecount * 20.0) + (0.5 + 0.1 * sin(framecount * 200.0) * cos(framecount)));
    look.y = mod(look.y - 0.01 * vShift, 1.0);
    return look;
}

float2 Circle(float Start, float Points, float Point)
{
    float Rad = (3.141592 * 2.0 * (1.0 / Points)) * (Point + Start);
    return float2(-(.3 + Rad), cos(Rad));
}

float3 rgb2yiq(float3 c)
{
    return float3(
        (0.2989 * c.x + 0.5959 * c.y + 0.2115 * c.z),
        (0.5870 * c.x - 0.2744 * c.y - 0.5229 * c.z),
        (0.1140 * c.x - 0.3216 * c.y + 0.3114 * c.z)
    );
}

float3 yiq2rgb(float3 c)
{
    return float3(
        (1.0 * c.x + 1.0 * c.y + 1.0 * c.z),
        (0.956 * c.x - 0.2720 * c.y - 1.1060 * c.z),
        (0.6210 * c.x - 0.6474 * c.y + 1.7046 * c.z)
    );
}

float3 Blur(float2 uv, float d, int samples)
{
    float3 sum = float3(0.0, 0.0, 0.0);
    float t = (sin(_Time.y * 5.0 + uv.y * 5.0)) / 10.0;
    float b = 1.0;
    t = 0.0;
    float2 PixelOffset = float2(d + .0005 * t, 0);
    
    float Start = 2.0 / 14.0;
    float2 Scale = 0.66 * 4.0 * 2.0 * PixelOffset.xy;
    float W = 1.0 / 15.0;
    
    for (int i = 0; i < 64; i++)
    {
        if (i >= samples)
            break;
        float3 N = tex2D(_MainTex, uv + Circle(Start, float(samples) - 1.0, float(i)) * Scale).rgb;
        sum += (N * W);
    }
    
    return sum * b;
}

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
    
    if (_ScreenDist == 1.0)
    {
        float radius = _VertsColor3;
        float warp = length(float3(uv - 0.5, radius)) / length(float2(0.5, radius));
        warp += 0.05;
        uv = (uv - 0.5) * warp + 0.5;
    }

    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
        return float4(0, 0, 0, 1);
    
    float4 final = float4(1.0, 1.0, 1.0, 1.0);
    float4 color;
    
    if (_VCRBlur == 1.0)
    {
        float wiggle_speed = 25.0;
    
        float d = 0.1 - round(mod(_Time.y / 3.0, 1.0)) * .1;;
        uv = jumpy(uv, mod(_Time.y * wiggle_speed, 7.0));

        float s = 0.0001 * -d + 0.0001 * _Wiggle * (sin(_Time.y * wiggle_speed));
        float e = min(.30, pow(max(0.0, cos(uv.y * 4.0 + .3) - .75) * (s + 0.5) * 1.0, 3.0)) * 25.0;
        float r = (_Time.y * (2.0 * s));
        uv.x += abs(r * pow(min(.003, (-uv.y + (.01 * mod(_Time.y, 5.0)))) * 3.0, 2.0)) * _Wiggle;
    
        d = 0.051 + abs(sin(s / 4.0));
        float c = max(0.0001, .002 * d) * _Smear;

        final.rgb = Blur(uv, c + c * uv.x, _BlurSamples);
        float y = rgb2yiq(final.rgb).r;

        uv.x += 0.01 * d;
        c *= 6.0;
        final.rgb = Blur(uv, c, _BlurSamples);
        float m = rgb2yiq(final.rgb).g;

        uv.x += 0.005 * d;
        c *= 2.50;
        final.rgb = Blur(uv, c, _BlurSamples);
        float q = rgb2yiq(final.rgb).b;
        final.rgb = yiq2rgb(float3(y, m, q)) - pow(s + e * 2.0, 3.0);

        final.a = 1.0;
        color = final;
    }
    else
    {
        color = tex2D(_MainTex, uv);
    }
    
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
