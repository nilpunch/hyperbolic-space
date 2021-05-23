using System;
using UnityEngine;

[Serializable]
public struct GyroVector
{
    public static readonly GyroVector identity = new GyroVector(Vector3.zero, Quaternion.identity);

    public Vector3 position; // This is the hyperbolic offset vector or position
    public Quaternion gyration; // This is the post-rotation as a result of holonomy

    public GyroVector(Vector3 position) : this(position, Quaternion.identity) {}

    public GyroVector(Vector3 position, Quaternion gyration)
    {
        this.position = position;
        this.gyration = gyration;
    }
    
    // Compose the GyroVector with a Mobius Translation
    public static GyroVector operator+(GyroVector gyroVector, Vector3 translation) 
    {
        HyperMath.MobiusAddGyr(gyroVector.position, Quaternion.Inverse(gyroVector.gyration) * translation, out Vector3 resultPosition, out Quaternion postRotation);
        return new GyroVector(resultPosition, gyroVector.gyration * postRotation);
    }
    public static GyroVector operator+(Vector3 translation, GyroVector gyroVector) 
    {
        HyperMath.MobiusAddGyr(translation, gyroVector.position, out Vector3 resultPosition, out Quaternion postRotation);
        return new GyroVector(resultPosition, gyroVector.gyration * postRotation);
    }
    public static GyroVector operator+(GyroVector a, GyroVector b) 
    {
        HyperMath.MobiusAddGyr(a.position, Quaternion.Inverse(a.gyration) * b.position, out Vector3 resultPosition, out Quaternion postRotation);
        return new GyroVector(resultPosition, b.gyration * a.gyration * postRotation);
    }

    // Inverse GyroVector
    public static GyroVector operator-(GyroVector gyroVector) 
    {
        return new GyroVector(-(gyroVector.gyration * gyroVector.position), Quaternion.Inverse(gyroVector.gyration));
    }

    // Inverse composition
    public static GyroVector operator-(GyroVector gyroVector, Vector3 translation) 
    {
        return gyroVector + (-translation);
    }
    public static GyroVector operator-(Vector3 translation, GyroVector gyroVector) 
    {
        return translation + (-gyroVector);
    }
    public static GyroVector operator-(GyroVector a, GyroVector b) 
    {
        return a + (-b);
    }
    
    // Apply the full GyroVector transformation to a point
    public static Vector3 operator*(GyroVector gyroVector, Vector3 point) 
    {
        return gyroVector.gyration * HyperMath.MobiusAdd(gyroVector.position, point);
    }
    
    public static GyroVector operator*(GyroVector gyroVector, float scalar)
    {
        return new GyroVector(HyperMath.MobiusScalarMultiplication(gyroVector.position, scalar), gyroVector.gyration);
    }
    public static GyroVector operator*(float scalar, GyroVector gyroVector)
    {
        return gyroVector * scalar;
    }
    
    public Vector3 MobiusAdd(Vector3 point) {
        return HyperMath.MobiusAdd(position, point);
    }
    
    public override string ToString() {
        return "(" + ((double)position.x).ToString("F9") + ", " +
               ((double)position.y).ToString("F9") + ", " +
               ((double)position.z).ToString("F9") + ") [" +
               ((double)gyration.x).ToString("F9") + ", " +
               ((double)gyration.y).ToString("F9") + ", " +
               ((double)gyration.z).ToString("F9") + ", " +
               ((double)gyration.w).ToString("F9") + "]";
    }
}