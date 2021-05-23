#pragma exclude_renderers d3d11 gles

float4 modelObjectPosition()
{
    return mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
}

float4 modelObjectScale()
{
    return float4(
        length(unity_ObjectToWorld._m00_m10_m20),
        length(unity_ObjectToWorld._m01_m11_m21),
        length(unity_ObjectToWorld._m02_m12_m22),
        1
    );
}

float sqrMagnitude(float3 vec)
{
    return dot(vec, vec);
}

float acosh(float x)
{
    return log(x + sqrt(x * x - 1));
}

float atanh(float f)
{
    return 0.5 * log((1 + f) / (1 - f));
}

float curvatureTan(float x, float curvature)
{
    if (curvature > 0.0f)
    {
        return tan(x);
    }
    else if (curvature < 0.0f)
    {
        return tanh(x);
    }
    else
    {
        return x;
    }
}

float4 hyperTranslate(float3 translation, float curvature)
{
    float magnitude = length(translation);
    if (magnitude < 1e-5f)
    {
        return 0;
    }
    return float4(translation * (curvatureTan(magnitude, curvature) / magnitude), 1);
}

float4 mobiusAdd(float3 u, float3 v, float curvature)
{
    float3 c = curvature * cross(u, v);
    float d = 1.0f - curvature * dot(u, v);
    float3 t = u + v;
    return float4((t * d + cross(c, t)) / (d * d + dot(c, c)), 1);
}

// 3D Mobius gyration
float4 mobiusGyr(float3 a, float3 b, float curvature)
{
    // We're actually doing this operation:
    // Quaternion.AngleAxis(180.0f, MobiusAdd(a, b)) * Quaternion.AngleAxis(180.0f, a + b);
    // But the precision is better (and faster) by doing the way below:
    float3 c = curvature * cross(a, b);
    float d = 1.0f - curvature * dot(a, b);
    float4 q = float4(c.x, c.y, c.z, d);
    q = normalize(q);
    return q;
}

void mobiusAddGyr(float3 a, float3 b, float curvature, out float3 result, out float4 postRotation)
{
    float3 c = curvature * cross(a, b);
    float d = 1.0f - curvature * dot(a, b);
    float3 t = a + b;
    result = (t * d + cross(c, t)) / (d * d + sqrMagnitude(c));
    postRotation = float4(c.x, c.y, c.z, -d);
    postRotation = normalize(postRotation);
}

float3 applyQuaternion(float4 rotation, float3 vertex)
{
    float num1 = rotation.x * 2.;
    float num2 = rotation.y * 2.;
    float num3 = rotation.z * 2.;
    float num4 = rotation.x * num1;
    float num5 = rotation.y * num2;
    float num6 = rotation.z * num3;
    float num7 = rotation.x * num2;
    float num8 = rotation.x * num3;
    float num9 = rotation.y * num3;
    float num10 = rotation.w * num1;
    float num11 = rotation.w * num2;
    float num12 = rotation.w * num3;
    float3 vector3;
    vector3.x = (float)((1.0 - ((float)num5 + (float)num6)) * (float)vertex.x + ((float)num7 - (float)num12) * (
        float)vertex.y + ((float)num8 + (float)num11) * (float)vertex.z);
    vector3.y = (float)(((float)num7 + (float)num12) * (float)vertex.x + (1.0 - ((float)num4 + (float)num6)) * (
        float)vertex.y + ((float)num9 - (float)num10) * (float)vertex.z);
    vector3.z = (float)(((float)num8 - (float)num11) * (float)vertex.x + ((float)num9 + (float)num10) * (float)
        vertex.y + (1.0 - ((float)num4 + (float)num5)) * (float)vertex.z);
    return vector3;
}

float4 quaternionMul(float4 lhs, float4 rhs)
{
    return float4(
        (float)((float)lhs.w * (float)rhs.x + (float)lhs.x * (float)rhs.w + (float)lhs.y * (float)rhs.z - (float)
            lhs.z * (float)rhs.y),
        (float)((float)lhs.w * (float)rhs.y + (float)lhs.y * (float)rhs.w + (float)lhs.z * (float)rhs.x - (float)
            lhs.x * (float)rhs.z),
        (float)((float)lhs.w * (float)rhs.z + (float)lhs.z * (float)rhs.w + (float)lhs.x * (float)rhs.y - (float)
            lhs.y * (float)rhs.x),
        (float)((float)lhs.w * (float)rhs.w - (float)lhs.x * (float)rhs.x - (float)lhs.y * (float)rhs.y - (float)
            lhs.z * (float)rhs.z));
}

float4 quaternionInverse(float4 q)
{
    return float4(-q.x, -q.y, -q.z, q.w);
}

float4 mobiusScaling(float3 v, float r)
{
    float magnitude = length(v);
    return float4(tanh(r * atanh(magnitude)) * v / magnitude, 1);
}

float4 fromPoincareToKlein(float3 vec, float curvature)
{
    return float4(vec * 2.0f / (1.0f + sqrMagnitude(vec)), 1);
}

float4 fromKleinToPoincare(float3 vec, float curvature)
{
    return float4(vec / (sqrt(1.0 + curvature * sqrMagnitude(vec)) + 1.0), 1);
}

float poincareDistance(float3 a, float3 b)
{
    return (float)acosh((1 + 2 * sqrMagnitude(a - b)) / ((1 - sqrMagnitude(a)) * (1 - sqrMagnitude(b))));
}

float4 gyrovectorTransformation(float3 vertex, float curvature, float4 offset)
{
    float4 modelPosition = modelObjectPosition();
    float4 localPlanarOffset = float4(modelPosition.x, 0, modelPosition.z, 0);

    // Model Matrix Transformation without translation
    float4 modelScaleRotation = float4(vertex, 1) - localPlanarOffset;

    float4 normalizedPlanarOffset = localPlanarOffset;

    // Normalization step
    if (curvature < 0)
    {
        normalizedPlanarOffset = fromKleinToPoincare(localPlanarOffset, curvature);
    }
    modelScaleRotation = fromKleinToPoincare(modelScaleRotation, curvature);


    // GyroVector Translation
    float4 vertexPosition = mobiusAdd(normalizedPlanarOffset, modelScaleRotation, curvature);

    // Apply Global Offset, Y separately from XZ 
    float3 globalPlanarOffset = float3(offset.x, 0, offset.z);
    float3 globalHeightOffset = float3(0, offset.y, 0);
    float4 globalVertexPosition = mobiusAdd(globalPlanarOffset, vertexPosition, curvature);
    globalVertexPosition = mobiusAdd(globalHeightOffset, globalVertexPosition, curvature);

    return globalVertexPosition;
}

struct GyroVector
{
    float3 position : POSITION;
    float4 gyration : GYRATION;
};

GyroVector createGyroVector(float3 position, float4 gyration)
{
    GyroVector gyroVector;
    gyroVector.position = position;
    gyroVector.gyration = gyration;
    return gyroVector;
}

GyroVector createGyroVector()
{
    return createGyroVector(0, 0);
}

GyroVector createGyroVector(float3 position)
{
    return createGyroVector(position, 0);
}

GyroVector add(GyroVector a, GyroVector b, float curvature)
{
    float3 resultPosition;
    float4 postRotation;
    mobiusAddGyr(a.position, applyQuaternion(quaternionInverse(a.gyration), b.position), curvature, resultPosition, postRotation);
    return createGyroVector(resultPosition,  quaternionMul(quaternionMul(b.gyration, a.gyration), postRotation));
}