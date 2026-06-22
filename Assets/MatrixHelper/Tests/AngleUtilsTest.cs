using NUnit.Framework;
using UnityEngine;
using nitou;

namespace Tests {


    [TestFixture]
    public class AngleUtilsTests {

        // テストデータを配列で準備
        private static readonly float[] testAngles = new float[] { -360f, -180f, 0f, 180f, 360f, 540f, 720f };

        private static readonly float[] expectedNormalized360 = new float[] { 0f, 180f, 0f, 180f, 0f, 180f, 0f };
        private static readonly float[] expectedNormalized180 = new float[] { 0f, -180f, 0f, 180f, 0f, 180f, -180f };

        private static readonly float[] testRadians = new float[] { -Mathf.PI, 0f, Mathf.PI, Mathf.PI * 2f, Mathf.PI * 3f };

        private static readonly float[] expectedNormalized2Pi = new float[] { Mathf.PI, 0f, Mathf.PI, 0f, Mathf.PI };
        private static readonly float[] expectedNormalizedPi = new float[] { -Mathf.PI, 0f, Mathf.PI, 0f, -Mathf.PI };


        [Test]
        public void TestNormalize360() {
            float[] testAngles = new float[] { -360f, -180f, 0f, 180f, 360f, 540f, 720f };
            float[] expectedAngles = new float[] { 0f, 180f, 0f, 180f, 0f, 180f, 0f };

            // Act & Assert
            for (int i = 0; i < testAngles.Length; i++) {
                var normalized = AngleUtils.Normalize360(testAngles[i]);
                Assert.That(
                    normalized, 
                    Is.EqualTo(expectedAngles[i]).Within(EulerAngles.Tolerance), 
                    $"Normalize360 failed for {testAngles[i]}");
            }
        }

        [Test]
        public void TestNormalize180() {

            float[] testAngles = new float[] { -360f, -180f, 0f, 180f, 360f, 540f, 720f };
            float[] expectedAngles = new float[] { 0f, -180f, 0f, 180f, 0f, 180f, -180f };

            // Act & Assert
            for (int i = 0; i < testAngles.Length; i++) {
                var normalized = AngleUtils.Normalize180(testAngles[i]);
                Assert.That(
                    normalized, 
                    Is.EqualTo(expectedAngles[i]).Within(EulerAngles.Tolerance), 
                    $"Normalize180 failed for {testAngles[i]}");
            }
        }
    }
}