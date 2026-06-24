using UnityEngine;

namespace nitou.Samples {

    /// <summary>
    /// MatrixHelper の各回転表現（オイラー角・回転行列・クォータニオン・軸‐角度）の
    /// 相互変換を体験するためのサンプル．
    /// </summary>
    /// <remarks>
    /// シーン上のオブジェクトにアタッチすると、Inspector で指定したオイラー角に対応する
    /// 回転後の座標軸（赤=X, 緑=Y, 青=Z）と、軸‐角度（黄）を Gizmos で描画する．
    /// コンテキストメニューから各表現のログ出力・往復検証も実行できる．
    ///
    /// [NOTE] 本ライブラリは右手系（MatrixUtils.Rx/Ry/Rz と整合）で計算する．
    ///  Unity の Transform/Quaternion（左手系）とは規約が異なるため、ここでの可視化は
    ///  「回転行列が基底ベクトルをどう写すか」を右手系のまま描画している．
    /// </remarks>
    [ExecuteAlways]
    public sealed class RotationConversionSample : MonoBehaviour {

        [Header("入力（非対称オイラー角 i-j-k）")]
        [SerializeField] private EulerAngles.Type _order = EulerAngles.Type.XYZ;

        [Tooltip("各軸まわりの回転角 [degree]")]
        [SerializeField] private Vector3 _eulerDegrees = new Vector3(30f, 45f, 60f);

        [Header("Gizmo 設定")]
        [SerializeField] private float _axisLength = 1.5f;
        [SerializeField] private bool _drawAxisAngle = true;


        /// <summary>現在の入力を表すオイラー角．</summary>
        public EulerAngles Euler => new EulerAngles(_order, _eulerDegrees * Mathf.Deg2Rad);


        /// ----------------------------------------------------------------------------
        #region Gizmo

        private void OnDrawGizmos() {
            var euler = Euler;
            Matrix4x4 m = euler.ToMatrix();
            Vector3 origin = transform.position;

            // 回転後の基底ベクトル（行列が e_x, e_y, e_z をどこへ写すか）
            DrawAxis(origin, m.MultiplyVector(Vector3.right), Color.red);
            DrawAxis(origin, m.MultiplyVector(Vector3.up), Color.green);
            DrawAxis(origin, m.MultiplyVector(Vector3.forward), Color.blue);

            // 軸‐角度（オイラーの回転定理）の回転軸
            if (_drawAxisAngle) {
                AxisAngle aa = euler.ToAxisAngle();
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(origin - aa.Axis * _axisLength, origin + aa.Axis * _axisLength);
            }
        }

        private void DrawAxis(Vector3 origin, Vector3 dir, Color color) {
            Gizmos.color = color;
            Gizmos.DrawLine(origin, origin + dir.normalized * _axisLength);
        }

        #endregion


        /// ----------------------------------------------------------------------------
        #region コンテキストメニュー

        /// <summary>
        /// 入力オイラー角を、全表現へ変換してログ出力する．
        /// </summary>
        [ContextMenu("Log All Representations")]
        public void LogAllRepresentations() {
            var euler = Euler;
            Matrix4x4 m = euler.ToMatrix();
            Quaternion q = euler.ToQuaternion();
            AxisAngle aa = euler.ToAxisAngle();

            Debug.Log(
                $"[RotationConversionSample]\n" +
                $"  Euler({_order})   : {euler.ToStringDeg()}\n" +
                $"  Quaternion       : ({q.x:F4}, {q.y:F4}, {q.z:F4}, {q.w:F4})\n" +
                $"  AxisAngle        : {aa}\n" +
                $"  Matrix(行ベクトル) : \n{FormatMatrix(m)}");
        }

        /// <summary>
        /// 各表現を経由して元のオイラー角（が表す回転）に戻ることを確認する．
        /// </summary>
        [ContextMenu("Verify Round Trip")]
        public void VerifyRoundTrip() {
            var euler = Euler;
            Matrix4x4 m = euler.ToMatrix();

            // Euler → Matrix → Euler
            var viaMatrix = MatrixUtils.ToEulerAngles(m, _order).ToMatrix();
            // Euler → Quaternion → Euler
            var viaQuat = EulerAngles.FromQuaternion(_order, euler.ToQuaternion()).ToMatrix();
            // Euler → AxisAngle → Euler
            var viaAxis = EulerAngles.FromAxisAngle(_order, euler.ToAxisAngle()).ToMatrix();

            Debug.Log(
                $"[RoundTrip] 最大誤差（回転行列比較）\n" +
                $"  via Matrix     : {MaxDiff(m, viaMatrix):E2}\n" +
                $"  via Quaternion : {MaxDiff(m, viaQuat):E2}\n" +
                $"  via AxisAngle  : {MaxDiff(m, viaAxis):E2}");
        }

        /// <summary>
        /// 同一の回転行列を、6種類すべての (i-j-k) 順序で逆変換した結果を出力する．
        /// 順序ごとに角度表現が異なることを確認できる．
        /// </summary>
        [ContextMenu("Log All Orders (same rotation)")]
        public void LogAllOrders() {
            Matrix4x4 m = Euler.ToMatrix();

            var sb = new System.Text.StringBuilder("[AllOrders] 同一回転を各順序で逆変換:\n");
            foreach (EulerAngles.Type order in System.Enum.GetValues(typeof(EulerAngles.Type))) {
                var e = MatrixUtils.ToEulerAngles(m, order);
                sb.AppendLine($"  {order} : {e.AngleAtDegree()}");
            }
            Debug.Log(sb.ToString());
        }

        #endregion


        /// ----------------------------------------------------------------------------
        #region Helper

        private static float MaxDiff(Matrix4x4 a, Matrix4x4 b) {
            float max = 0f;
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    max = Mathf.Max(max, Mathf.Abs(a[r, c] - b[r, c]));
            return max;
        }

        private static string FormatMatrix(Matrix4x4 m) {
            return
                $"    | {m.m00,7:F3} {m.m01,7:F3} {m.m02,7:F3} |\n" +
                $"    | {m.m10,7:F3} {m.m11,7:F3} {m.m12,7:F3} |\n" +
                $"    | {m.m20,7:F3} {m.m21,7:F3} {m.m22,7:F3} |";
        }

        #endregion
    }
}
