texture src: register(t0);
sampler srcS: register(s0);
texture map: register(t1);
sampler mapS: register(s1);

uniform float4x4 World;
uniform float2 BufferSize;

float4 Distort(float2 uv : TEXCOORD0, float4 vertColor : COLOR0) : COLOR {
    float4 offsetPixel = tex2D(mapS, uv);
    int offsetX = (int) ((offsetPixel.r * 255 - 128) * offsetPixel.b);
    int offsetY = (int) ((offsetPixel.g * 255 - 128) * offsetPixel.b);
    float2 texelOffset = float2(((float) offsetX) / BufferSize.x, ((float) offsetY) / BufferSize.y);

    return tex2D(srcS, uv + texelOffset);
}

technique Shader {
    pass pass0 {
        PixelShader = compile ps_3_0 Distort();
    }
}
