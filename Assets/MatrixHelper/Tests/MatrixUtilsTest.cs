using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// [REF]
//  _: オイラー角計算ツール (Euler Angle Calculator) https://www.pystyle.info/apps/euler-angles-calculator/

namespace nitou.Tests {

    public class MatrixUtilsTest {
        private const float Threshold = 1e-3f;


        #region 順変換 - X軸回転行列

        [Test]
        public void X軸回転行列が正しく生成されること() {
            // Arrange
            float theta = 90 * Mathf.Deg2Rad;

            // Act
            Matrix4x4 result = MatrixUtils.Rx(theta);
            Debug.Log(result);

            // Assert
            Assert.That(result.m00, Is.EqualTo(1).Within(Threshold));
            Assert.That(result.m10, Is.EqualTo(0).Within(Threshold));
            Assert.That(result.m20, Is.EqualTo(0).Within(Threshold));

            Assert.That(result.m01, Is.EqualTo(0).Within(Threshold));
            Assert.That(result.m11, Is.EqualTo(0).Within(Threshold));
            Assert.That(result.m21, Is.EqualTo(1).Within(Threshold));
            
            Assert.That(result.m02, Is.EqualTo(0).Within(Threshold));
            Assert.That(result.m12, Is.EqualTo(-1).Within(Threshold));
            Assert.That(result.m22, Is.EqualTo(0).Within(Threshold));
        }

        #endregion

        #region 順変換 - Y軸回転行列

        [TestCase(0, 0, 0)]
        [TestCase(30, 45, 80)]
        [TestCase(90, 0, 0)]
        [TestCase(0, 90, 0)]
        [TestCase(180, 180, 180)]
        [TestCase(-30, -45, -80)]
        [TestCase(360, 360, 360)]
        public void XYZオイラー角から回転行列が正しく生成されること(float x, float y, float z) { 
            // Arrange
            var angles = new Vector3(x, y, z) * Mathf.Deg2Rad;
            var inputEuler = new EulerAngles(EulerAngles.Type.XYZ, angles);

            // Act
            var mat = inputEuler.ToMatrix();
            Debug.Log(mat);
            var outputEuler = MatrixUtils.GetEulerAnglesXYZ(mat);

            // Assert（±180°等は角度一意でないため、回転行列の往復一致で検証する）
            Debug.Log(inputEuler.ToStringDeg());
            Debug.Log(outputEuler.ToStringDeg());
            AssertRotationEqual(mat, outputEuler.ToMatrix(), inputEuler.ToStringDeg());
        }

        [TestCase(0, 0, 0)]
        [TestCase(30, 45, 80)]
        [TestCase(90, 0, 0)]
        [TestCase(0, 90, 0)]
        [TestCase(180, 180, 180)]
        [TestCase(-30, -45, -80)]
        [TestCase(360, 360, 360)]
        public void ZYZオイラー角から回転行列が正しく生成されること(float x, float y, float z) {
            // Arrange
            var angles = new Vector3(x, y, z) * Mathf.Deg2Rad;
            var inputEuler = new EulerAngles2(EulerAngles2.Type.ZYZ, angles.x, angles.y, angles.z);

            // Act
            var mat = inputEuler.ToMatrix();
            Debug.Log(mat);
            var outputEuler = MatrixUtils.FromRotationMatrixZYZ(mat);

            // Assert（±180°や負の中間角は角度一意でないため、回転行列の往復一致で検証する）
            Debug.Log(inputEuler.ToStringDeg());
            Debug.Log(outputEuler.ToStringDeg());
            AssertRotationEqual(mat, outputEuler.ToMatrix(), inputEuler.ToStringDeg());
        }

        // [NOTE] 対称オイラー角(i-j-i)では中間角 p が Acos により [0,180] に制限されるため、
        //  往復が一意に決まる範囲（p∈[0,180]、特異点 p∈{0,180} では s2=0）でケースを選定している．

        [TestCase(0, 0, 0)]     // 特異点 (p=0)
        [TestCase(90, 0, 0)]    // 特異点 (p=0)
        [TestCase(50, 180, 0)]  // 特異点 (p=180)
        [TestCase(30, 45, 80)]
        [TestCase(0, 90, 0)]
        [TestCase(-30, 60, 80)]
        [TestCase(45, 135, -60)]
        public void XZXオイラー角が回転行列を介して正しく復元されること(float s1, float p, float s2) {
            // Arrange
            var angles = new Vector3(s1, p, s2) * Mathf.Deg2Rad;
            var inputEuler = new EulerAngles2(EulerAngles2.Type.XZX, angles.x, angles.y, angles.z);

            // Act
            var mat = inputEuler.ToMatrix();
            var outputEuler = MatrixUtils.GetEulerAnglesXZX(mat);

            // Assert
            Debug.Log(inputEuler.ToStringDeg());
            Debug.Log(outputEuler.ToStringDeg());
            Assert.That(outputEuler, Is.EqualTo(inputEuler));
        }

