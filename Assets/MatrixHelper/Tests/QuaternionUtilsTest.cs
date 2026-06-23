using NUnit.Framework;
using UnityEngine;

namespace nitou.Tests {

    public class QuaternionUtilsTest {
        private const float Threshold = 1e-3f;

        // 回転行列の回転成分(3x3)が一致することを検証する
        private static void AssertRotationEqual(Matrix4x4 a, Matrix4x4 b, string label) {
            for (int r = 0; r < 3; r++) {
                for (int c = 0; c < 3; c++) {
                    Assert.That(b[r, c], Is.EqualTo(a[r, c]).Within(Threshold), $"{label}: m{r}{c} 不一致");
                }
            }
        }

        // 検証用の代表的な回転（順序と角度）
        private static Matrix4x4[] SampleMatrices() {
            var orders = (EulerAngles.Type[])System.Enum.GetValues(typeof(EulerAngles.Type));
            var angles = new[] {
                new Vector3(0, 0, 0),    new Vector3(30, 40, 50),  new Vector3(-20, 80, 140),
                new Vector3(90, 0, 0),   new Vector3(0, 90, 0),    new Vector3(0, 0, 90),
                new Vector3(170, 10, -160), new Vector3(45, -45, 45),
            };
            var list = new System.Collections.Generic.List<Matrix4x4>();
            foreach (var o in orders)
                foreach (var a in angles)
                    list.Add(new EulerAngles(o, a * Mathf.Deg2Rad).ToMatrix());
            return list.ToArray();
        }


        #region 回転行列 ↔ クォータニオン

        [Test]
        public void 回転行列がクォータニオンを介して復元されること() {
            foreach (var mat in SampleMatrices()) {
                var q = QuaternionUtils.ToQuaternion(mat);
                var restored = QuaternionUtils.FromQuaternion(q);
                AssertRotationEqual(mat, restored, "mat→quat→mat");
            }
        }

        [Test]
        public void 単位クォータニオンが恒等行列を生成すること() {
            var mat = QuaternionUtils.FromQuaternion(new Quaternion(0, 0, 0, 1));
            AssertRotationEqual(Matrix4x4.identity, mat, "identity");
        }

        #endregion


        #region オイラー角 ↔ クォータニオン

        [Test]
        public void オイラー角がクォータニオンを介して復元されること() {
            foreach (EulerAngles.Type order in System.Enum.GetValues(typeof(EulerAngles.Type))) {
                var input = new EulerAngles(order, new Vector3(25, 35, -40) * Mathf.Deg2Rad);
                var q = input.ToQuaternion();
                var output = EulerAngles.FromQuaternion(order, q);
                AssertRotationEqual(input.ToMatrix(), output.ToMatrix(), $"euler→quat→euler {order}");
            }
        }

        [Test]
        public void 対称オイラー角がクォータニオンを介して復元されること() {
            foreach (EulerAngles2.Type order in System.Enum.GetValues(typeof(EulerAngles2.Type))) {
                var input = new EulerAngles2(order, 30 * Mathf.Deg2Rad, 60 * Mathf.Deg2Rad, -50 * Mathf.Deg2Rad);
                var q = input.ToQuaternion();
                var output = EulerAngles2.FromQuaternion(order, q);
                AssertRotationEqual(input.ToMatrix(), output.ToMatrix(), $"euler2→quat→euler2 {order}");
            }
        }

        #endregion


        #region 軸‐角度 (AxisAngle)

        [Test]
        public void 軸角度が回転行列を介して復元されること() {
            var axes = new[] { Vector3.right, Vector3.up, Vector3.forward, new Vector3(1, 2, 3), new Vector3(-2, 1, 0.5f) };
            var angsDeg = new[] { 0f, 10f, 90f, 179f, -120f, 180f };
            foreach (var axis in axes) {
                foreach (var deg in angsDeg) {
                    var input = new AxisAngle(axis, deg * Mathf.Deg2Rad);
                    var restored = AxisAngle.FromMatrix(input.ToMatrix());
                    AssertRotationEqual(input.ToMatrix(), restored.ToMatrix(), $"axisangle {axis} {deg}");
                }
            }
        }

        [Test]
        public void 軸角度のToMatrixとToQuaternionが整合すること() {
            var input = new AxisAngle(new Vector3(1, -2, 0.5f), 75 * Mathf.Deg2Rad);
            var viaMatrix = input.ToMatrix();
            var viaQuat = QuaternionUtils.FromQuaternion(input.ToQuaternion());
            AssertRotationEqual(viaMatrix, viaQuat, "axisangle: ToMatrix vs ToQuaternion");
        }

        [Test]
        public void オイラー角と軸角度が相互変換できること() {
            foreach (EulerAngles.Type order in System.Enum.GetValues(typeof(EulerAngles.Type))) {
                var input = new EulerAngles(order, new Vector3(15, -55, 70) * Mathf.Deg2Rad);
                var aa = input.ToAxisAngle();
                var output = EulerAngles.FromAxisAngle(order, aa);
                AssertRotationEqual(input.ToMatrix(), output.ToMatrix(), $"euler→axisangle→euler {order}");
            }
        }

        #endregion
    }
}
