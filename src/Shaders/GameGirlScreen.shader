Shader"Futile/GameGirlScreen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Brightness("Verts fill color", Float) = 0
        _Offset("Verts fill color", Float) = 0
        _Contrast("Verts fill color", Float) = 0
        _BayerTex("Texture", 2D) = "white" {}
        _BayPal("Texture", 2D) = "white" {}
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

// Brightness (less than 0 is darker, above 0 is brighter)
uniform float _Brightness;
uniform float _Offset;
uniform float _Contrast;
uniform sampler2D _BayerTex;
uniform sampler2D _BayPal;
uniform float4 _BayPal_TexelSize;

// Colors that we will use
//uniform float4 _Color1 = float4(0.784313725, 0.788235294, 0.262745098, 1);
//uniform float4 _Color2 = float4(0.490196078, 0.521568627, 0.152941176, 1); // sub lumin from color1
//uniform float4 _Color3 = float4(0, 0.415686275, 0, 1); // double lumin from color4
//uniform float4 _Color4 = float4(0.015686275, 0.243137255, 0, 1); 

// Color offset - changes threshold for color adjustments
//uniform float offset = 0.5;

struct v2f
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 scrPos : TEXCOORD1;
    float4 clr : COLOR;
};

float4 _MainTex_ST;

float get_lum(float4 pixcol)
{
    float lum = ((pixcol.r * 0.2126) + (pixcol.g * 0.7152) + (pixcol.b * 0.0722));
    float contrast = 0.45;
    float offset = 0.5;
    lum = (lum - 0.5 + offset) * contrast + 0.5;
    lum = clamp(lum, 0.0, 1.0);
    return lum;
}

// Adapted from https://godotshaders.com/shader/dither-gradient-shader/
// Colorizes the grayscale pixel
float4 colorize(float4 color, float2 uv)
{
	// calculate pixel luminosity (https://stackoverflow.com/questions/596216/formula-to-determine-brightness-of-rgb-color)
    float lum = (color.r * 0.299) + (color.g * 0.587) + (color.b * 0.114);
    //float lum = ((color.r * 0.2126) + (color.g * 0.7152) + (color.b * 0.0722));
	
	// adjust with contrast and offset parameters
    lum = (lum - 0.5 + _Offset) * _Contrast + 0.5;
    lum = clamp(lum, 0.0, 1.0);
    

	
	// reduce luminosity bit depth to give a more banded visual if desired	
    //float bits = float(32);
    //lum = floor(lum * bits) / bits;
	
	// to support multicolour palettes, we want to dither between the two colours on the palette
	// which are adjacent to the current pixel luminosity.
	// to do this, we need to determine which 'band' lum falls into, calculate the upper and lower
	// bound of that band, then later we will use the dither texture to pick either the upper or 
	// lower colour.
	
	// get the palette texture size mapped so it is 1px high (so the x value however many colour bands there are)
    int2 col_size = 1 / _BayPal_TexelSize.xy;
	
    float col_x = float(col_size.x) - 1.0; // colour boundaries is 1 less than the number of colour bands
    float col_texel_size = 1.0 / col_x; // the size of one colour boundary
	
    lum = max(lum - 0.00001, 0.0); // makes sure our floor calculation below behaves when lum == 1.0
    float lum_lower = floor(lum * col_x) * col_texel_size;
    float lum_upper = (floor(lum * col_x) + 1.0) * col_texel_size;
    float lum_scaled = lum * col_x - floor(lum * col_x); // calculates where lum lies between the upper and lower bound
	
	// map the dither texture onto the screen. there are better ways of doing this that makes the dither pattern 'stick'
	// with objects in the 3D world, instead of being mapped onto the screen. see lucas pope's details posts on how he 
	// achieved this in Obra Dinn: https://forums.tigsource.com/index.php?topic=40832.msg1363742#msg1363742
    int2 noise_size = int2(32, 32);
    float2 inv_noise_size = float2(1.0 / float(noise_size.x), 1.0 / float(noise_size.y));
    float2 noise_uv = uv * inv_noise_size * float2(float(_ScreenParams.x), float(_ScreenParams.y));
    float threshold = tex2D(_BayerTex, noise_uv).r;
	
	// adjust the dither slightly so min and max aren't quite at 0.0 and 1.0
	// otherwise we wouldn't get fullly dark and fully light dither patterns at lum 0.0 and 1.0
    threshold = threshold * 0.99 + 0.005;
	
	// the lower lum_scaled is, the fewer pixels will be below the dither threshold, and thus will use the lower bound colour,
	// and vice-versa
    float ramp_val = lum_scaled < threshold ? 0.0f : 1.0f;
	// sample at the lower bound colour if ramp_val is 0.0, upper bound colour if 1.0
    float col_sample = lerp(lum_lower, lum_upper, ramp_val);
    float3 final_col = tex2D(_BayPal, float2(col_sample, 0.5)).rgb;
	
	// return the final colour!
    return float4(final_col.rgb, 1);
}

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
    float4 color = tex2D(_MainTex, uv);
    
    //float average_lum = get_lum(color);
    color = colorize(color, uv);
    
    
    float2 ps = uv * _ScreenParams.xy / i.scrPos.w;
    
    bool changed = false;
    
    int px = (int) ps.x % 9;
    int py = (int) ps.y % 9;
    float4 white = float4(1, 1, 1, 1);
    float4 black = float4(0, 0, 0, 1);
    
    if (px == 8 && _Brightness > 0.0)
    {
        color = lerp(color, white, _Brightness);
        changed = true;
    }
    else if (!changed && px == 8 && _Brightness < 0.0)
    {
        color = lerp(black, color, 1 + _Brightness);
        changed = true;
    }
    
    if (!changed && py == 8 && _Brightness > 0.0)
    {
        color = lerp(color, white, _Brightness);
        changed = true;
    }
    else if (!changed && py == 8 && _Brightness < 0.0)
    {
        color = lerp(black, color, 1 + _Brightness);
        changed = true;
    }
    
    return color;
}
            ENDCG
        }
    }
}
