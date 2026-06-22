using System;
using UnityEngine;

public sealed class ToolPosture {

    /// <summary>
    /// 
    /// </summary>
    public Vector3 ToolAxis { get; }

    /// <summary>
    /// 
    /// </summary>
    public float AxisRotationAngle { get; }

    public static Vector3 BaseAxis => Vector3.up;


    /// <summary>
    /// コンストラクタ（工具軸と軸周りの回転角度を指定して生成）
    /// </summary>
    /// <param name="toolAxis">工具軸方向（正規化される）</param>
    /// <param name="axisRotationAngle">軸周りの回転角度（度数法）</param>
    public ToolPosture(Vector3 toolAxis, float axisRotationAngle) {
        if (toolAxis == Vector3.zero)
            throw new ArgumentException("工具軸方向はゼロベクトルにできません。");

        ToolAxis = toolAxis.normalized;
        AxisRotationAngle = axisRotationAngle;
    }

    public override string ToString() {
        return $"Axis: {ToolAxis}, Angle {AxisRotationAngle}";
    }

    /// <summary>
    /// YXYオイラー角への変換
    /// </summary>
    public (float phi1, float theta, float phi2) ToYXYEuler() {
        // 傾斜角 (BaseAxis と ToolAxis の間の角度)
        float theta = Vector3.Angle(BaseAxis, ToolAxis);

        // XZ平面への射影
        Vector3 projected = new Vector3(ToolAxis.x, 0, ToolAxis.z);

        // 第一旋回角 (XZ平面のX軸基準)
        float phi1 = Mathf.Atan2(projected.z, projected.x) * Mathf.Rad2Deg;

        // 第二旋回角 (工具軸周りの回転)
        float phi2 = AxisRotationAngle;

        return (phi1, theta, phi2);
    }

    /// <summary>
    /// YZYオイラー角への変換
    /// </summary>
    public (float psi, float theta, float phi) ToYZYEuler() {
        // 傾斜角 (BaseAxis と ToolAxis の間の角度)
        float theta = Vector3.Angle(BaseAxis, ToolAxis);

        // XZ平面への射影
        Vector3 projected = new Vector3(ToolAxis.x, 0, ToolAxis.z);

        // 第一旋回角 (XZ平面のZ軸基準)
        float psi = Mathf.Atan2(projected.x, projected.z) * Mathf.Rad2Deg;

        // 第二旋回角 (工具軸周りの回転)
        float phi = AxisRotationAngle;

        return (psi, theta, phi);
    }

    /// <summary>
    /// 工具姿勢をXYZオイラー角に変換
    /// </summary>
    /// <returns>XYZ順序のオイラー角（度数法）</returns>
    public Vector3 ToXYZEuler() {
        // 工具軸方向を表現する回転
        Quaternion directionRotation = Quaternion.FromToRotation(BaseAxis, ToolAxis);

        // 工具軸周りの回転
        Quaternion axisRotation = Quaternion.AngleAxis(AxisRotationAngle, ToolAxis);

        // 最終的なクォータニオン（方向と軸周りの回転を合成）
        Quaternion combinedRotation = directionRotation * axisRotation;

        // XYZオイラー角に変換
        return combinedRotation.eulerAngles;
    }


    #region Static

    /// <summary>
    /// XYZオイラー角からToolPostureを生成（AxisRotationAngleも考慮）
    /// </summary>
    public static ToolPosture FromEulerAngles(Vector3 eulerAngles) {
        // オイラー角からクォータニオンを生成
        Quaternion rotation = Quaternion.Euler(eulerAngles);

        // 工具軸方向を計算（基準軸を回転）
        Vector3 toolAxis = rotation * BaseAxis;

        // 工具軸周りの回転角度を計算
        // 回転軸が工具軸と一致する部分を取り出す
        Quaternion directionRotation = Quaternion.FromToRotation(BaseAxis, toolAxis);
        Quaternion axisRotation = Quaternion.Inverse(directionRotation) * rotation;

        // ツール軸周りの角度
        float axisRotationAngle = 2f * Mathf.Acos(axisRotation.w) * Mathf.Rad2Deg;

        // 符号を考慮した角度補正（軸回転方向に従う）
        if (axisRotationAngle > 180f) axisRotationAngle -= 360f;

        return new ToolPosture(toolAxis, axisRotationAngle);
    }
    #endregion
}
