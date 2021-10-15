using pp;
using System.Collections.Generic;
using UnityEngine;

public class Sample
    : MonoBehaviour
{
    public MeshFilter sourceModel;
    public Transform worldContainer;

    public int randomSeed = 1234;
    public float initialDistance = 0.8f;

    public float sampleRadius = 0.1f;
    public float pixelSize = 0.002f;

    private GameObject fragment;
    private List<Vector3> samples;
    private mattatz.TransformControl.TransformControl tc;

    // for display text
    public float score = 0.0f;

    private void Start()
    {
        Random.InitState(randomSeed);

        Debug.Assert(sourceModel);
        Debug.Assert(worldContainer);

        OnClick_ResetButton();
    }

    private void Update()
    {
        if (!fragment || !tc) return;
        if (Input.GetKeyDown(KeyCode.Space)) OnClick_ModeButton();

        tc.Control();

        // calculate score ; pos-diff(length) + angle-diff*pos-diff
        var origin = fragment.transform.Find("origin");
        float posDiff = (fragment.transform.position + origin.transform.localPosition).magnitude;
        float angleDiff = Quaternion.Angle(Quaternion.identity, fragment.transform.rotation);
        //Debug.Log($"posDiff = {posDiff:F4}, angleDiff = {angleDiff:F4}");
        score = posDiff + posDiff * angleDiff;
        score = 100 / (score + 1.0f);
    }

    public void OnClick_ResetButton()
    {
        InitializeCamera();

        // 각 point 들의 위치들(위치 차?)를 이용해 차이를 계산해야하므로,
        //   고정된 위치의 vertex 좌표 또는 pointcloud 를 사용하기 애매함. point 들도 변화를 주어야 할 수도 있고..

        var pointcloud = new aus.Geometry.PointCloud(sourceModel.mesh); // zero(pos,rot) 상태에서의 위치들

        Debug.Assert(sourceModel.transform.position == Vector3.zero);

        // sample fragment
        var randomIndex = Random.Range(pointcloud.Count / 4, pointcloud.Count - (pointcloud.Count / 4));
        var randomPos = pointcloud.Points[randomIndex];
        samples = pointcloud.GetPoints(randomPos, sampleRadius);
        Debug.Log($"sample point count = {samples.Count}"); // TODO: 너무 적은 sample 이면 다시

        var pivot = Util.CreateSamplePoints(randomPos, samples, pixelSize);

        // initial random position, rotation
        pivot.transform.position = Random.onUnitSphere * Random.Range(0.2f, 0.4f);
        pivot.transform.rotation = Random.rotation;

        if (fragment) Destroy(fragment);
        fragment = pivot;
        fragment.transform.parent = worldContainer;

        tc = fragment.AddComponent<mattatz.TransformControl.TransformControl>();
        tc.global = true;
        tc.mode = mattatz.TransformControl.TransformControl.TransformMode.Rotate;
        tc.useDistance = true;
        tc.distance = 5.0f;

        Debug.Log($"control pos={fragment.transform.position:F4}, rot={fragment.transform.rotation:F4}");
    }

    public void OnClick_ModeButton()
    {
        if (!fragment || !tc) return;
        if (tc.mode == mattatz.TransformControl.TransformControl.TransformMode.Rotate)
            tc.mode = mattatz.TransformControl.TransformControl.TransformMode.Translate;
        else if (tc.mode == mattatz.TransformControl.TransformControl.TransformMode.Translate)
            tc.mode = mattatz.TransformControl.TransformControl.TransformMode.Rotate;
    }

    private aus.RollbackTransform hintBackup;

    public void OnPointerDown_HintButton()
    {
        if (!fragment) return;
        hintBackup = new aus.RollbackTransform(fragment.transform);
        var origin = fragment.transform.Find("origin");
        fragment.transform.position = -origin.transform.localPosition;
        fragment.transform.rotation = Quaternion.identity;
    }

    public void OnPointerUp_HintButton()
    {
        if (hintBackup == null) return;
        hintBackup.Dispose();
        hintBackup = null;
    }

    private void InitializeCamera()
    {
        var lookTarget = sourceModel.GetComponent<Renderer>().bounds.center; // logic position(mesh or transform) could not be same as visual position
        Util.LookAt(initialDistance, Camera.main.transform, lookTarget);
    }
}