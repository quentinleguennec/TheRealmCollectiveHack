using UnityEngine;

public static class MathTools
{
    //Returns the closiest point to cur position on bounds of cld
    public static Vector3 CalcPointOnBounds(Collider cld, Vector3 cur)
    {
        return cld.ClosestPoint(cur);

        //SphereCollider sphc = cld as SphereCollider;

        //if (!sphc)
        //    return cld.ClosestPoint(cur);
        ////return cld.ClosestPointOnBounds( cur );
        //else
        //{
        //    //cld.ClosestPointOnBounds returns not precise values for spheres
        //    //Fortunately they could be calculated easily
        //    var realPos = sphc.transform.position + sphc.center;
        //    var dir = cur - realPos;
        //    var realScale = sphc.transform.lossyScale;
        //    var realRadius = sphc.radius * Mathf.Max(realScale.x, realScale.y, realScale.z);
        //    var dirLength = dir.magnitude;

        //    //BoxCollider.ClosestPointOnBounds returns cur if points are inside the volume
        //    if (dirLength < realRadius)
        //        return cur;

        //    var dirFraction = realRadius / dirLength;
        //    return realPos + dirFraction * dir;
        //}
    }

    //Map interval of angles between vectors [0..Pi] to interval [0..1]
    //Vectors a and b must be normalized
    public static float AngleToFactor(Vector3 a, Vector3 b)
    {
        //plot((1-cos(x))/2, x = 0..Pi);
        return (1 - Vector3.Dot(a, b)) / 2;
    }
}
