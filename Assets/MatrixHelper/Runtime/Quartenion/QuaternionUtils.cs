using UnityEngine;

namespace nitou {

    /// <summary>
    /// 回転行列とクォータニオンの相互変換を提供する静的クラス．
    /// </summary>
    /// <remarks>
    /// [NOTE] 本ライブラリの回転は右手系（<see cref="MatrixUtils.Rx"/> /
    /// <see cref="MatrixUtils.Ry"/> / <see cref="MatrixUtils.Rz"/> と整合）で定義する．
    /// 返す <see cref="Quaternion"/> は (x,y,z,w) のデータ容器として扱い，右手系の
    /// 標準式で構成する．Unity の <c>Transform.rotation</c>（左手系）とは規約が異なる
    /// ため，得られたクォータニオンをそのまま transform に代入しないこと．
    /// </remarks>
    public static class QuaternionUtils {

        /// <summary>
        /// クォータニオン (x,y,z,w) から回転行列を生成する．
        /// </summary>
        public static Matrix4x4 FromQuaternion(Quaternion q) {
            float x = q.x, y = q.y, z = q.z, w = q.w;

            var mat = Matrix4x4.identity;
            mat.m00 = 1f - 2f * (y * y + z * z);
            mat.m01 = 2f * (x * y - z * w);
            mat.m02 = 2f * (x * z + y * w);

            mat.m10 = 2f * (x * y + z * w);
            mat.m11 = 1f - 2f * (x * x + z * z);
            mat.m12 = 2f * (y * z - x * w);

            mat.m20 = 2f * (x * z - y * w);
            mat.m21 = 2f * (y * z + x * w);
            mat.m22 = 1f - 2f * (x * x + y * y);
            return mat;
        }

        /// <summary>
        /// 回転行列からクォータニオン (x,y,z,w) を取得する．
        /// </summary>
        /// <remarks>数値的に安定な Shepperd 法（最大成分で分岐）を用いる．</remarks>
        public static Quaternion ToQuaternion(Matrix4x4 mat) {
            float trace = mat.m00 + mat.m11 + mat.m22;
            float x, y, z, w;

            if (trace > 0f) {
                float s = Mathf.Sqrt(trace + 1f) * 2f;          // s = 4w
                w = 0.25f * s;
                x = (mat.m21 - mat.m12) / s;
                y = (mat.m02 - mat.m20) / s;
                z = (mat.m10 - mat.m01) / s;
            } else if (mat.m00 > mat.m11 && mat.m00 > mat.m22) {
                float s = Mathf.Sqrt(1f + mat.m00 - mat.m11 - mat.m22) * 2f; // s = 4x
                w = (mat.m21 - mat.m12) / s;
                x = 0.25f * s;
                y = (mat.m01 + mat.m10) / s;
                z = (mat.m02 + mat.m20) / s;
            } else if (mat.m11 > mat.m22) {
                float s = Mathf.Sqrt(1f + mat.m11 - mat.m00 - mat.m22) * 2f; // s = 4y
                w = (mat.m02 - mat.m20) / s;
                x = (mat.m01 + mat.m10) / s;
                y = 0.25f * s;
                z = (mat.m12 + mat.m21) / s;
            } else {
                float s = Mathf.Sqrt(1f + mat.m22 - mat.m00 - mat.m11) * 2f; // s = 4z
                w = (mat.m10 - mat.m01) / s;
                x = (mat.m02 + mat.m20) / s;
                y = (mat.m12 + mat.m21) / s;
                z = 0.25f * s;
            }
            return new Quaternion(x, y, z, w);
        }
    }
}
