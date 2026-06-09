texture text: register(t0);
sampler textSampler: register(s0);

uniform float4x4 World;
uniform float2 TexelSize;

float4 SpritePixelShader(float2 uv : TEXCOORD0, float4 vertColor : COLOR0) : COLOR {
    float4 center = tex2D(textSampler, uv);

    float4 up    = tex2D(textSampler, uv + float2(0, TexelSize.y));
    float4 down  = tex2D(textSampler, uv - float2(0, TexelSize.y));
    float4 left  = tex2D(textSampler, uv  - float2(TexelSize.x, 0));
    float4 right = tex2D(textSampler, uv + float2(TexelSize.x, 0));

    return (center.a <= 0.0 && (up.a > 0.0 || down.a > 0.0 || left.a > 0.0 || right.a > 0.0))
        ? vertColor
        : float4(0.0, 0.0, 0.0, 0.0);
}

technique Shader
{
    pass pass0
    {
        PixelShader = compile ps_3_0 SpritePixelShader();
    }
}
