using System;
using UnityEngine;

// [REF]
//  qiita: 回転行列、クォータニオン(四元数)、オイラー角の相互変換 https://qiita.com/aa_debdeb/items/3d02e28fb9ebfa357eaf
//  qiita: 行列(Matrix4x4)の世界を垣間見る https://qiita.com/hikoalpha/items/6612c3704c3c9610a08a
//  qiita: オイラー角に潜む5つの罠 https://qiita.com/take4eng/items/0f5a9ff47fd345e5fc33
//  _: 左手系のクォータニオンから右手系のロール・ピッチ・ヨーを求める https://mtkbirdman.com/unity-quaternion-euler


namespace nitou {

    public static class MatrixUtils {

        /// ----------------------------------------------------------------------------
        #region 順変換

        // 主軸周りの回転

        /// <summary>
        /// X軸周りの回転行列を生成．
        /// </summary>
        /// <param name="theta">回転角度 [rad]</param>
        public static Matrix4x4 Rx(float theta) {
            float c = Mathf.Cos(theta);
            float s = Mathf.Sin(theta);

            // |  1  0  0 |
            // |  0  c -s |
            // |  0  s  c |

            var mat = Matrix4x4.identity;
            {
                mat.m00 = 1;
                mat.m10 = 0;
                mat.m20 = 0;

                mat.m01 = 0;
                mat.m11 = c;
                mat.m21 = s;

                mat.m02 = 0;
                mat.m12 = -s;
                mat.m22 = c;
            };
            return mat;
        }

        /// <summary>
        /// Y軸周りの回転行列を生成
        /// </summary>
        /// <param name="theta">回転角度 [rad]</param>
        public static Matrix4x4 Ry(float theta) {
            float c = Mathf.Cos(theta);
            float s = Mathf.Sin(theta);

            // |  c  0  s |
            // |  0  1  0 |
            // | -s  0  c |

            var mat = Matrix4x4.identity;
            {
                mat.m00 = c;
                mat.m10 = 0;
                mat.m20 = -s;

                mat.m01 = 0;
                mat.m11 = 1;
                mat.m21 = 0;

                mat.m02 = s;
                mat.m12 = 0;
                mat.m22 = c;
            };
            return mat;
        }

        /// <summary>
        /// Z軸周りの回転行列を生成
        /// </summary>
        /// <param name="theta">回転角度 [rad]</param>
        public static Matrix4x4 Rz(float theta) {
            float c = Mathf.Cos(theta);
            float s = Mathf.Sin(theta);

            // |  c -s  0 |
            // |  s  c  0 |
            // |  0  0  1 |

            var mat = Matrix4x4.identity;
            {
                mat.m00 = c;
                mat.m10 = s;
                mat.m20 = 0;

                mat.m01 = -s;
                mat.m11 = c;
                mat.m21 = 0;

                mat.m02 = 0;
                mat.m12 = 0;
                mat.m22 = 1;
            };
            return mat;
        }

        // オイラー角

        /// <summary>
        /// オイラー角から回転行列を生成．
        /// </summary>
        /// <param name="x">X`軸周りの回転角 [rad]</param>
        /// <param name="y">Y`軸周りの回転角 [rad]</param>
        /// <param name="z">Z`軸周りの回転角 [rad]</param>
        public static Matrix4x4 FromEulerAngle(EulerAngles.Type type, float x, float y, float z) {
            return type switch {
                // [NOTE] オイラー角では右から次の行列を掛けていく (※固定角は左から)
                EulerAngles.Type.XYZ => Rx(x) * Ry(y) * Rz(z), // X → Y → Z
                EulerAngles.Type.XZY => Rx(x) * Rz(z) * Ry(y), // X → Z → Y
                EulerAngles.Type.YXZ => Ry(y) * Rx(x) * Rz(z), // Y → X → Z
                EulerAngles.Type.YZX => Ry(y) * Rz(z) * Rx(x), // Y → Z → X
                EulerAngles.Type.ZXY => Rz(z) * Rx(x) * Ry(y), // Z → X → Y
                EulerAngles.Type.ZYX => Rz(z) * Ry(y) * Rx(x), // Z → Y → X
                _ => throw new NotImplementedException($"Euler type {type} is not implemented. Please verify the input.")
            };
        }

