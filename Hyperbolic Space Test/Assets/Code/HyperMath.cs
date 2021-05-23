using System;
using System.Collections.Generic;
using UnityEngine;

public static class HyperMath
{
    // The hyperbolic width of a tile
    public static float CellWidth = 0.0f;

    // The distance from the center of a tile to the vertex in Poincare coordinates
    public static float PoincareCellDiagonal = 0.0f;

    // The location of each vertex in Klein coordinates
    public static float KleinValue = 0.0f;

    // Curvature class (-1 = Hyperbolic, 0 = Euclidean, 1 = Spherical)
    public static float Curvature = 0.0f;

    // Number of square tiles that connect at each vertex
    public static int SquareTilesPerVertex = 4;

    public static void SetTileType(int n)
    {
        // Do calculations in double precision because this only needs to be called once
        // and it is very important that these number be as accurate as possible
        SquareTilesPerVertex = n;
        if (n == 4)
        {
            Curvature = 0.0f;
            KleinValue = 0.5f;
            CellWidth = 0.5f;
            PoincareCellDiagonal = Mathf.Sqrt(0.5f);
        }
        else
        {
            Curvature = n < 4 ? 1.0f : -1.0f;
            double a = Math.PI / 4.0;
            double b = Math.PI / n;
            double c = Math.Cos(a) * Math.Cos(b) / (Math.Sin(a) * Math.Sin(b));
            double s = Math.Sqrt(0.5 * Math.Abs(c - 1.0) / (c + 1.0));

            PoincareCellDiagonal = (float) (Math.Sqrt(2.0) * s);
            KleinValue = (float) (s / (0.5 - Curvature * s * s) + (3e-4 / n));

            // The tiny epsilon is added at the end to hide small gaps between tiles
            if (Curvature < 0.0f)
            {
                CellWidth = (float) (Acosh(Math.Cos(b) / Math.Sin(a)) - 1e-4);
            }
            else
            {
                CellWidth = (float) (Math.Acos(Math.Cos(b) / Math.Sin(a)) + 1e-3);
            }
        }
    }

    public static double Acosh(double x)
    {
        return Math.Log(x + Math.Sqrt(x * x - 1));
    }

    public static double Atanh(double x)
    {
        return 0.5 * Math.Log((1.0 + x) / (1.0 - x));
    }

    // Curvature-dependent tangent
    public static float CurvatureTan(float x)
    {
        if (Curvature > 0.0f)
        {
            return Mathf.Tan(x);
        }

        if (Curvature < 0.0f)
        {
            return (float) Math.Tanh(x);
        }

        return x;
    }

    public static Vector3 MobiusScalarMultiplication(Vector3 v, float r)
    {
        float magnitude = v.magnitude;
        return (float) Math.Tanh(r * Atanh(magnitude)) * v / magnitude;
    }

    public static float PoincareDistance(Vector3 a, Vector3 b)
    {
        return (float) Acosh((1f + 2f * Vector3.SqrMagnitude(a - b)) / ((1f - a.sqrMagnitude) * (1f - b.sqrMagnitude)));
    }
    
    public static float KleinDistance(Vector3 a, Vector3 b)
    {
        float innderProd = Vector3.Dot(a, b);
        return 2f * (float)Acosh(Mathf.Sqrt((innderProd * innderProd) / (Vector3.Dot(a, a) * Vector3.Dot(b, b))));
    }

    // 3D Möbius addition (non-commutative, non-associative)
    // NOTE: This is much more numerically stable than the one in Ungar's paper
    public static Vector3 MobiusAdd(Vector3 a, Vector3 b)
    {
        Vector3 c = Curvature * Vector3.Cross(a, b);
        float d = 1.0f - Curvature * Vector3.Dot(a, b);
        Vector3 t = a + b;
        return (t * d + Vector3.Cross(c, t)) / (d * d + c.sqrMagnitude);
    }

    // 3D Mobius gyration
    public static Quaternion MobiusGyr(Vector3 a, Vector3 b)
    {
        // We're actually doing this operation:
        // Quaternion.AngleAxis(180.0f, MobiusAdd(a, b)) * Quaternion.AngleAxis(180.0f, a + b);
        // But the precision is better (and faster) by doing the way below:
        Vector3 c = Curvature * Vector3.Cross(a, b);
        float d = 1.0f - Curvature * Vector3.Dot(a, b);
        Quaternion q = new Quaternion(c.x, c.y, c.z, d);
        q.Normalize();
        return q;
    }

    // Optimization to combine Mobius addition and gyration operations.
    // Equivalent to sum = MobiusAdd(a,b); gyr = MobiusGyr(b,a);
    public static void MobiusAddGyr(Vector3 a, Vector3 b, out Vector3 result, out Quaternion postRotation)
    {
        Vector3 c = Curvature * Vector3.Cross(a, b);
        float d = 1.0f - Curvature * Vector3.Dot(a, b);
        Vector3 t = a + b;
        result = (t * d + Vector3.Cross(c, t)) / (d * d + c.sqrMagnitude);
        postRotation = new Quaternion(c.x, c.y, c.z, -d);
        postRotation.Normalize();
    }

