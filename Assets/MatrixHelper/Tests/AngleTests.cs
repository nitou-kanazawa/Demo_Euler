using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using nitou;
using System;

namespace Tests {

    public class AngleTests {

        // 定数
        private const double Tolerance = 0.0001; // 許容誤差

        private static class Circle {
            public const float Full = 360.0f;
            public const float Half = 180.0f;
            public const float Quart = 90.0f;
        }


        /// ----------------------------------------------------------------------------
        #region 生成・変換

        [TestCase(180, Mathf.PI)]
        [TestCase(-720, -4 * Mathf.PI)]
        [TestCase(-360 + -180, -1, -180)]
        [TestCase(-3600 + 180, -10, Mathf.PI)]
        [TestCase(0, 0)]
        public void インスタンスを作成する_正常値１(float expectedDegree, int value, float? additionalValue = null) {
            // Arrange
            Angle expected = Angle.FromDegree(expectedDegree);
            Angle result;

            // Act
            if (additionalValue.HasValue)
                result = Angle.FromDegree(value, additionalValue.Value);
            else
                result = value == 0 ? Angle.Zero : Angle.FromRadian(value);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(float.NaN)]
        [TestCase(float.NegativeInfinity)]
        [TestCase(float.PositiveInfinity)]
        public void インスタンスを作成する_異常値(float value) {
            // Assert
            Assert.Throws<ArithmeticException>(() => Angle.FromDegree(value));
        }
        #endregion


        [Test]
        public void 反転する() {
            Assert.That(Angle.Zero.Reverse(), Is.EqualTo(Angle.Zero));
            Assert.That(Angle.FromDegree(45).Reverse(), Is.EqualTo(Angle.FromDegree(-315)));
            Assert.That(Angle.FromDegree(-90).Reverse(), Is.EqualTo(Angle.FromDegree(270)));
            Assert.That(Angle.FromDegree(180).Reverse(), Is.EqualTo(Angle.FromDegree(-180)));
            Assert.That(Angle.FromDegree(360).Reverse(), Is.EqualTo(Angle.FromDegree(-360)));
            Assert.That(Angle.FromDegree(359).Reverse(), Is.EqualTo(Angle.FromDegree(-1)));
            Assert.That(Angle.FromDegree(361).Reverse(), Is.EqualTo(Angle.FromDegree(-1, -359)));
            Assert.That(Angle.FromDegree(-450).Reverse(), Is.EqualTo(Angle.FromDegree(360 + 270)));
            Assert.That(Angle.FromDegree(2, 90).Reverse(), Is.EqualTo(Angle.FromDegree(-2, -270)));
        }



        /// ----------------------------------------------------------------------------
        #region 正規化

        [TestCase(370, 10)]
        [TestCase(-450, -90)]
        [TestCase(-1080, 0)]
        public void 正規化された角度が期待通りである(float input, float expectedNormalized) {
            // Arrange
            var angle = Angle.FromDegree(input);

            // Act
            var normalizedDegree = angle.NormalizedDegree;

            // Assert
            Assert.That(normalizedDegree, Is.EqualTo(expectedNormalized).Within(Tolerance));
        }

        //[TestCase(0, 0)]
        //[TestCase(45, -315)]
        //[TestCase(-90, 270)]
        //[TestCase(180, -180)]
        //[TestCase(360, -360)]
        //[TestCase(359, -1)]
        //[TestCase(0, 0)]
        //[TestCase(0, 0)]
        //[TestCase(0, 0)]
        //[TestCase(0, 0)]
        //[TestCase(0, 0)]
        //[TestCase(-90, 270)] 
        //public void 正の正規化された角度を取得できる(float inputDegree1, float expectedNormalizedDegree) {
        //    // Arrange
        //    var angle = Angle.FromDegree(inputDegree);

        //    // Act
        //    var normalized = angle.PositiveNormalize();
        //    var target

        //    // Assert
        //    Assert.That(normalizedDegree, Is.EqualTo(expectedNormalizedDegree).Within(Tolerance));
        //}
        #endregion


        /// ----------------------------------------------------------------------------
        #region 判定

        [TestCase(720, true)]
        [TestCase(450, false)]
        public void 角度が360の倍数か判定できる(float inputDegree, bool expectedIsTrueCircle) {
            // Arrange
            var angle = Angle.FromDegree(inputDegree);

            // Act
            var isTrueCircle = angle.IsTrueCircle;

            // Assert
            Assert.That(isTrueCircle, Is.EqualTo(expectedIsTrueCircle));
        }
        #endregion


        /// ----------------------------------------------------------------------------
        #region 変換

        [TestCase(Mathf.PI / 2, 90)]
        [TestCase(-Mathf.PI, -180)]
        public void 弧度法からの変換が正しく行われる(float inputRadian, float expectedDegree) {
            // Arrange
            var angle = Angle.FromRadian(inputRadian);

            // Act
            var degree = angle.TotalDegree;

            // Assert
            Assert.That(degree, Is.EqualTo(expectedDegree).Within(Tolerance));
        }

        [TestCase(90, -90)]
        [TestCase(-180, 180)]
        public void 符号を反転させた角度を取得できる(float inputDegree, float expectedReversedDegree) {
            // Arrange
            var angle = Angle.FromDegree(inputDegree);

            // Act
            var reversed = angle.SignReverse();

            // Assert
            Assert.That(reversed.TotalDegree, Is.EqualTo(expectedReversedDegree).Within(Tolerance));
        }

        [TestCase(-90, 90)]
        [TestCase(180, 180)]
        public void 角度の絶対値を取得できる(float inputDegree, float expectedAbsoluteDegree) {
            // Arrange
            var angle = Angle.FromDegree(inputDegree);

            // Act
            var absolute = angle.Absolute();

            // Assert
            Assert.That(absolute.TotalDegree, Is.EqualTo(expectedAbsoluteDegree).Within(Tolerance));
        }
        #endregion


        /// ----------------------------------------------------------------------------
        #region 演算子

        [Test]
        public void 演算子の加算が正しく動作する() {
            // Arrange
            var angle1 = Angle.FromDegree(30);
            var angle2 = Angle.FromDegree(45);

            // Act
            var result = angle1 + angle2;

            // Assert
            Assert.That(result.TotalDegree, Is.EqualTo(75).Within(0.0001f));
        }

        [Test]
        public void 演算子の減算が正しく動作する() {
            // Arrange
            var angle1 = Angle.FromDegree(100);
            var angle2 = Angle.FromDegree(40);

            // Act
            var result = angle1 - angle2;

            // Assert
            Assert.That(result.TotalDegree, Is.EqualTo(60).Within(0.0001f));
        }

        [Test]
        public void 演算子の乗算が正しく動作する() {
            // Arrange
            var angle = Angle.FromDegree(90);

            // Act
            var result = angle * 2;

            // Assert
            Assert.That(result.TotalDegree, Is.EqualTo(180).Within(0.0001f));
        }

        [Test]
        public void 演算子の除算が正しく動作する() {
            // Arrange
            var angle = Angle.FromDegree(180);

            // Act
            var result = angle / 2;

            // Assert
            Assert.That(result.TotalDegree, Is.EqualTo(90).Within(0.0001f));
        }
        #endregion
    }
}