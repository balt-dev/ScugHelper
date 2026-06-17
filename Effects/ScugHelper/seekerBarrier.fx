texture text: register(t0);
sampler textSampler: register(s0);

uniform float4x4 World;
uniform float2 CameraPosition;
uniform float2 TexelSize;
uniform float ActiveTime;
uniform float4 ParticleColor;

float mod(float x, float y) {
    return x - y * floor(x / y);
}

// From https://github.com/patriciogonzalezvivo/lygia/blob/main/generative/random.hlsl
float rand1(float x) {
    x = frac(x * 0.1031);
    x *= x + 33.33;
    x *= x + x;
    return frac(x);
}

#define MaxParticleCount 8

float4 SpritePixelShader(float2 uv : TEXCOORD0, float4 vertColor : COLOR0) : COLOR {
    float4 actualColor = tex2D(textSampler, uv) * vertColor;
    float2 pxCoord = (uv / TexelSize) + CameraPosition;
    float2 worldPos = float2(floor(mod(pxCoord.x, 512)), floor(mod(pxCoord.y, 128)));

    int particleCount = int(rand1(worldPos.x * -7.3 + 17.3) * MaxParticleCount) + 1;

    float4 particleTotal = float4(0.0, 0.0, 0.0, 0.0);
    if (actualColor.a <= 0.01) return actualColor;

    for (int i = 1; i <= particleCount; i++) {
        float randomValue = rand1(worldPos.x * float(i) * 7.193 + 14.124);
        float particleSpeed = randomValue < 0.4 ? 12.0 : randomValue < 0.7 ? 20.0 : 40.0;
        float particleStart = randomValue * -790.53;
        int particlePos = int(mod(particleStart + particleSpeed * ActiveTime, 128.0));
        if (int(mod(worldPos.y, 128.0)) == particlePos) particleTotal = ParticleColor + particleTotal * (1.0 - ParticleColor.a);
    }

    return particleTotal + actualColor * (1.0 - particleTotal.a);
}

technique Shader
{
    pass pass0
    {
        PixelShader = compile ps_3_0 SpritePixelShader();
    }
}
