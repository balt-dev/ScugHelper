texture src: register(t0);
sampler srcS: register(s0);

uniform float4x4 World;
uniform float2 BufferSize;
uniform float AlphaCoefficient;

float4 Blank(float2 uv : TEXCOORD0, float4 vertColor : COLOR0) : COLOR {
    float4 sample = tex2D(srcS, uv);
    return float4(vertColor.rgb * lerp(1, sample.a, AlphaCoefficient), vertColor.a * sample.a);
}

technique Shader {
    pass pass0 {
        PixelShader = compile ps_3_0 Blank();
    }
}
