using UnityEngine;

public class MathHelper
{
    // public static void ModifyAlpha(ref Color color, float a)
    // {
    //     a = Mathf.Clamp(a, 0f, 1f);
        
    //     return new Color(color.r, _col.g, _col.b, newAlpha);
    // }

    public static float SqrDistance(Vector3 a, Vector3 b)
    {
        return (b - a).sqrMagnitude;
    }

    // public static void GetVector_xzProjection(Vector3 v, out Vector3 result)
    // {
    //     //return new Vector3(v.x, 0f, v.z).normalized;
    //     result = new Vector3(v.x, 0f, v.z).normalized;
    // }

    const float eps = 0.0001f;

    public static bool V3Approx(Vector3 lhs, Vector3 rhs)
    {
        return (lhs - rhs).magnitude < eps;
    }

}
