using UnityEngine;

namespace nitou {

    // クォータニオン・軸‐角度との相互変換（右手系．QuaternionUtils と整合）
    public partial struct EulerAngles {

        /// <summary>
        /// クォータニオン (x,y,z,w) に変換する．
        /// </summary>
        public Quaternion ToQuaternion() => QuaternionUtils.ToQuaternion(ToMatrix());

        /// <summary>
        /// 軸‐角度に変換する．
        /// </summary>
        public AxisAngle ToAxisAngle() => AxisAngle.FromMatrix(ToMatrix());

        /// <summary>
        /// クォータニオン (x,y,z,w) から指定順序のオイラー角を取得する．
        /// </summary>
        public static EulerAngles FromQuaternion(Type order, Quaternion q)
            => MatrixUtils.ToEulerAngles(QuaternionUtils.FromQuaternion(q), order);

        /// <summary>
        /// 軸‐角度から指定順序のオイラー角を取得する．
        /// </summary>
        public static EulerAngles FromAxisAngle(Type order, AxisAngle axisAngle)
            => MatrixUtils.ToEulerAngles(axisAngle.ToMatrix(), order);
    }


    // クォータニオン・軸‐角度との相互変換（対称オイラー角）
    public partial struct EulerAngles2 {

        /// <summary>
        /// クォータニオン (x,y,z,w) に変換する．
        /// </summary>
        public Quaternion ToQuaternion() => QuaternionUtils.ToQuaternion(ToMatrix());

        /// <summary>
        /// 軸‐角度に変換する．
        /// </summary>
        public AxisAngle ToAxisAngle() => AxisAngle.FromMatrix(ToMatrix());

        /// <summary>
        /// クォータニオン (x,y,z,w) から指定順序の対称オイラー角を取得する．
        /// </summary>
        public static EulerAngles2 FromQuaternion(Type order, Quaternion q)
            => MatrixUtils.ToEulerAngles(QuaternionUtils.FromQuaternion(q), order);

        /// <summary>
        /// 軸‐角度から指定順序の対称オイラー角を取得する．
        /// </summary>
        public static EulerAngles2 FromAxisAngle(Type order, AxisAngle axisAngle)
            => MatrixUtils.ToEulerAngles(axisAngle.ToMatrix(), order);
    }
}
