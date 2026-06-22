using System;
using UnityEngine;

namespace nitou {

    /// <summary>
    /// 軸‐角度（オイラーの回転定理）表現の回転を表す構造体．
    /// </summary>
    /// <remarks>
    /// 右手系（<see cref="MatrixUtils"/> の Rx/Ry/Rz と整合）．
    /// <see cref="Axis"/> は単位ベクトル，<see cref="Angle"/> は [rad]．
    /// </remarks>
    public readonly struct AxisAngle : IEquatable<AxisAngle> {

        /// <summary>回転軸（単位ベクトル）．</summary>
        public Vector3 Axis { get; }

        /// <summary>回転角 [rad]．</summary>
        public float Angle { get; }

        // 定数
        public static readonly float Tolerance = 1e-5f;

        /// <summary>
        /// コンストラクタ．軸は自動で正規化する．
        /// </summary>
        public AxisAngle(Vector3 axis, float angle) {
            float m = axis.magnitude;
            Axis = m > 1e-9f ? axis / m : Vector3.forward;
            Angle = angle;
        }

        /// <summary>回転なし（恒等回転）．</summary>
        public static AxisAngle Identity => new AxisAngle(Vector3.forward, 0f);

        /// <summary>
        /// 回転行列に変換する（ロドリゲスの公式）．
        /// </summary>
        public Matrix4x4 ToMatrix() {
            float c = Mathf.Cos(Angle);
            float s = Mathf.Sin(Angle);
            float t = 1f - c;
            float ax = Axis.x, ay = Axis.y, az = Axis.z;

            var mat = Matrix4x4.identity;
            mat.m00 = c + ax * ax * t;
            mat.m01 = ax * ay * t - az * s;
            mat.m02 = ax * az * t + ay * s;

            mat.m10 = ay * ax * t + az * s;
            mat.m11 = c + ay * ay * t;
            mat.m12 = ay * az * t - ax * s;

            mat.m20 = az * ax * t - ay * s;
            mat.m21 = az * ay * t + ax * s;
            mat.m22 = c + az * az * t;
            return mat;
        }

        /// <summary>
        /// クォータニオン (x,y,z,w) に変換する．
        /// </summary>
        public Quaternion ToQuaternion() {
            float h = Angle * 0.5f;
            float s = Mathf.Sin(h);
            return new Quaternion(Axis.x * s, Axis.y * s, Axis.z * s, Mathf.Cos(h));
        }

        /// <summary>
        /// クォータニオン (x,y,z,w) から軸‐角度を取得する．
        /// </summary>
        public static AxisAngle FromQuaternion(Quaternion q) {
            float n = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            if (n < 1e-9f) return Identity;

            float x = q.x / n, y = q.y / n, z = q.z / n, w = q.w / n;
            float v = Mathf.Sqrt(x * x + y * y + z * z);   // = |sin(θ/2)|
            if (v < 1e-9f) return new AxisAngle(Vector3.forward, 0f);

            float angle = 2f * Mathf.Atan2(v, w);
            return new AxisAngle(new Vector3(x, y, z) / v, angle);
        }

        /// <summary>
        /// 回転行列から軸‐角度を取得する．
        /// </summary>
        public static AxisAngle FromMatrix(Matrix4x4 mat) => FromQuaternion(QuaternionUtils.ToQuaternion(mat));

        /// <summary>
        /// 同値判定．同じ回転を表す (axis,θ) と (-axis,-θ) は等価とみなす．
        /// </summary>
        public bool Equals(AxisAngle other) {
            Quaternion a = ToQuaternion();
            Quaternion b = other.ToQuaternion();
            float dot = a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
            return Mathf.Abs(dot) > 1f - Tolerance;   // 符号反転（二重被覆）も許容
        }

        public override bool Equals(object obj) => obj is AxisAngle other && Equals(other);

        public override int GetHashCode() {
            // Equals と整合させるため w >= 0 へ正規化した量子化値で算出
            Quaternion q = ToQuaternion();
            if (q.w < 0f) { q.x = -q.x; q.y = -q.y; q.z = -q.z; q.w = -q.w; }
            const float scale = 1000f;
            int hx = Mathf.RoundToInt(q.x * scale);
            int hy = Mathf.RoundToInt(q.y * scale);
            int hz = Mathf.RoundToInt(q.z * scale);
            int hw = Mathf.RoundToInt(q.w * scale);
            return HashCode.Combine(hx, hy, hz, hw);
        }

        public override string ToString() => $"AxisAngle(axis: {Axis}, angle: {Angle * Mathf.Rad2Deg}deg)";
    }
}
