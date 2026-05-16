#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

DECLARE_TEXTURE(text, 0);

uniform float4x4 World;
uniform float2 TexelSize;

float4 SpritePixelShader(float2 uv : TEXCOORD0, float4 vertColor : COLOR0) : COLOR

    float4 center = SAMPLE_TEXTURE(text, uv);

    float4 up    = SAMPLE_TEXTURE(text, uv + float2(0, TexelSize.y));
    float4 down  = SAMPLE_TEXTURE(text, uv - float2(0, TexelSize.y));
    float4 left  = SAMPLE_TEXTURE(text, uv - float2(TexelSize.x, 0));
    float4 right = SAMPLE_TEXTURE(text, uv + float2(TexelSize.x, 0));

    return (center.a <= 0.0 && (up.a > 0.0 || down.a > 0.0 || left.a > 0.0 || right.a > 0.0))
        ? vertColor
        : float4(0.0, 0.0, 0.0, 0.0);
}

void SpriteVertexShader(inout float4 color    : COLOR0,
                        inout float2 texCoord : TEXCOORD0,
                        inout float4 position : SV_Position)
{
    position = mul(position, World);
}

technique Shader
{
    pass pass0
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 SpritePixelShader();
    }
}
