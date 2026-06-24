texture text: register(t0);
sampler textSampler: register(s0);

// Credit to https://github.com/Firch/Bunco!

uniform float4x4 World;
uniform float2 BufferSize;
uniform float2 Offset;

uniform float Time;
uniform float SpinTime;
uniform float SpinEase;
uniform float4 Color1;
uniform float4 Color2;
uniform float4 Color3;
uniform float Contrast;
uniform float SpinAmount;

float4 SpritePixelShader(float2 realUV : TEXCOORD0, float4 vertColor : COLOR0) : COLOR {
    float2 pixelPos = realUV * BufferSize;
    float2 uv = pixelPos / length(BufferSize) + Offset - float2(0.5, 0.5) + float2(0.05, 0.25);

    float uvLen = length(uv);

    //Adding in a center swirl, changes with Time. Only applies meaningfully if the 'spin amount' is a non-zero float
    float speed = (SpinTime*SpinEase*0.2) + 302.2;
    float newPixelAngle = (atan2(uv.y, uv.x)) + speed - SpinEase*20.*(1.*SpinAmount*uvLen + (1. - 1.*SpinAmount));
    float2 mid = (BufferSize.xy/length(BufferSize.xy))/2.;
    uv = (float2((uvLen * cos(newPixelAngle) + mid.x), (uvLen * sin(newPixelAngle) + mid.y)) - mid);

    //Now add the paint effect to the swirled UV
    uv *= 30.;
    speed = Time*(2.);
   	float2 uv2 = float2(uv.x+uv.y, uv.x+uv.y);

    for(int i=0; i < 5; i++) {
  		uv2 += sin(max(uv.x, uv.y)) + uv;
  		uv  += 0.5*float2(cos(5.1123314 + 0.353*uv2.y + speed*0.131121),sin(uv2.x - 0.113*speed));
  		uv  -= 1.0*cos(uv.x + uv.y) - 1.0*sin(uv.x*0.711 - uv.y);
   	}

    //Make the paint amount range from 0 - 2
    float contrastMod = (0.25*Contrast + 0.5*SpinAmount + 1.2);
    float paintRes =min(2., max(0.,length(uv)*(0.035)*contrastMod));
    float c1p = max(0.,1. - contrastMod*abs(1.-paintRes));
    float c2p = max(0.,1. - contrastMod*abs(paintRes));
    float c3p = 1. - min(1., c1p + c2p);

    float4 retCol = (0.3/Contrast)*Color1 + (1. - 0.3/Contrast)*(Color1*c1p + Color2*c2p + float4(c3p*Color3.rgb, c3p*Color1.a));

    return retCol * vertColor;
}

technique Shader {
    pass pass0 {
        PixelShader = compile ps_3_0 SpritePixelShader();
    }
}
