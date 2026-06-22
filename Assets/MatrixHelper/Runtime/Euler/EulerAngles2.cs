using System;
using UnityEngine;

namespace nitou {

    /// <summary>
    /// ２軸のオイラー角を表す構造体．
    /// </summary>
    public partial struct EulerAngles2 : IEquatable<EulerAngles2> {

        // [NOTE] 計算ではなく値の保持が目的のため、要素は不変とする

        public Type Order { get; }
        public float S1 { get; }
        public float P { get; }
        public float S2 { get; }

        /// <summary>
        /// コンストラクタ．
        /// </summary>
        public EulerAngles2(Type order, float s1, float p, float s2) {
            this.Order = order;
            this.S1 = AngleUtils.NormalizePi(s1);
            this.P = AngleUtils.NormalizePi(p);
            this.S2 = AngleUtils.NormalizePi(s2);
        }

        /// <summary>
        /// ゼロ判定
        /// </summary>
        public bool IsZero() {
            return Mathf.Abs(S1) < EulerAngles.Tolerance
                && Mathf.Abs(P) < EulerAngles.Tolerance
                && Mathf.Abs(S2) < EulerAngles.Tolerance;
        }

        /// <summary>
        /// 同値判定．
        /// </summary>
        public override bool Equals(object obj) {
            return obj is EulerAngles2 other && Equals(other);
        }

        /// <summary>
        /// 同値判定．
        /// </summary>
        public bool Equals(EulerAngles2 other) {
            return Order == other.Order
                && Mathf.Abs(S1 - other.S1) < EulerAngles.Tolerance
                && Mathf.Abs(P - other.P) < EulerAngles.Tolerance
                && Mathf.Abs(S2 - other.S2) < EulerAngles.Tolerance;
        }

        public override string ToString() {
            return $"{Order} ({S1}, {P}, {S2})";
        }

        public string ToStringDeg() {
            return $"{Order} {AngleAtDegree()}";
        }

        public Vector3 AngleAtDegree() => new Vector3 (S1, P, S2) * Mathf.Rad2Deg;


        public EulerAngles2 WithAngles(float s1, float p, float s2) => new(Order, s1, p, s2);
        //public EulerAngles2 WithAnglesS1(float x) => new(Order, x, Y, Z);
        //public EulerAngles2 WithAnglesP(float y) => new(Order, X, y, Z);
        //public EulerAngles2 WithAnglesS2(float z) => new(Order, X, Y, z);

        /// <summary>
        /// 回転行列に変換する．
        /// </summary>
        public Matrix4x4 ToMatrix() {
            return MatrixUtils.FromEulerAngle(Order, S1, P, S2);
        }
    }
}