    // Hyperbolic coordinate system conversions
    public static Vector3 UnitToKlein(Vector3 u)
    {
        return u * KleinValue;
    }

    public static Vector3 KleinToPoincare(Vector3 u)
    {
        return u / (Mathf.Sqrt(1.0f + Curvature * Vector3.Dot(u, u)) + 1.0f);
    }

    public static Vector3 UnitToPoincare(Vector3 u)
    {
        return KleinToPoincare(UnitToKlein(u));
    }

    public static Vector3 KleinToUnit(Vector3 u)
    {
        return u / KleinValue;
    }

    public static Vector3 PoincareToKlein(Vector3 u)
    {
        return u * (2.0f / (1.0f - Curvature * Vector3.Dot(u, u)));
    }

    public static Vector3 PoincareToUnit(Vector3 u)
    {
        return KleinToUnit(PoincareToKlein(u));
    }

    public static float PoincareToUnitScaleFactor(Vector3 p)
    {
        return 0.5f * KleinValue * Mathf.Abs(1.0f + Curvature * (p.x * p.x + p.z * p.z));
    }

    // Apply a translation to the a hyper-rotation
    public static Vector3 HyperTranslate(float dx, float dz)
    {
        return HyperTranslate(new Vector3(dx, 0.0f, dz));
    }

    public static Vector3 HyperTranslate(float dx, float dy, float dz)
    {
        return HyperTranslate(new Vector3(dx, dy, dz));
    }

    public static Vector3 HyperTranslate(Vector3 translation)
    {
        float magnitude = translation.magnitude;
        if (magnitude < 1e-5f)
        {
            return Vector3.zero;
        }

        return translation * (CurvatureTan(magnitude) / magnitude);
    }

    public static GyroVector TileCoordToGyroVectorAlternateOne(string s)
    {
        GyroVector gv = GyroVector.identity;
        int countedRotation = 0;
        for (int i = 0; i < s.Length; ++i)
        {
            char currentChar = char.ToUpper(s[i]);
            
            switch (RotateChar(currentChar, countedRotation))
            {
                case 'R':
                    gv += HyperTranslate(CellWidth, 0.0f);
                    break;
                case 'L':
                    gv += HyperTranslate(-CellWidth, 0.0f);
                    break;
                case 'U':
                    gv += HyperTranslate(0.0f, CellWidth);
                    break;
                case 'D':
                    gv += HyperTranslate(0.0f, -CellWidth);
                    break;
            }
            
            switch (currentChar)
            {
                case 'R':
                    countedRotation += 1;
                    break;
                case 'L':
                    countedRotation -= 1;
                    break;
                case 'D':
                    countedRotation += 2;
                    break;
            }
        }

        return gv;
    }
    
    public static GyroVector TileCoordToGyroVectorAlternateTwo(string s)
    {
        GyroVector gv = GyroVector.identity;
        int countedRotation = 0;
        for (int i = 0; i < s.Length; ++i)
        {
            char currentChar = char.ToUpper(s[i]);
            
            switch (currentChar)
            {
                case 'R':
                    countedRotation += 1;
                    continue;
                case 'L':
                    countedRotation -= 1;
                    continue;

                case 'D':
                    continue;
            }
            
            switch (RotateChar(currentChar, countedRotation))
            {
                case 'R':
                    gv += HyperTranslate(CellWidth, 0.0f);
                    break;
                case 'L':
                    gv += HyperTranslate(-CellWidth, 0.0f);
                    break;
                case 'U':
                    gv += HyperTranslate(0.0f, CellWidth);
                    break;
                case 'D':
                    gv += HyperTranslate(0.0f, -CellWidth);
                    break;
            }
        }

        return gv;
    }

    public static GyroVector TileCoordToGyroVector(string s)
    {
        return TileCoordToGyroVectorAlternateTwo(s);
    }
    
    public static GyroVector TileCoordToGyroVectorAlternateThree(string s)
    {
        GyroVector gv = GyroVector.identity;
        for (int i = 0; i < s.Length; ++i)
        {
            char currentChar = char.ToUpper(s[i]);
            switch (currentChar)
            {
                case 'R':
                    gv += HyperTranslate(CellWidth, 0.0f);
                    break;
                case 'L':
                    gv += HyperTranslate(-CellWidth, 0.0f);
                    break;
                case 'U':
                    gv += HyperTranslate(0.0f, CellWidth);
                    break;
                case 'D':
                    gv += HyperTranslate(0.0f, -CellWidth);
                    break;
            }
        }

        return gv;
    }

    private static readonly List<char> _charOrder = new List<char> {'R', 'D', 'L', 'U'};


    private static char RotateChar(char ch, int count)
    {
        return _charOrder[(_charOrder.IndexOf(ch) + count + 4) % 4];
    }

    private static int CalculateRotation(char from, char to)
    {
        int fromIndex = _charOrder.IndexOf(from);
        int toIndex = _charOrder.IndexOf(to);

        if (RotateChar(from, -1) == to)
        {
            return -1;
        }

        if (RotateChar(@from, 1) == to)
        {
            return 1;
        }

        if (RotateChar(@from, 2) == to)
        {
            return 2;
        }

        return 0;
    }
}