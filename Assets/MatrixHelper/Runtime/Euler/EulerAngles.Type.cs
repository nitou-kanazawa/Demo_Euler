using System;

// [REF]
//  _: オイラー角と固定角は逆順の関係にあるらしい https://space-denpa.jp/2021/05/04/relationship-euler-fix/


namespace nitou {

    // [NOTE]
    //  オイラー角は元の座標軸周りの回転を表す．

    // [NOTE] 
    //  オイラー角 X-Y-Z系 の場合
    //      1. 初期の座標系を x-y-z とする
    //      2. x 軸周りで α 回転する
    //      3. 回転後のy' 軸周りで β 回転する
    //      4. 回転後のz'' 軸周りで γ 回転する

    public partial struct EulerAngles {

        /// <summary>
        /// (i-j-k) オイラー角の分類．x軸、y軸、z軸 の全てを使用するタイプ．
        /// </summary>
        public enum Type {
            XYZ,
            XZY,
            YXZ,
            YZX,
            ZXY,
            ZYX,
        }
    }

    public partial struct EulerAngles2 {

        /// <summary>
        /// (i-j-i) オイラー角の分類．最初と最後に同じ軸を2度使用するタイプ．
        /// </summary>
        public enum Type {
            XZX,
            XYX,
            YZY,
            YXY,
            ZXZ,
            ZYZ
        }
    }

    // [NOTE]
    //  固定角は元の座標軸周りの回転を表す．

    // [NOTE] 
    //  固定角 X-Y-Z系 の場合
    //      1. 初期の座標系を x-y-z とする
    //      2. x 軸周りで α 回転する
    //      3. y 軸周りで β 回転する
    //      4. z 軸周りで γ 回転する

    public partial struct FixedAngles {

        /// <summary>
        /// (i-j-k) 固定角の分類．x軸、y軸、z軸 の全てを使用するタイプ．
        /// </summary>
        public enum FixedAngleType {
            XYZ,
            XZY,
            YXZ,
            YZX,
            ZXY,
            ZYX,
        }
    }

    /// <summary>
    /// (i-j-i) 固定角の分類．最初と最後に同じ軸を2度使用するタイプ．
    /// </summary>
    public enum FixedAngle2Type {
        XZX,
        XYX,
        YZY,
        YXY,
        ZXZ,
        ZYZ
    }

}