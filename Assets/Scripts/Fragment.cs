using aus.Extension;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace pp
{
    public class Fragment
        : MonoBehaviour
    {
        public Material VoxelMaterial { get; private set; }
        public Bounds InitialSampleBounds { get; private set; }
        public Transform Origin => sourceOrigin.transform;

        [Header("Augmentaion Setup")]
        public bool shuffleIndecies = true; // to avoid relation of index(order) than position

        // transoform independent points
        public IEnumerable<Vector3> SamplePoints { get; private set; }

        private GameObject sourceOrigin;

        public void Setup(MeshFilter mesh, float displayVoxelSize = 0.002f)
        {
            SamplePoints = GenerateRandomSample(mesh.mesh, shuffleIndecies);
            Debug.Assert(SamplePoints.Count() > 0);

            if (!VoxelMaterial)
            {
                VoxelMaterial = new Material(Shader.Find("Standard"));
                VoxelMaterial.color = Color.red;
            }
            Debug.Assert(VoxelMaterial);

            if (sourceOrigin) Destroy(sourceOrigin);
            sourceOrigin = CreateVisualObject(SamplePoints, transform, VoxelMaterial);

            InitialSampleBounds = GetBoundsOfPoints(SamplePoints);
        }

        private static Bounds GetBoundsOfPoints(IEnumerable<Vector3> points)
        {
            var b = new Bounds(points.FirstOrDefault(), Vector3.zero);
            foreach (var p in points) b.Encapsulate(p);
            return b;
        }

        private static List<Vector3> GenerateRandomSample(Mesh source, bool shuffleIndex = true, float sampleRadius = 0.1f)
        {
            var pc = new aus.Geometry.PointCloud(source);
            var randomIndex = Random.Range(0, pc.Count);
            var randomPos = pc.Points[randomIndex];
            var samples = pc.GetPoints(randomPos, sampleRadius);
            if (shuffleIndex) samples.Shuffle();
            return samples;
        }

        private static GameObject CreateVisualObject(IEnumerable<Vector3> points, Transform parent, Material mat)
        {
            var origin = new GameObject("origin");
            foreach (var p in points)
            {
                var voxel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                voxel.name = $"{p:F4}"; // original position as the name for debugging
                voxel.GetComponent<MeshRenderer>().material = mat;
                voxel.transform.parent = origin.transform;
                voxel.transform.localPosition = p;
                voxel.transform.localScale = Vector3.one * 0.002f; // display voxelSize
            }
            origin.transform.parent = parent;
            return origin;
        }

        //private static GameObject CreateVisualObject(IEnumerable<Vector3> points, Material mat)
        //{
        //    // TODO: using point cloud shader/mesh than PrimitiveType.Sphere
        //    var origin = new GameObject("origin");
        //    foreach (var p in points)
        //    {
        //        var voxel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //        voxel.name = $"{p:F4}"; // original position as the name for debugging
        //        voxel.GetComponent<MeshRenderer>().material = mat;
        //        voxel.transform.position = p;
        //        voxel.transform.localScale = Vector3.one * 0.002f; // display voxelSize
        //        voxel.transform.parent = origin.transform;
        //    }
        //    return origin;
        //}

    }
}