        /// <summary>
        /// オイラー角から回転行列を生成．
        /// </summary>
        /// <param name="s2">１回目の回転角 [rad]</param>
        /// <param name="p">中央軸の回転角 [rad]</param>
        /// <param name="s1">２回目の回転角 [rad]</param>
        public static Matrix4x4 FromEulerAngle(EulerAngles2.Type type, float s1, float p, float s2) {
            return type switch {
                EulerAngles2.Type.XZX => Rx(s1) * Rz(p) * Rx(s2),
                EulerAngles2.Type.XYX => Rx(s1) * Ry(p) * Rx(s2),
                EulerAngles2.Type.YZY => Ry(s1) * Rz(p) * Ry(s2),
                EulerAngles2.Type.YXY => Ry(s1) * Rx(p) * Ry(s2),
                EulerAngles2.Type.ZXZ => Rz(s1) * Rx(p) * Rz(s2),
                EulerAngles2.Type.ZYZ => Rz(s1) * Ry(p) * Rz(s2),
                _ => throw new NotImplementedException($"Euler type {type} is not implemented. Please verify the input.")
            };
        }

        #endregion


        /// ----------------------------------------------------------------------------
        #region 逆変換

        private const float GimbalLockThreshold = 1e-6f;

        /// <summary>
        /// 回転行列からXYZ-オイラー角を取得する．
        /// </summary>
        /// <param name="mat">XYZオイラー角の回転行列</param>
        /// <returns></returns>
        public static EulerAngles GetEulerAnglesXYZ(Matrix4x4 mat) {

            float phi = Mathf.Asin(Mathf.Clamp(mat.m02, -1.0f, 1.0f));    
            bool isGimbalLock = Mathf.Abs(Mathf.Cos(phi)) < GimbalLockThreshold;

            float theta, psi;
            if (isGimbalLock) {
                theta = Mathf.Atan2(mat.m21, mat.m11);
                psi = 0;
            } else {
                theta = Mathf.Atan2(-mat.m12, mat.m22);
                psi = Mathf.Atan2(-mat.m01, mat.m00);
            }

            return new EulerAngles(EulerAngles.Type.XYZ, theta, phi, psi);
        }

        /// <summary>
        /// 回転行列からZYX-オイラー角を取得する．
        /// </summary>
        /// <param name="mat">ZYXオイラー角の回転行列</param>
        /// <returns></returns>
        public static EulerAngles GetEulerAnglesZYX(Matrix4x4 mat) {

            float phi = Mathf.Asin(Mathf.Clamp(-mat.m20, -1.0f, 1.0f));   // ArcSinの有効範囲[-1,1]に制限
            bool isGimbalLock = Mathf.Abs(Mathf.Cos(phi)) < GimbalLockThreshold;

            float theta, psi;
            if (isGimbalLock) {
                theta = 0;
                psi = Mathf.Atan2(-mat.m01, mat.m11);
            } else {
                theta = Mathf.Atan2(mat.m21, mat.m22);
                psi = Mathf.Atan2(mat.m10, mat.m00);
            }

            return new EulerAngles(EulerAngles.Type.ZYX, theta, phi, psi);
        }

        // -----

        /// <summary>
        /// 回転行列からXZX-オイラー角を取得する．
        /// </summary>
        /// <remarks>R = Rx(s1)・Rz(p)・Rx(s2)</remarks>
        /// <param name="mat">XZXオイラー角の回転行列</param>
        public static EulerAngles2 GetEulerAnglesXZX(Matrix4x4 mat) {
            // p は inner 軸(Z)回りの回転角．m00 = cos(p)
            float p = Mathf.Acos(Mathf.Clamp(mat.m00, -1.0f, 1.0f));

            float s1, s2;
            // sin(p) ≈ 0 のとき s1 と s2 が縮退するため、s2 = 0 として s1 を求める
            if (Mathf.Approximately(p, 0)) {
                s2 = 0;
                s1 = Mathf.Atan2(mat.m21, mat.m11);
            } else if (Mathf.Approximately(p, Mathf.PI)) {
                s2 = 0;
                s1 = Mathf.Atan2(-mat.m21, -mat.m11);
            } else {
                s1 = Mathf.Atan2(mat.m20, mat.m10);
                s2 = Mathf.Atan2(mat.m02, -mat.m01);
            }

            return new EulerAngles2(EulerAngles2.Type.XZX, s1, p, s2);
        }


