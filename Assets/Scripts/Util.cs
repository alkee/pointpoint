using System.Collections.Generic;
using UnityEngine;

namespace pp
{
    public static class Util
    {
        public static GameObject CreateSamplePoints(Vector3 center, List<Vector3> points, float pointSize, Material pointMaterial = null)
        {
            if (points.Count == 0) return null;

            // pivot
            //   + origin(0,0) reference
            //   + points

            var pivot = new GameObject("pivot");
            pivot.transform.position = center;

            var origin = new GameObject("origin");
            origin.transform.parent = pivot.transform;

            //var bounds = new Bounds(points[0], Vector3.zero);
            foreach (var p in points)
            {
                //bounds.Encapsulate(p);
                var point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                point.name = $"{p:F4}";
                point.transform.parent = pivot.transform;
                point.transform.position = p;
                point.transform.localScale = Vector3.one * pointSize;
                if (pointMaterial) point.GetComponent<Renderer>().material = pointMaterial;
                else point.GetComponent<Renderer>().material.color = Color.red;
            }

            return pivot;
        }

        public static void LookAt(float distance, Transform source, Vector3 target)
        {
            LookAt(distance, source, target, Vector3.up);
        }

        public static void LookAt(float distance, Transform source, Vector3 target, Vector3 up)
        {
            var originalDistance = (target - source.position).magnitude;
            distance -= originalDistance;

            source.LookAt(target, up);
            var dir = source.forward * -1;
            source.Translate(dir * distance, Space.World);
        }
    }
}