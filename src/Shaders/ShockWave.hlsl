#define D2D_INPUT_COUNT 1
#define D2D_INPUT0_COMPLEX

#include "d2d1effecthelpers.hlsli"

cbuffer ConstantBuffer : register(b0)
{
    float2 CenterPoint; // center point
    float Progress;     // progress
    float Ratio;        // width / height
}

D2D_PS_ENTRY(main)
{
    // map Intensity to 0.1 - 0.8
    float Intensity = clamp(1 - sqrt(Progress), 0.01, 1);
    
    float CurrentTime = Progress;
    
    float3 WaveParams = float3(10.0, 0.8, 0.1); 
    
    float2 WaveCentre = CenterPoint;
   
    float4 uv = D2DGetInputCoordinate(0);
    float2 texCoord = float2(uv.x, uv.y);

    float Dist = distance(float2(texCoord.x * Ratio, texCoord.y), float2(WaveCentre.x * Ratio, WaveCentre.y)) * Intensity;

    float4 Color = D2DSampleInput(0, texCoord);
    
    // Only distort the pixels within the parameter distance from the centre
    if ((Dist <= ((CurrentTime) + (WaveParams.z))) && 
        (Dist >= ((CurrentTime) - (WaveParams.z)))) 
    {
        // The pixel offset distance based on the input parameters
        float Diff = (Dist - CurrentTime); 
        float ScaleDiff = (1 - pow(abs(Diff * WaveParams.x), WaveParams.y)); 
        float DiffTime = (Diff  * ScaleDiff);
        
        // The direction of the distortion
        float2 DiffTexCoord = normalize(texCoord - WaveCentre);         
        
        // Perform the distortion and reduce the effect over time
        texCoord += ((DiffTexCoord * DiffTime) / (CurrentTime * Dist * 40.0));
        Color = D2DSampleInput(0, texCoord);
        float4 Color2 = Color * ScaleDiff;
        
        // Blow out the color and reduce the effect over time
        Color += (Color * ScaleDiff) / (CurrentTime * Dist * 40.0);
    } 
    
    return Color; 
}