        [TestCase(0, 0, 0)]     // 特異点 (p=0)
        [TestCase(90, 0, 0)]    // 特異点 (p=0)
        [TestCase(50, 180, 0)]  // 特異点 (p=180)
        [TestCase(30, 45, 80)]
        [TestCase(0, 90, 0)]
        [TestCase(-30, 60, 80)]
        [TestCase(45, 135, -60)]
        public void ZXZオイラー角が回転行列を介して正しく復元されること(float s1, float p, float s2) {
            // Arrange
            var angles = new Vector3(s1, p, s2) * Mathf.Deg2Rad;
            var inputEuler = new EulerAngles2(EulerAngles2.Type.ZXZ, angles.x, angles.y, angles.z);

            // Act
            var mat = inputEuler.ToMatrix();
            var outputEuler = MatrixUtils.GetEulerAnglesZXZ(mat);

            // Assert
            Debug.Log(inputEuler.ToStringDeg());
            Debug.Log(outputEuler.ToStringDeg());
            Assert.That(outputEuler, Is.EqualTo(inputEuler));
        }




        #endregion


        #region 逆変換 - 全順序の網羅（行列往復）

        // 回転行列の回転成分(3x3)が一致することを検証する
        private static void AssertRotationEqual(Matrix4x4 a, Matrix4x4 b, string label) {
            for (int r = 0; r < 3; r++) {
                for (int c = 0; c < 3; c++) {
                    Assert.That(b[r, c], Is.EqualTo(a[r, c]).Within(Threshold),
                        $"{label}: m{r}{c} 不一致");
                }
            }
        }

        // 特異点（中間角 0/±90/180）を含むサンプル
        private static readonly Vector3[] _samples = {
            new Vector3(0, 0, 0),     new Vector3(30, 40, 50),   new Vector3(-20, 80, 140),
            new Vector3(90, 0, 0),    new Vector3(0, 90, 0),     new Vector3(0, 0, 90),
            new Vector3(10, 90, -30), new Vector3(10, -90, 30),  new Vector3(170, 10, -160),
            new Vector3(45, -45, 45), new Vector3(0, 180, 0),    new Vector3(60, 180, -60),
        };

        [Test]
        public void 全順序_非対称オイラー角が回転行列を介して復元されること() {
            foreach (EulerAngles.Type order in System.Enum.GetValues(typeof(EulerAngles.Type))) {
                foreach (var s in _samples) {
                    var input = new EulerAngles(order, s * Mathf.Deg2Rad);
                    var mat = input.ToMatrix();
                    var output = MatrixUtils.ToEulerAngles(mat, order);
                    AssertRotationEqual(mat, output.ToMatrix(), $"{order} {s}");
                }
            }
        }

        [Test]
        public void 全順序_対称オイラー角が回転行列を介して復元されること() {
            foreach (EulerAngles2.Type order in System.Enum.GetValues(typeof(EulerAngles2.Type))) {
                foreach (var s in _samples) {
                    var input = new EulerAngles2(order, s.x * Mathf.Deg2Rad, s.y * Mathf.Deg2Rad, s.z * Mathf.Deg2Rad);
                    var mat = input.ToMatrix();
                    var output = MatrixUtils.ToEulerAngles(mat, order);
                    AssertRotationEqual(mat, output.ToMatrix(), $"{order} {s}");
                }
            }
        }

        #endregion


        //[Test]
        //public void 回転行列からZYXオイラー角が正しく取得されること() {
        //    // Arrange
        //    var angles = new Vector3(30, 0,0)
        //    var inputEuler = new EulerAngles(EulerAngles.Type.ZYX, )
        //    var mat = MatrixUtils.Rx(Mathf.PI / 2) * MatrixUtils.Ry(Mathf.PI / 2) * MatrixUtils.Rz(Mathf.PI / 2);

        //    // Act
        //    EulerAngles result = MatrixUtils.GetEulerAnglesZYX(mat);

        //    // Assert
        //    Assert.That(result.Order, Is.EqualTo(EulerAngles.Type.ZYX));
        //    Assert.That(result.X, Is.EqualTo(Mathf.PI / 2).Within(1e-3f));
        //    Assert.That(result.Y, Is.EqualTo(Mathf.PI / 2).Within(1e-3f));
        //    Assert.That(result.Z, Is.EqualTo(Mathf.PI / 2).Within(1e-3f));
        //}

    }
}
