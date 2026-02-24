using UnityEngine;
using UnityEngine.Splines;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SplineGuide : MonoBehaviour
{
    [SerializeField] private SplineContainer spline;
    [SerializeField] private float distance = 0f;
    [SerializeField] private float SplineLength = 0f;
    [SerializeField] private bool IsFork = false;
    public RouteManager routeManager;
    void Update()
    {
        if (!spline) return;
        if (!IsFork)
        {
            SplineLength = spline.CalculateLength();
            distance = Mathf.Clamp(distance, 0f, SplineLength);
            SplineAdvanceSystem.SetObj(spline, gameObject, distance);
        }
        else
        {
            SplineLength = routeManager.SplineLength;
            distance = Mathf.Clamp(distance, 0f, routeManager.SplineLength);
            if (!routeManager) return;
            routeManager.distance = distance;
            gameObject.transform.position = routeManager.calcPos;
            gameObject.transform.eulerAngles = routeManager.calcRot;
        }
    }
}
