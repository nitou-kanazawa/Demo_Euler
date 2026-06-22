using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// [REF]
//  _: オイラー角計算ツール (Euler Angle Calculator) https://www.pystyle.info/apps/euler-angles-calculator/

namespace nitou.Tests {


    public class EulerAnglesTest {

        [Test]
        public void 同じ値として判定されること() {
            var angles1 = new EulerAngles(EulerAngles.Type.XYZ, 0.5235988f, 0.7853982f, 1.396263f);
            var angles2 = new EulerAngles(EulerAngles.Type.XYZ, 0.5235988f, 0.7853981f, 1.396263f);

            Assert.IsTrue(angles1.Equals(angles2)); // 許容誤差内で同値
            Assert.IsFalse(angles1.IsZero()); // ゼロではない
        }
    }

}