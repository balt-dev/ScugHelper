texture text: register(t0);
sampler textSampler: register(s0);

uniform float4x4 World;
uniform float2 TexelSize;

float4 SpritePixelShader(float2 uv : TEXCOORD0, float4 vertColor : COLOR0) : COLOR {
    float4 center = tex2D(textSampler, uv);
    
    if (center.a >= 0.01) return center * vertColor;

    float up    = tex2D(textSampler, uv + float2(0, TexelSize.y)).a;
    float down  = tex2D(textSampler, uv - float2(0, TexelSize.y)).a;
    float left  = tex2D(textSampler, uv  - float2(TexelSize.x, 0)).a;
    float right = tex2D(textSampler, uv + float2(TexelSize.x, 0)).a;

    return float4(0.0, 0.0, 0.0, max(max(up, down), max(left, right)));
}

technique Shader
{
    pass pass0
    {
        PixelShader = compile ps_3_0 SpritePixelShader();
    }
}
