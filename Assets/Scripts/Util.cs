using System.Collections.Generic;
using System.Linq;
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

        public static GameObject DrawPoints(IEnumerable<Vector3> points, Color color, float size = 0.002f)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;

            var container = new GameObject();
            container.transform.position = Vector3.zero;
            foreach (var p in points)
            {
                var voxel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                voxel.name = $"{p:F4}"; // original position as the name for debugging
                voxel.GetComponent<MeshRenderer>().material = mat;
                voxel.transform.parent = container.transform;
                voxel.transform.localPosition = p;
                voxel.transform.localScale = Vector3.one * size;
            }

            return container;
        }
    }

    public static class UnityExt
    {
        public static IEnumerable<Vector3> MultiplyPoint3x4(this IEnumerable<Vector3> points, Matrix4x4 transform)
        {
            return points.Select(x => transform.MultiplyPoint3x4(x));
        }

        public static IEnumerable<Vector3> Sample(this IEnumerable<Vector3> points, int count)
        {
            var srcCount = points.Count();
            if (count > srcCount) throw new System.ArgumentException($"count should less than {srcCount}", nameof(count));
            var sample = new List<Vector3>();
            if (count < 1) return sample;

            var stepSize = srcCount / (double)count;
            var next = 0.0;
            var i = 0;
            foreach (var p in points)
            {
                if (i++ >= next)
                {
                    next += stepSize;
                    sample.Add(p);
                }
            }
            return sample;
        }
    }
}