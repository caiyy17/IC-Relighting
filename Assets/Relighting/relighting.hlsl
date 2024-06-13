float3 CalculateBarycentricWeights(float2 p, float2 a, float2 b, float2 c)
{
    float2 v0 = b - a, v1 = c - a, v2 = p - a;
    float d00 = dot(v0, v0);
    float d01 = dot(v0, v1);
    float d11 = dot(v1, v1);
    float d20 = dot(v2, v0);
    float d21 = dot(v2, v1);
    float denom = d00 * d11 - d01 * d01;
    float v = (d11 * d20 - d01 * d21) / denom;
    float w = (d00 * d21 - d01 * d20) / denom;
    float u = 1.0 - v - w;
    return float3(u, v, w);
}

void CalculateWeights(
    float2 param, float2 pointA, float2 pointB, float2 pointC, float2 pointD, float2 pointE,
    out float weightA, out float weightB, out float weightC, out float weightD, out float weightE)
{
    if (param.x >= 0 && param.y >= 0){
        float3 weights = CalculateBarycentricWeights(param, pointA, pointB, pointC);
        weightA = weights.x;
        weightB = weights.y;
        weightC = weights.z;
        weightD = 0;
        weightE = 0;
    }
    else if (param.x < 0 && param.y >= 0){
        float3 weights = CalculateBarycentricWeights(param, pointA, pointC, pointD);
        weightA = weights.x;
        weightB = 0;
        weightC = weights.y;
        weightD = weights.z;
        weightE = 0;
    }
    else if (param.x < 0 && param.y < 0){
        float3 weights = CalculateBarycentricWeights(param, pointA, pointD, pointE);
        weightA = weights.x;
        weightB = 0;
        weightC = 0;
        weightD = weights.y;
        weightE = weights.z;
    }
    else if (param.x >= 0 && param.y < 0){
        float3 weights = CalculateBarycentricWeights(param, pointA, pointE, pointB);
        weightA = weights.x;
        weightB = weights.z;
        weightC = 0;
        weightD = 0;
        weightE = weights.y;
    }
    else{
        weightA = 0;
        weightB = 0;
        weightC = 0;
        weightD = 0;
        weightE = 0;
    }
}

float4 MyCustomFunction_float(
    float4 inputL, float4 inputR, float4 inputU, float4 inputD, 
    float4 globalColor, float4 lightColor, float2 lightPosition, float intensity,
    out float4 result)
{
    float lightFactor = 10.0;
    float epsilon = 0.001;
    float4 neutral = (inputL + inputR + inputU + inputD) / 4.0;
    float4 relativeL = (inputL + epsilon) / (neutral + epsilon);
    float4 relativeR = (inputR + epsilon) / (neutral + epsilon);
    float4 relativeU = (inputU + epsilon) / (neutral + epsilon);
    float4 relativeD = (inputD + epsilon) / (neutral + epsilon);

    float4 diffU = inputR - inputL;
    float4 diffV = inputU - inputD;

    float weightA, weightB, weightC, weightD, weightE;
    CalculateWeights(lightPosition * lightFactor, float2(0, 0), float2(1, 0), float2(0, 1), float2(-1, 0), float2(0, -1), weightA, weightB, weightC, weightD, weightE);

    float4 basePart = globalColor;
    float4 lightPart = (lightPosition.x * diffU + lightPosition.y * diffV) * lightFactor * lightColor;
    // float4 lightPart = weightA * 1 + weightB * relativeR + weightC * relativeU + weightD * relativeL + weightE * relativeD;
    lightPart = lightPart * intensity;
    lightPart = clamp(lightPart, 0, 3.0);
    float4 combined = basePart + lightPart;
    result = combined * neutral;
    return 1.0;
}