        /// <summary>
        /// 回転行列からZXZ-オイラー角を取得する．
        /// </summary>
        /// <remarks>R = Rz(s1)・Rx(p)・Rz(s2)</remarks>
        /// <param name="mat">ZXZオイラー角の回転行列</param>
        public static EulerAngles2 GetEulerAnglesZXZ(Matrix4x4 mat) {
            // p は inner 軸(X)回りの回転角．m22 = cos(p)
            float p = Mathf.Acos(Mathf.Clamp(mat.m22, -1.0f, 1.0f));

            float s1, s2;
            // sin(p) ≈ 0 のとき s1 と s2 が縮退するため、s2 = 0 として s1 を求める
            if (Mathf.Approximately(p, 0) || Mathf.Approximately(p, Mathf.PI)) {
                s2 = 0;
                s1 = Mathf.Atan2(mat.m10, mat.m00);
            } else {
                s1 = Mathf.Atan2(mat.m02, -mat.m12);
                s2 = Mathf.Atan2(mat.m20, mat.m21);
            }

            return new EulerAngles2(EulerAngles2.Type.ZXZ, s1, p, s2);
        }

        /// <summary>
        /// 回転行列からZYZ-オイラー角を取得する．
        /// </summary>
        /// <remarks>
        /// 実装は <see cref="FromRotationMatrixZYZ"/> に集約している．
        /// （旧実装は中間角に Acos(m11) を用いた誤りがあったため修正）
        /// </remarks>
        /// <param name="mat">ZYZオイラー角の回転行列</param>
        public static EulerAngles2 GetEulerAnglesZYZ(Matrix4x4 mat) => FromRotationMatrixZYZ(mat);

        /// <summary>
        /// 回転行列からZYZオイラー角を導出します。
        /// </summary>
        /// <param name="mat">回転行列 (4x4)</param>
        /// <returns>ZYZオイラー角 (alpha, beta, gamma)</returns>
        public static EulerAngles2 FromRotationMatrixZYZ(Matrix4x4 mat) {

            // Rz`y`z`(αβγ)
            // = Rα Rβ Rγ
            //   | cαcβcγ-sαsγ -cαcβsγ-sαcβγ cαsβ |
            // = | cαcβcγ+cαsγ -sαcβsγ+cαcγ  sαsβ |
            //   |    -sβcγ         sβsγ      cβ  |

            // m22からβを導出できる．
            // またsinβ≠0（非ジンバルロック）のとき、m02とm12からα、m20とm21からγを導出できる．

            float alpha, beta, gamma;
            beta = Mathf.Acos(mat.m22);

            // 特殊ケースの処理
            // beta = 0 の場合 (Z軸回りの回転のみ)
            if (Mathf.Approximately(beta, 0)) {
                alpha = 0;
                gamma = Mathf.Atan2(mat.m01, mat.m00); // atan2(R12, R11)
            } 
            // beta = pi の場合 (反転)
            else if (Mathf.Approximately(beta, Mathf.PI)) {
                alpha = 0;
                gamma = Mathf.Atan2(-mat.m01, -mat.m00); // atan2(-R12, -R11)
            } 
            

            // 通常ケース
            else {
                alpha = Mathf.Atan2(mat.m12, mat.m02); // arctan(m12, m02)
                gamma = Mathf.Atan2(mat.m21, -mat.m20); // arctan(m21, -m20)
            }

            return new EulerAngles2(EulerAngles2.Type.ZYZ, alpha, beta, gamma);
        }


        #endregion
    }

}