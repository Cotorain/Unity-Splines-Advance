/*----------------------------------------------------------------
    This code used by API of Unity Splines Advance System.
    You don't have to add this script to components of GameObject.
----------------------------------------------------------------*/
using UnityEngine;
//using System.Collections;
using System.Collections.Generic;
using UnityEngine.Splines;
using Unity.Mathematics;

[Tooltip("This code is for APIs. You don't have to attach here.")] //The tooltip might not be necessary:(
public static class SplineAdvanceSystem
{
    /// <summary>
    /// distanceで指定したSplie上の位置を計算し、Transformの位置と回転を出力します。
    /// </summary>
    /// <param name="spline">計算したいSplineを指定します</param>
    /// <param name="distance">Spline上の位置をUnitで入力します</param>
    /// <param name="calcPos">distanceで求めた地点の位置をVector3で返します</param>
    /// <param name="calcRot">distanceで求めた地点の回転をVector3(Euler角)で返します。</param>
    public static void CalcSpline(SplineContainer spline, float distance, out Vector3 calcPos, out Vector3 calcRot)
    {
        if (spline == null)
        {
            Debug.LogError("SplineContainer is null. Cannot calculate spline.");
            calcPos = Vector3.zero;
            calcRot = Vector3.zero;
            return;
        }
        // distance をSpline長に対する割合 t (0..1) に変換して評価する
        float SplineLength = spline.CalculateLength();
        if (SplineLength <= 0f)
        {
            Debug.LogWarning("SplineAdvanceSystem.CalcSpline(): spline length is zero.");
            calcPos = Vector3.zero;
            calcRot = Vector3.zero;
            return;
        }

        float parameterT = distance / SplineLength;

        // パラメーターが範囲外の場合、ループさせる
        if (parameterT > 1f) parameterT = 0f;
        else if (parameterT < 0f) parameterT = 1f;
        parameterT = math.clamp(parameterT, 0f, 1f);

        float3 pos;
        float3 tangent;
        float3 up;

        spline.Evaluate(parameterT, out pos, out tangent, out up);

        calcPos = (Vector3)pos;

        Quaternion quat = Quaternion.LookRotation((Vector3)math.normalize(tangent), (Vector3)up);
        calcRot = quat.eulerAngles;
    }

    /// <summary>
    /// CalcSpline()を使用し、distanceで指定したSpline上の位置にオブジェクトの位置と回転を適応します。
    /// </summary>
    /// <param name="spline">オブジェクトを配置したいSplineを指定します</param>
    /// <param name="targetObj">配置を適応したいオブジェクトを指定します</param>
    /// <param name="distance">Spline上の位置をUnitで入力します</param>
    public static void SetObj(SplineContainer spline, GameObject targetObj, float distance)
    {
        if (spline == null || targetObj == null)
        {
            Debug.LogError("Invalid argument. Cannot set object.");
            return;
        }

        CalcSpline(spline, distance, out Vector3 pos, out Vector3 rot);

        targetObj.transform.position = pos;
        targetObj.transform.eulerAngles = rot;
    }

    /// <summary>
    /// distanceで指定したSpline上の位置から、offsetで指定した位置へのオフセット位置を計算し、近似点Transformの位置と回転を出力します。
    /// 注意：offsetで指定した座標はあくまで近似点であり、実際にdistance上の点から直線にoffsetの分だけ離したSpline上の位置ではありません。
    /// 近似点の計算方法：distanceとoffset(distance + offset)でそれぞれSpline上の位置を計算し、その2点を結ぶベクトル方向にdistanceを基準としてoffset分だけ離した点を近似点としています。
    /// その際、回転はoffsetで指定した位置のSpline上の回転をそのまま使用します。
    /// offsetの値が0の時の処理も考えましたが、(distance + Offset)の時点でdistanceの位置と同じになるため、特別な処理は行いません。
    /// </summary>
    /// <param name="spline">オブジェクトを配置したいSplineを指定します</param>
    /// <param name="distance">Spline上の位置（offsetの原点）をUnitで入力します</param>
    /// <param name="offset">distanceから離したい距離を入力します。値が負の時はdistanceから後方に、正の時は前方になります。</param>
    /// <param name="calcPos">近似点の座標を返します。</param>
    /// <param name="calcRot">近似点の回転をVector3(Euler角)で返します。</param>
    public static void GetOffsetOnSpline(SplineContainer spline, float distance, float offset, out Vector3 calcPos, out Vector3 calcRot)
    {
        if (spline == null)
        {
            Debug.LogError("SplineContainer is null. Cannot calculate offset on spline.");
            calcPos = Vector3.zero;
            calcRot = Vector3.zero;
            return;
        }
        CalcSpline(spline, distance, out Vector3 originPos, out Vector3 originRot);
        CalcSpline(spline, distance + offset, out Vector3 nearestPosOnSpline, out Vector3 nearestRotOnSpline);

        Vector3 nearestPointPos = (nearestPosOnSpline - originPos).normalized;
        // distance から offset 分だけ離れた点を近似点とする
        calcPos = originPos + nearestPointPos * offset;
        calcRot = nearestRotOnSpline;
    }
    /// <summary>
    /// distanceで指定したSpline上の位置から、その位置に対応する‰(勾配)値を出力します。
    /// </summary>
    /// <param name="spline">勾配を取得したいSplineを指定します。</param>
    /// <param name="distance">勾配を取得したいSpline上の位置をUnitで入力します</param>
    /// <param name="parmill">勾配を‰(パーミル)で返します。</param>
    public static void GetIncrineParmill(SplineContainer spline, float distance, out float parmill)
    {
        if (spline == null)
        {
            Debug.LogError("SplineContainer is null. Cannot calculate offset on spline.");
            distance = 0f;
            parmill = 0f;
            return;
        }
        CalcSpline(spline, distance, out Vector3 originPos, out Vector3 originRot);
        CalcSpline(spline, distance + 0.001f, out Vector3 nextPos, out Vector3 nextRot);
        float heightDiff = nextPos.y - originPos.y;
        parmill = heightDiff * 1000000f;//‰を求めるために0.001unit精度では1000000倍する。
    }
}
