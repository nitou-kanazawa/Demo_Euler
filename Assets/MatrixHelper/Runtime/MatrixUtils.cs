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

        /// <summary>
        /// 回転行列からXZY-オイラー角を取得する．
        /// </summary>
        /// <remarks>R = Rx・Rz・Ry</remarks>
        public static EulerAngles GetEulerAnglesXZY(Matrix4x4 mat) {
            float b = Mathf.Asin(Mathf.Clamp(-mat.m01, -1.0f, 1.0f));   // 中間角(Z)
            float a, g;                                                 // a:X回転, g:Y回転
            if (Mathf.Abs(Mathf.Cos(b)) < GimbalLockThreshold) {
                g = 0;
                a = mat.m01 < 0 ? Mathf.Atan2(mat.m20, mat.m10) : Mathf.Atan2(-mat.m20, -mat.m10);
            } else {
                a = Mathf.Atan2(mat.m21, mat.m11);
                g = Mathf.Atan2(mat.m02, mat.m00);
            }
            return new EulerAngles(EulerAngles.Type.XZY, a, g, b);      // (X,Y,Z)=(a,g,b)
        }

        /// <summary>
        /// 回転行列からYXZ-オイラー角を取得する．
        /// </summary>
        /// <remarks>R = Ry・Rx・Rz</remarks>
        public static EulerAngles GetEulerAnglesYXZ(Matrix4x4 mat) {
            float b = Mathf.Asin(Mathf.Clamp(-mat.m12, -1.0f, 1.0f));   // 中間角(X)
            float a, g;                                                 // a:Y回転, g:Z回転
            if (Mathf.Abs(Mathf.Cos(b)) < GimbalLockThreshold) {
                g = 0;
                a = Mathf.Atan2(-mat.m20, mat.m00);
            } else {
                a = Mathf.Atan2(mat.m02, mat.m22);
                g = Mathf.Atan2(mat.m10, mat.m11);
            }
            return new EulerAngles(EulerAngles.Type.YXZ, b, a, g);      // (X,Y,Z)=(b,a,g)
        }

        /// <summary>
        /// 回転行列からYZX-オイラー角を取得する．
        /// </summary>
        /// <remarks>R = Ry・Rz・Rx</remarks>
        public static EulerAngles GetEulerAnglesYZX(Matrix4x4 mat) {
            float b = Mathf.Asin(Mathf.Clamp(mat.m10, -1.0f, 1.0f));    // 中間角(Z)
            float a, g;                                                 // a:Y回転, g:X回転
            if (Mathf.Abs(Mathf.Cos(b)) < GimbalLockThreshold) {
                g = 0;
                a = Mathf.Atan2(mat.m02, mat.m22);
            } else {
                a = Mathf.Atan2(-mat.m20, mat.m00);
                g = Mathf.Atan2(-mat.m12, mat.m11);
            }
            return new EulerAngles(EulerAngles.Type.YZX, g, a, b);      // (X,Y,Z)=(g,a,b)
        }

        /// <summary>
        /// 回転行列からZXY-オイラー角を取得する．
        /// </summary>
        /// <remarks>R = Rz・Rx・Ry</remarks>
        public static EulerAngles GetEulerAnglesZXY(Matrix4x4 mat) {
            float b = Mathf.Asin(Mathf.Clamp(mat.m21, -1.0f, 1.0f));    // 中間角(X)
            float a, g;                                                 // a:Z回転, g:Y回転
            if (Mathf.Abs(Mathf.Cos(b)) < GimbalLockThreshold) {
                g = 0;
                a = Mathf.Atan2(mat.m10, mat.m00);
            } else {
                a = Mathf.Atan2(-mat.m01, mat.m11);
                g = Mathf.Atan2(-mat.m20, mat.m22);
            }
            return new EulerAngles(EulerAngles.Type.ZXY, b, g, a);      // (X,Y,Z)=(b,g,a)
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
        /// 回転行列からXYX-オイラー角を取得する．
        /// </summary>
        /// <remarks>R = Rx(s1)・Ry(p)・Rx(s2)</remarks>
        public static EulerAngles2 GetEulerAnglesXYX(Matrix4x4 mat) {
            float p = Mathf.Acos(Mathf.Clamp(mat.m00, -1.0f, 1.0f));   // inner 軸(Y)
            float s1, s2;
            if (Mathf.Approximately(p, 0)) {
                s2 = 0;
                s1 = Mathf.Atan2(-mat.m12, mat.m22);
            } else if (Mathf.Approximately(p, Mathf.PI)) {
                s2 = 0;
                s1 = Mathf.Atan2(mat.m12, -mat.m22);
            } else {
                s1 = Mathf.Atan2(mat.m10, -mat.m20);
                s2 = Mathf.Atan2(mat.m01, mat.m02);
            }
            return new EulerAngles2(EulerAngles2.Type.XYX, s1, p, s2);
        }

        /// <summary>
        /// 回転行列からYZY-オイラー角を取得する．
        /// </summary>
        /// <remarks>R = Ry(s1)・Rz(p)・Ry(s2)</remarks>
        public static EulerAngles2 GetEulerAnglesYZY(Matrix4x4 mat) {
            float p = Mathf.Acos(Mathf.Clamp(mat.m11, -1.0f, 1.0f));   // inner 軸(Z)
            float s1, s2;
            if (Mathf.Approximately(p, 0)) {
                s2 = 0;
                s1 = Mathf.Atan2(mat.m02, mat.m00);
            } else if (Mathf.Approximately(p, Mathf.PI)) {
                s2 = 0;
                s1 = Mathf.Atan2(mat.m02, -mat.m00);
            } else {
                s1 = Mathf.Atan2(mat.m21, -mat.m01);
                s2 = Mathf.Atan2(mat.m12, mat.m10);
            }
            return new EulerAngles2(EulerAngles2.Type.YZY, s1, p, s2);
        }

        /// <summary>
        /// 回転行列からYXY-オイラー角を取得する．
        /// </summary>
        /// <remarks>R = Ry(s1)・Rx(p)・Ry(s2)</remarks>
        public static EulerAngles2 GetEulerAnglesYXY(Matrix4x4 mat) {
            float p = Mathf.Acos(Mathf.Clamp(mat.m11, -1.0f, 1.0f));   // inner 軸(X)
            float s1, s2;
            if (Mathf.Approximately(p, 0)) {
                s2 = 0;
                s1 = Mathf.Atan2(mat.m02, mat.m00);
            } else if (Mathf.Approximately(p, Mathf.PI)) {
                s2 = 0;
                s1 = Mathf.Atan2(-mat.m02, mat.m00);
            } else {
                s1 = Mathf.Atan2(mat.m01, mat.m21);
                s2 = Mathf.Atan2(mat.m10, -mat.m12);
            }
            return new EulerAngles2(EulerAngles2.Type.YXY, s1, p, s2);
        }

        /// <summary>
        /// 回転行列からZYZ-オイラー角を取得する．
        /// </summary>
        /// <remarks>R = Rz(s1)・Ry(p)・Rz(s2)</remarks>
        /// <param name="mat">ZYZオイラー角の回転行列</param>
        public static EulerAngles2 GetEulerAnglesZYZ(Matrix4x4 mat) {
            float p = Mathf.Acos(Mathf.Clamp(mat.m22, -1.0f, 1.0f));   // inner 軸(Y)
            float s1, s2;
            // sin(p) ≈ 0 のとき s1 と s2 が縮退するため、s2 = 0 として s1 を求める
            if (Mathf.Approximately(p, 0)) {
                s2 = 0;
                s1 = Mathf.Atan2(mat.m10, mat.m00);
            } else if (Mathf.Approximately(p, Mathf.PI)) {
                s2 = 0;
                s1 = Mathf.Atan2(-mat.m10, -mat.m00);
            } else {
                s1 = Mathf.Atan2(mat.m12, mat.m02);
                s2 = Mathf.Atan2(mat.m21, -mat.m20);
            }
            return new EulerAngles2(EulerAngles2.Type.ZYZ, s1, p, s2);
        }

        /// <summary>
        /// 回転行列からZYZオイラー角を導出する（<see cref="GetEulerAnglesZYZ"/> の別名）．
        /// </summary>
        /// <param name="mat">回転行列 (4x4)</param>
        public static EulerAngles2 FromRotationMatrixZYZ(Matrix4x4 mat) => GetEulerAnglesZYZ(mat);

        // -----

        /// <summary>
        /// 回転行列から、指定した順序の (i-j-k) オイラー角を取得する．
        /// </summary>
        /// <param name="mat">回転行列</param>
        /// <param name="order">オイラー角の順序</param>
        public static EulerAngles ToEulerAngles(Matrix4x4 mat, EulerAngles.Type order) {
            return order switch {
                EulerAngles.Type.XYZ => GetEulerAnglesXYZ(mat),
                EulerAngles.Type.XZY => GetEulerAnglesXZY(mat),
                EulerAngles.Type.YXZ => GetEulerAnglesYXZ(mat),
                EulerAngles.Type.YZX => GetEulerAnglesYZX(mat),
                EulerAngles.Type.ZXY => GetEulerAnglesZXY(mat),
                EulerAngles.Type.ZYX => GetEulerAnglesZYX(mat),
                _ => throw new NotImplementedException($"Euler type {order} is not implemented.")
            };
        }

        /// <summary>
        /// 回転行列から、指定した順序の (i-j-i) 対称オイラー角を取得する．
        /// </summary>
        /// <param name="mat">回転行列</param>
        /// <param name="order">対称オイラー角の順序</param>
        public static EulerAngles2 ToEulerAngles(Matrix4x4 mat, EulerAngles2.Type order) {
            return order switch {
                EulerAngles2.Type.XZX => GetEulerAnglesXZX(mat),
                EulerAngles2.Type.XYX => GetEulerAnglesXYX(mat),
                EulerAngles2.Type.YZY => GetEulerAnglesYZY(mat),
                EulerAngles2.Type.YXY => GetEulerAnglesYXY(mat),
                EulerAngles2.Type.ZXZ => GetEulerAnglesZXZ(mat),
                EulerAngles2.Type.ZYZ => GetEulerAnglesZYZ(mat),
                _ => throw new NotImplementedException($"Euler type {order} is not implemented.")
            };
        }


        #endregion
    }

}