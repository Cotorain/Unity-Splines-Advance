using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class JointInfo
{
    public SplineContainer Spline; //通過するSplineを指定
    public float start; //Spline上の開始位置
    public float end; // Spline上の終了位置
    [HideInInspector] public float cumulativeStart; //Spline上の開始位置を実際の距離に変換した値([0]からの距離)
    [HideInInspector] public float cumulativeEnd; //Spline上の終了位置を実際の距離に変換した値([0]からの距離)
}
[ExecuteAlways]
public class RouteManager : MonoBehaviour
{
    public List<JointInfo> jointInfo = new List<JointInfo>();
    [HideInInspector] public float distance; //合同Spline上の距離
    public Vector3 calcPos { get; private set; } //distanceに対応する外部参照用の位置変数
    public Vector3 calcRot { get; private set; } //distanceに対応する外部参照用の回転変数
    public float SplineLength { get; private set; } //合同Splineの全長
    private SplineContainer referenceSpline;
    private float referenceDistance;
    void Start()
    {
        OnEnable();
    }
    internal void OnEnable()
    {
        float cumulativeLength = 0f;
        for (int i = 0; i < jointInfo.Count; i++)
        {
            JointInfo info = jointInfo[i];
            info.cumulativeStart = cumulativeLength;

            if (info.Spline != null)
            {
                float splineLength = info.Spline.CalculateLength();
                info.start = Mathf.Clamp(info.start, 0f, splineLength);
                info.end = Mathf.Clamp(info.end, 0f, splineLength);
                cumulativeLength += info.end - info.start;
            }
            else
            {
                Debug.LogWarning($"JointInfo[{i}].Spline is null. This joint will be skipped during Update.");
            }

            info.cumulativeEnd = cumulativeLength;
        }
        SplineLength = cumulativeLength; // 合同Splineの全長を計算
    }
    void Update()
    {
        if (jointInfo.Count == 0) return;
        RefreshCalcResult(distance);
    }

    /// <summary>
    /// 指定された距離に対応する位置と回転を計算して、calcPos と calcRot に格納します。
    /// </summary>
    /// <param name="queryDistance">計算対象の距離</param>
    public void RefreshCalcResult(float queryDistance)
    {
        if (jointInfo.Count == 0) return;
        // queryDistanceに対応するSplineを探す
        foreach (var info in jointInfo)
        {
            if (queryDistance >= info.cumulativeStart && queryDistance <= info.cumulativeEnd)
            {
                referenceSpline = info.Spline;
                referenceDistance = info.start + (queryDistance - info.cumulativeStart);
                break;
            }
        }

        // Splineが有効な場合のみ位置と回転を計算
        if (referenceSpline != null)
        {
            Vector3 Pos, Rot;
            SplineAdvanceSystem.CalcSpline(referenceSpline, referenceDistance, out Pos, out Rot);
            calcPos = Pos;
            calcRot = Rot;
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(RouteManager))]
[CanEditMultipleObjects]
internal class RouteManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RouteManager myScript = (RouteManager)target;
        if (GUILayout.Button("Recalculate Route"))
        {
            myScript.OnEnable();
        }
    }
}
#endif