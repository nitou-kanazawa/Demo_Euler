using UnityEngine;

namespace nitou {

    /// <summary>
    /// 角度計算の汎用メソッドを提供する静的クラス
    /// </summary>
    public static class AngleUtils {


        // 2つの2Dベクトルの間の角度を計算（度で返す）
        public static float AngleBetweenVectors(Vector2 from, Vector2 to) {
            return Vector2.SignedAngle(from, to);
        }



        /// <summary>
        /// 角度を指定された精度に丸める．
        /// </summary>
        public static float RoundAngle(float angle, float step) {
            return Mathf.Round(angle / step) * step;
        }


        /// ----------------------------------------------------------------------------
        #region Normalize

        /// <summary>
        /// 角度[deg]を 0 ～ 360 の範囲で正規化する．
        /// </summary>
        public static float Normalize360(float angle) {
            // 値域： [0,360) 
            return Mathf.Repeat(angle, 360f);
        }

        /// <summary>
        /// 角度[deg]を −180 ～ 180 の範囲で正規化する．
        /// </summary>
        public static float Normalize180(float angle) {
            // 値域： [-180,180) 
            angle = Mathf.Repeat(angle + 180f, 360f) - 180f;
            return angle;
        }

        /// <summary>
        /// 角度を (−180,180] の範囲で正規化する．
        /// </summary>
        public static Vector3 Normalize180(Vector3 angles) {
            return new Vector3(
                Normalize180(angles.x),
                Normalize180(angles.y),
                Normalize180(angles.z)
            );
        }

        /// <summary>
        /// 角度[deg]を (−180, 180] の範囲で正規化する．
        /// </summary>
        /// <remarks>
        /// <see cref="Normalize180(float)"/> が [−180, 180) なのに対し、
        /// こちらは上限側を含む (−180, 180]（+180 は +180 のまま、−180 は +180 に写る）．
        /// </remarks>
        public static float Normalize180Inclusive(float angle) {
            // 値域： (-180,180]
            angle %= 360f; // 360で剰余
            return angle > 180f ? angle - 360f : (angle <= -180f ? angle + 360f : angle);
        }

        /// <summary>
        /// 角度[rad]を 0 ～ 2π の範囲で正規化する．
        /// </summary>
        public static float Normalize2Pi(float radians) {
            // 値域： [0,2PI) 
            return Mathf.Repeat(radians, Mathf.PI * 2f);
        }

        /// <summary>
        /// 角度[rad]を -π ～ π の範囲で正規化する．
        /// </summary>
        public static float NormalizePi(float radians) {
            // 値域： [-PI,PI) 
            radians = Mathf.Repeat(radians + Mathf.PI, Mathf.PI * 2f) - Mathf.PI;
            return radians;
        }

        #endregion
    }
}
