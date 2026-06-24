# MatrixHelper サンプル

`MatrixHelper` の各回転表現の相互変換を体験するためのサンプルです。

## 使い方

1. シーン内の空の GameObject に `RotationConversionSample` をアタッチします。
2. Inspector で **オイラー角の順序（`Order`）** と **角度（`Euler Degrees`, 度数法）** を設定します。
3. Scene ビューに、回転後の座標軸が Gizmos で描画されます。
   - 🔴 赤 … X 軸
   - 🟢 緑 … Y 軸
   - 🔵 青 … Z 軸
   - 🟡 黄 … 軸‐角度（オイラーの回転定理）の回転軸
4. コンポーネントの右クリックメニュー（ContextMenu）から、以下を実行できます。
   - **Log All Representations** … オイラー角／回転行列／クォータニオン／軸‐角度を一覧表示
   - **Verify Round Trip** … 各表現を経由して元の回転へ戻ることを確認（回転行列で誤差比較）
   - **Log All Orders (same rotation)** … 同一の回転を 6 順序すべてで逆変換した結果を表示

## 扱う API

| 変換 | API |
| --- | --- |
| オイラー角 → 回転行列 | `EulerAngles.ToMatrix()` |
| 回転行列 → オイラー角（順序指定） | `MatrixUtils.ToEulerAngles(mat, order)` |
| オイラー角 → クォータニオン | `EulerAngles.ToQuaternion()` |
| クォータニオン → オイラー角 | `EulerAngles.FromQuaternion(order, q)` |
| オイラー角 → 軸‐角度 | `EulerAngles.ToAxisAngle()` |
| 軸‐角度 → オイラー角 | `EulerAngles.FromAxisAngle(order, aa)` |

## 注意（座標系）

本ライブラリの回転は **右手系**（`MatrixUtils.Rx/Ry/Rz` と整合）で計算します。
Unity の `Transform` / `Quaternion` は **左手系** のため規約が異なります。
本サンプルの可視化は「回転行列が基底ベクトルをどう写すか」を右手系のまま描画しており、
`transform.rotation` へ直接代入する用途には対応していません。
