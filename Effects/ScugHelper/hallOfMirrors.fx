texture text: register(t0);
sampler textSampler: register(s0);

uniform float4x4 World;
uniform float2 BufferSize;
uniform float2 Offset;
uniform float Rotation;
uniform float Zoom;

float mod(float x, float y) {
    return x - y * floor(x / y);
}

float4 SpritePixelShader(float2 uv : TEXCOORD0, float4 vertColor : COLOR0) : COLOR {
    float2 aspectRatio = float2(1.0, BufferSize.x / BufferSize.y);
    float2 centeredUv = (uv - float2(0.5, 0.5)) / aspectRatio;
    float2 rotatedUv = float2(
        cos(Rotation) * centeredUv.x - sin(Rotation) * centeredUv.y,
        sin(Rotation) * centeredUv.x + cos(Rotation) * centeredUv.y
    );
    float2 zoomedUv = rotatedUv * aspectRatio / Zoom;
    float2 offset = Offset / BufferSize;
    float2 finalUv = zoomedUv + 0.5 + offset;
    return tex2D(textSampler, finalUv) * vertColor;
}

technique Shader {
    pass pass0 {
        PixelShader = compile ps_3_0 SpritePixelShader();
    }
}
