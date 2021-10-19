using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace pp
{
    public class Stage
        : MonoBehaviour
    {
        [Header("Level data")]
        [Tooltip(".obj files in StreamingAssets directory")]
        public List<string> sourceObjPaths = new List<string>();

        #region Unity message handlers

        private void Awake()
        {
            scenePivot = new GameObject(nameof(scenePivot));
            scenePivot.transform.parent = transform;
            modelPivot = new GameObject(nameof(modelPivot));
            modelPivot.transform.parent = transform;
        }

        private void Start()
        {
            Debug.Assert(sourceObjPaths.Count > 0);

            //Init();
            Load();
        }

        private void Update()
        {
            testVal = GetTransformDiff();
        }

        #endregion Unity message handlers

        private GameObject scenePivot; // parent of scene
        private Vector3 sceneOffset;
        private aus.Geometry.WavefrontObjMesh scene; // source .obj mesh
        private GameObject modelPivot; // parent of model
        private Vector3 modelOffset;
        private Fragment model; // sample fragment from scene

        private Vector3[] originalScenePoints;
        public IEnumerable<Vector3> ScenePoints => originalScenePoints.MultiplyPoint3x4(scenePivot.transform.localToWorldMatrix);
        public IEnumerable<Vector3> ModelPoints => model.SamplePoints.MultiplyPoint3x4(modelPivot.transform.localToWorldMatrix);

        private void Load()
        {
            // == TRANSFORM TREE ==
            // Stage
            //   scene obj(rotation pivot)
            //     origin(scene - wavefrontobjmesh)
            //       obj mesh
            //   model obj(rotation pivot)
            //     origin(model - fragment)
            //       points

            LoadScene();
            LoadModel();

            // randomize stage
            //RandomizeTransform(scenePivot.transform, 0.0f, 0.3f);
            //RandomizeTransform(modelPivot.transform, 0.0f, 0.3f);
            //modelPivot.transform.Translate(0, 0.3f, 0, Space.World); // always higher than

            Debug.Log($"{nameof(sceneOffset)}={sceneOffset:F4}, {nameof(modelOffset)}={modelOffset:F4}");
        }

        private void LoadScene()
        {
            if (scene) Destroy(scene.gameObject); // clear previous scene
            var sceneObj = new GameObject("scene obj");
            sceneObj.transform.parent = scenePivot.transform;
            scene = sceneObj.AddComponent<aus.Geometry.WavefrontObjMesh>();
            var randomModelFilePath = SelectRandomFilePath(sourceObjPaths);
            scene.Load(randomModelFilePath);
            sceneOffset = scene.GetMeshFilter().mesh.bounds.center;
            scenePivot.transform.localPosition = Vector3.zero;
            sceneObj.transform.localPosition = -sceneOffset; // center to their pivot

            originalScenePoints = scene.GetMeshFilter().mesh.vertices;
        }

        private void LoadModel()
        {
            if (model) Destroy(model.gameObject); // clear previous model
            var modelObj = new GameObject("model obj");
            modelObj.transform.parent = modelPivot.transform;
            model = modelObj.AddComponent<Fragment>();
            model.Setup(scene.GetMeshFilter());
            modelOffset = model.InitialSampleBounds.center;
            modelPivot.transform.localPosition = Vector3.zero;
            modelObj.transform.localPosition = -modelOffset; // center to their pivot
        }

        public float testVal;
        public bool hint;
        public void Test()
        {
            // hint
            if (hint) modelPivot.transform.localPosition = GetModelPivotPosOverScenePivot();

            var residual = GetResidual();
            Debug.Log($"{nameof(residual)} = {residual}");
        }

        private Vector3 OffsetDiff => (modelOffset - sceneOffset);

        private Vector3 GetModelPivotPosOverScenePivot()
        {
            return OffsetDiff + scenePivot.transform.localPosition;
        }

        public float GetTransformDiff()
        {
            var posDiff = (scenePivot.transform.localPosition - modelPivot.transform.localPosition + OffsetDiff).magnitude;
            var angleDiff = Quaternion.Angle(scenePivot.transform.localRotation, modelPivot.transform.localRotation);
            return posDiff + posDiff * angleDiff;
        }

        public float GetResidual(int sampleCount = 100)
        {
            var m = Matrix4x4.TRS(modelPivot.transform.localPosition - modelOffset
                , modelPivot.transform.localRotation
                , modelPivot.transform.localScale);
            var s = Matrix4x4.TRS(scenePivot.transform.localPosition - sceneOffset
                , scenePivot.transform.localRotation
                , scenePivot.transform.localScale);

            var mm = model.SamplePoints;
            mm = mm.MultiplyPoint3x4(m);
            var ss = originalScenePoints;
            ss = ss.MultiplyPoint3x4(s).ToArray();

            //Util.DrawPoints(mm, Color.red);
            //Util.DrawPoints(ss, Color.white);

            var scenePointTree = KDTree.MakeFromPoints(ss);

            var total = 0.0f;
            var modelPoints = mm.Sample(sampleCount); // performance issue...

            foreach (var mp in modelPoints)
            {
                total += scenePointTree.FindNearestK_R(mp, 1);
            }

            var avg = Mathf.Sqrt(total) / modelPoints.Count();
            return avg;
        }

        private static string SelectRandomFilePath(List<string> objFilePathsInStreamingAssets)
        {
            var randomIndex = Random.Range(0, objFilePathsInStreamingAssets.Count);
            return Path.Combine(Application.streamingAssetsPath
                , objFilePathsInStreamingAssets[randomIndex]);
        }

        private static void RandomizeTransform(Transform transform, float minDistance, float maxDistance)
        {
            var distance = Random.Range(minDistance, maxDistance);
            transform.localPosition = Random.onUnitSphere * distance;
            transform.localRotation = Random.rotation;
        }

        //private GameObject sourceModel; // scene
        //private GameObject fragment; // model
        //private List<Vector3> originalSamplePoints;
        //private List<Vector3> samplePoints; // transfrom source model points by initTransform
        //private Matrix4x4 sampleTransform; // initial model transform

        //private void Init()
        //{
        //    // load source
        //    if (sourceModel) Destroy(sourceModel);
        //    sourceModel = LoadRandomeModel(sourceObjPaths);
        //    sourceModel.transform.parent = transform;
        //    RandomizeTransform(sourceModel.transform, 0, 0.2f);

        //    // sampling from source
        //    var sourceMeshFilter = sourceModel.GetComponent<MeshFilter>();
        //    Debug.Assert(sourceMeshFilter);
        //    if (fragment) Destroy(fragment);
        //    fragment = new GameObject(nameof(fragment));
        //    var f = fragment.AddComponent<Fragment>();
        //    f.Setup(sourceMeshFilter);
        //    f.transform.parent = transform;

        //    //originalSamplePoints = GenerateRandomFragment(sourceMeshFilter);
        //    //// TODO: better starting point of fragment
        //    //sampleTransform = GetRenadomTransformMatrix(0, 0.5f);
        //    //samplePoints = MultiplyPoint3x4(sampleTransform, originalSamplePoints).ToList();

        //    // create fragment from sample
        //    //if (fragment) Destroy(fragment);
        //    //fragment = new GameObject(nameof(fragment));
        //    //var f = fragment.AddComponent<Fragment>();
        //    //f.Setup(sourceMeshFilter);
        //    //f.transform.parent = transform;
        //}

        //private static GameObject LoadRandomeModel(List<string> objFilePathsInStreamingAssets)
        //{
        //    var randomIndex = Random.Range(0, objFilePathsInStreamingAssets.Count);
        //    var newSourceFilePath = Path.Combine(Application.streamingAssetsPath
        //        , objFilePathsInStreamingAssets[randomIndex]);
        //    Debug.Log($"loading mesh : {newSourceFilePath}");

        //    var obj = aus.Geometry.WavefrontObjMesh.CreateObject(newSourceFilePath);
        //    obj.name = Path.GetFileNameWithoutExtension(newSourceFilePath);
        //    return obj;
        //}

        //private static List<Vector3> GenerateRandomFragment(MeshFilter source, float sampleRadius = 0.1f)
        //{
        //    var pc = new aus.Geometry.PointCloud(source.mesh);
        //    var randomIndex = Random.Range(0, pc.Count);
        //    var randomPos = pc.Points[randomIndex];
        //    var samples = pc.GetPoints(randomPos, sampleRadius);
        //    return samples;
        //}

        //private static void RandomizeTransform(Transform transform, float minDistance, float maxDistance)
        //{
        //    var distance = Random.Range(minDistance, maxDistance);
        //    transform.localPosition = Random.onUnitSphere * distance;
        //    transform.localRotation = Random.rotation;
        //}

        //private static Matrix4x4 GetRenadomTransformMatrix(float minTranslate, float maxTranslate)
        //{
        //    var distance = Random.Range(minTranslate, maxTranslate);
        //    return Matrix4x4.TRS(Random.onUnitSphere * distance, Random.rotation, Vector3.one);
        //}

        //private static IEnumerable<Vector3> RandomTransform(IEnumerable<Vector3> src, float minTranslate, float maxTranslate)
        //{
        //    var distance = Random.Range(minTranslate, maxTranslate);
        //    var m = Matrix4x4.TRS(Random.onUnitSphere * distance, Random.rotation, Vector3.one);
        //    return MultiplyPoint3x4(m, src);
        //}

        //private static IEnumerable<Vector3> MultiplyPoint3x4(Matrix4x4 transform, IEnumerable<Vector3> points)
        //{
        //    return points.Select(x => transform.MultiplyPoint3x4(x));
        //}
    }
}