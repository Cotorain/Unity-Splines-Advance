/*----------------------------------------------------------------
    This code extends the functionality of Unity-Splines-Advance.
    Please apply this script to the spline you want to arrange objects on. It will not work if applied to an object.
    Mesh deformation functionality is currently under development.
----------------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Splines;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 配置するオブジェクトの各自設定項目
/// </summary>
[System.Serializable]
public class InstanceSetting
{
    [InspectorName("Prefab")] public GameObject prefab;
    [InspectorName("Mesh Deform (β-Version)")] public bool MeshDeform = false;
    [InspectorName("Installation interval")] public float InstallationInterval = 1.0f;
    [InspectorName("Transform correction")] public Vector3 TransformCorrection = Vector3.zero;
    [InspectorName("Rotation correction (Euler)")] public Vector3 RotationCorrection = Vector3.zero;
    [InspectorName("Installation starting point")] public float InstallationStartingPoint = 0f;
    [InspectorName("Installation ending point")] public float InstallationEndingPoint = 10f;
    [InspectorName("Instantiate Parent Object")] public GameObject InstantiateParentObject = null;
    [InspectorName("Forward Axis")] public SplineAdvanceInstantiate.ForwardAxis forwardAxis = SplineAdvanceInstantiate.ForwardAxis.Z;
}

[ExecuteInEditMode] //エディター上でも常時実行していることを忘れないこと
public class SplineAdvanceInstantiate : MonoBehaviour
{
    /// <summary>
    /// 前方軸を指定
    /// </summary>
    public enum ForwardAxis
    {
        [InspectorName("Object X+")] X,
        [InspectorName("Object Z+")] Z
    }

    private SplineContainer splineContainer;
    [SerializeField][InspectorName("Spline Length")] private float splineLength;
    [SerializeField][InspectorName("GameObject List and Settings")] private List<InstanceSetting> instanceSettings = new List<InstanceSetting>();
    private List<GameObject> instantiatedObjects = new List<GameObject>(); // Instantiateしたオブジェクトのリスト

    /// <summary>
    /// このスクリプトのアタッチ時にSplineContainerを取得し、なければエラーログを出力
    /// </summary>
    private void OnEnable()
    {
        splineContainer = GetComponent<SplineContainer>();
        if (splineContainer == null)
        {
            Debug.LogError("Attach this script to the GameObject with SplineContainer.");
            return;
        }
    }
    /*
      #########################################################
      #  DANGER: MUST NOT INCREASE PROSESSINGS IN Update()!   #
      #  OTHERWISE YOUR COMPUTER WILL SURELY DIE.             #
      #########################################################
    */
    private void Update()
    {
        // Spline長を常に最新に保つ
        if (splineContainer == null) return;
        splineLength = splineContainer.CalculateLength();
    }
    /*
      #########################################################
      #  DANGER: MUST NOT INCREASE PROSESSINGS IN Update()!   #
      #  OTHERWISE YOUR COMPUTER WILL SURELY DIE.             #
      #########################################################
    */

    /// <summary>
    /// Spline上にオブジェクトを配置。
    /// </summary>
    internal void InstanceObj()
    {
        // オブジェクトの重複を防ぐため、いったんInstantiate済みのオブジェクトを削除
        ClearInstanceObjects();
        if (splineContainer == null)
        {
            Debug.LogError("SplineContainer is null. Cannot instance objects.");
            return;
        }
        for (int i = 0; i < instanceSettings.Count; i++)
        {
            InstanceSetting setting = instanceSettings[i];

            // Null check for prefab
            if (setting.prefab == null)
            {
                Debug.LogWarning($"InstanceSetting[{i}]: Prefab is null. Skipping this setting.");
                continue;
            }

            // Validate InstantiateParentObject - required
            if (setting.InstantiateParentObject == null)
            {
                Debug.LogError($"InstanceSetting[{i}]: InstantiateParentObject is null. This setting must have a parent object assigned. Skipping this setting.");
                continue;
            }

            // Validate InstallationInterval to prevent infinite loops
            if (setting.InstallationInterval <= 0)
            {
                Debug.LogError($"InstanceSetting[{i}]: InstallationInterval must be greater than 0. Current value: {setting.InstallationInterval}. Skipping this setting.");
                continue;
            }

            if (instanceSettings[i].InstallationEndingPoint > splineLength || instanceSettings[i].InstallationEndingPoint <= 0)
            {
                instanceSettings[i].InstallationEndingPoint = splineLength;
                Debug.LogWarning("InstallationEndingPoint is out of spline length. Set to spline length automatically.");
            }

            int instanceCount = 0;
            for (float a = setting.InstallationStartingPoint; a <= setting.InstallationEndingPoint + 0.0001f; a += setting.InstallationInterval)
            {
                // Safety check: prevent infinite loops by limiting instance count
                instanceCount++;
                if (instanceCount > 100000)
                {
                    Debug.LogError($"InstanceSetting[{i}]: Too many instances created. Possible infinite loop detected. Breaking loop.");
                    break;
                }

                // 'a' is a InstallationInterval along the spline in world units. Use InstallationInterval-based API.
                float InstallationIntervalAlong = a;
                SplineAdvanceSystem.CalcSpline(splineContainer, InstallationIntervalAlong, out Vector3 objPos, out Vector3 objRotEuler);// Calculate position and rotation (Euler angles)

                // Convert Euler angles to Quaternion for instantiation
                Quaternion objRot = Quaternion.Euler(objRotEuler);
                // Instantiate at spline position/rotation (apply positional/rotational corrections after deformation)
                GameObject obj = Instantiate(setting.prefab, objPos, objRot);

                // Set parent object (already validated as non-null)
                obj.transform.SetParent(setting.InstantiateParentObject.transform, true);

                // Align forward axis before deformation
                if (setting.forwardAxis == ForwardAxis.X)
                {
                    obj.transform.Rotate(0f, -90f, 0f, Space.Self);
                }

                // Apply mesh deformation along spline if requested
                if (setting.MeshDeform)
                {
                    MeshDeform(obj, splineContainer, InstallationIntervalAlong);
                }

                // Apply rotation correction (Euler degrees)
                if (setting.RotationCorrection != Vector3.zero)
                {
                    obj.transform.Rotate(setting.RotationCorrection, Space.Self);
                }

                // Apply position offset after deformation and rotation corrections
                if (setting.TransformCorrection != Vector3.zero)
                {
                    obj.transform.position += setting.TransformCorrection;
                }

                instantiatedObjects.Add(obj);// Add to instantiated objects list
            }
        }
    }

    /// <summary>
    /// Instantiateしたオブジェクトをすべて削除します。
    /// もしinstantiatedObjects リストが失われた場合のフォールバックとして、
    /// 各設定された InstantiateParentObject の下にある、Instantiateされたと思われる子オブジェクトも削除します。
    /// </summary>
    internal void ClearInstanceObjects()
    {
        bool anyParentForCleanup = false;
        if (instanceSettings != null)
        {
            for (int idx = 0; idx < instanceSettings.Count; idx++)
            {
                var s = instanceSettings[idx];
                if (s != null && s.InstantiateParentObject != null)
                {
                    anyParentForCleanup = true;
                    break;
                }
            }
        }
        if (!anyParentForCleanup)
        {
            Debug.LogWarning("No InstantiateParentObject set in instance settings. Cannot perform fallback cleanup.");
        }
        if (instantiatedObjects == null) instantiatedObjects = new List<GameObject>();
        foreach (GameObject obj in instantiatedObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        instantiatedObjects.Clear();

        // Fallback cleanup: if instantiatedObjects list was lost (e.g. domain reload),
        // attempt to remove children under each configured InstantiateParentObject that look like
        // instantiated prefab clones. This helps avoid orphaned objects when the
        // internal list is not available.
        foreach (var setting in instanceSettings)
        {
            if (setting == null || setting.InstantiateParentObject == null) continue;
            Transform parent = setting.InstantiateParentObject.transform;
            if (parent == null) continue;

            string prefabName = setting.prefab != null ? setting.prefab.name : null;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                if (child == null) continue;

                // If we know the prefab name, only remove children that look like that prefab's clone.
                if (!string.IsNullOrEmpty(prefabName))
                {
                    if (child.name.StartsWith(prefabName) && child.name.Contains("(Clone)"))
                    {
                        DestroyImmediate(child);
                    }
                }
                else
                {
                    // If prefab name unknown, remove generic clones to avoid orphaned clones.
                    if (child.name.Contains("(Clone)"))
                    {
                        DestroyImmediate(child);
                    }
                }
            }
        }
    }

    /// <summary>
    /// メッシュをSplineに沿って変形します。
    /// 共有アセットを変更しないようにメッシュの新しいインスタンスを作成します。
    /// Works best for meshes whose length axis is local Z.
    /// メッシュ変形は自分の能力不足であったため、AIに頼りました:(
    /// </summary>
    /// <param name="obj">メッシュの変形をするInstantiateされたオブジェクト</param>
    /// <param name="spline">変形を沿わせるSplineを指定</param>
    /// <param name="centreInstallationInterval">メッシュの中心を配置するSpline上の距離（ワールド単位）</param>
    private void MeshDeform(GameObject obj, SplineContainer spline, float centreInstallationInterval)
    {
        if (obj == null || spline == null) return;

        MeshFilter mf = obj.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;

        float splineLength = spline.CalculateLength();
        if (splineLength <= 0f) return;

        Mesh src = mf.sharedMesh;
        Mesh mesh = Instantiate(src);
        mf.sharedMesh = mesh;

        Vector3[] verts = mesh.vertices;
        if (verts == null || verts.Length == 0) return;

        // Determine bounds along local Z
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;
        for (int i = 0; i < verts.Length; i++)
        {
            float z = verts[i].z;
            if (z < minZ) minZ = z;
            if (z > maxZ) maxZ = z;
        }

        // For each vertex, map its local Z to a point on the spline
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 v = verts[i];

            // 中心距離 + メッシュZオフセット（距離ベース）
            float sampleInstallationInterval = centreInstallationInterval + (v.z - minZ);

            // System仕様（1超過→0）を回避
            sampleInstallationInterval = Mathf.Clamp(sampleInstallationInterval, 0f, splineLength);

            SplineAdvanceSystem.CalcSpline(spline, sampleInstallationInterval, out Vector3 p, out Vector3 rEuler);

            Quaternion r = Quaternion.Euler(rEuler);

            Vector3 lateral = r * new Vector3(v.x, v.y, 0f);
            Vector3 worldPos = p + lateral;

            verts[i] = obj.transform.InverseTransformPoint(worldPos);
        }

        mesh.vertices = verts;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SplineAdvanceInstantiate))]
internal class SplineAdvanceInstantiateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SplineAdvanceInstantiate myScript = (SplineAdvanceInstantiate)target;
        if (GUILayout.Button("Instance Objects"))
        {
            myScript.InstanceObj();
        }
        if (GUILayout.Button("Clear Instance Objects"))
        {
            myScript.ClearInstanceObjects();
        }
    }
}
#endif