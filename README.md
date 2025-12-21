# Unity Spline Advance v1.0
## 初めに - Unity Spline Advanceについて
Unity Spline Advanceは、Unity Splinesを拡張する目的で作成された、API及び拡張機能が同梱されたシステムです。Unity C#コードにおいて、ユーザーは、API（void）をスクリプトから使用することができます。

## 動作環境（確認済み）
- Unity 6.3LTS(6000.3.1f1) 以降
- Unity Splines 2.8.2 以降

## 添付ファイル（概要）
### SplineAdvanceSystem.cs
- APIが含まれた静的コードです。**インスペクター上に追加する必要はありません**。
- メソッドは`public`で宣言されていますので、ユーザーはAPIをどこからでも使えます。
### SplineAdvanceInstantiate.cs
- オブジェクトの配置を拡張した機能です。**インスペクター上に追加して使用します**。
- メッシュ変形などの機能が含まれています。
### README.md
- READMEです。このファイルです。
### LICENSE
- MIT Licenseです。

## インストール方法
このファイルをGitHubの`Code`→`Download ZIP`よりインストールし、解凍して`Assets`フォルダーもしくは`Packages`フォルダーに貼り付けてください。
また、GitHubのURLから直接パッケージとしてインストールする場合は、Unityの`Package Manager`→`+▼`→`Add package from Git URL...`で出てきたテキスト入力欄に`https://github.com/Cotorain/Unity-Splines-Advance.git`と入力してください。

## SplineAdvanceSystemのAPIについて
### `SplineAdvanceSystem.CalcSpline(SplineContainer spline, float distance, out Vector3 calcPos, out Vector3 calcRot)`
- Splineの距離を指定すると、その地点の座標を返すAPIです。
- `out`指定がある変数はこのメソッド以降の何処かの処理で使う必要があります。(Unity C#の仕様の為)
### 各値について
- `SplineContainer spline`  
  計算の元となるSplineを指定します。
- `float distance`  
  Spline上の位置(`Knot[0]`からの距離)をUnity Unitで指定します。
- `out Vector3 calcPos`  
  `distance`で指定したSpline上の位置の座標を返します。
- `out Vector3 calcRot`  
  `distance`で指定したSpline上の位置の回転をEuler角で返します。

----

### `SplineAdvanceSystem.SetObj(SplineContainer spline, GameObject targetObj, float distance)`
- Splineの距離を指定すると、Spline上に指定されたオブジェクトを配置するAPIです。
### 各値について
- `SplineContainer spline`  
  計算の元となるSplineを指定します。
- `GameObject targetObj`  
  配置を適用するオブジェクトを指定します。
- `float distance`  
  Spline上の位置(`Knot[0]`からの距離)をUnity Unitで指定します。  
### 注意点
- `float distance`の値が`SplineContainer spline`で設定されたSplineの長さよりも大きくなると、0にリセットされます。また、この値が0より小さくなると1に移動します。  
これは、Splineの開閉問わずオブジェクトが起点や終点にとどまることを防ぐためです。

----
### `SplineAdvanceSystem.GetOffsetOnSpline(SplineContainer spline, float distance, float offset, out Vector3 calcPos, out Vector3 calcRot)`
- Spline上の点から一定の距離、直線状に離れたspline上の点の近似点を返すAPIです。
- 列車の台車の挙動などに利用できます。
### 各値について
- `SplineContainer spline`  
  計算の元となるSplineを指定します。
- `float distance`  
  Spline上の位置(`Knot[0]`からの距離)をUnity Unitで指定します。
- `float offset`  
  `distance`からのspline上の距離を指定します。負の値なら`distance`より後方に、正の値なら`distance`より前方になります。
- `out Vector3 calcPos`  
  `distance`で指定したSpline上の位置の座標を返します。
- `out Vector3 calcRot`  
  `distance`で指定したSpline上の位置の回転をEuler角で返します。
### 注意点
- 求められる位置は**あくまで近似点であり、実際の正確な値を保証するわけではありません**。
- 複雑な形のSplineでは、**大きくずれることがあります**。
- `Calcspline()`の仕様により、二点のうちどちらかがSplineの範囲外になると、開Splineの場合は挙動に不具合が生じます。

----

## SplineAdvanceInstantiateの使用方法
オブジェクトをSplineに沿って配置するスクリプトです。変形(使用可能ただしβ版である)機能も使えます。
### 使用方法
1. Spline ContainerがアタッチされたGameObjectにアタッチします。
2. 各値（後述）を設定します。
3. Instance Objectsボタンをクリックします。
### 各値について
- Spline Length  
  同GameObjectにアタッチされているSpline Containerの長さをUnity Unitで表示します。この値は常時更新されます。
- Instance Objects（ボタン）  
  GameObjectのインスタンスを生成します。配置先はInstantiate Parent Objectで指定した親オブジェクトの子オブジェクトとなります。
- Clear Instance Objects（ボタン）  
  Instanceされたオブジェクト、またはInstantiate Parent Objectの直下にある` (Clone)`表記があるオブジェクトを削除します。

--Instance Settings--
- Prefab  
  Instantiateを適応するオブジェクトのPrefabです。
- Mesh Deform  
  メッシュ変形を行うかどうかを選択します。（現在はβ版です。）
- Installation Interval  
  オブジェクトを設置する間隔です。**Unity Unitで設定します**。
- Transform Correction  
  設置後にTransformの補正を行います。補正はGlobal座標系で行います。
- Rotation Correction  
  設置後にRotationの補正を行います。補正は**Local座標系**で行います。
- Installation Starting Point  
  設置開始地点です。Unity Unitで指定します。
- Installation Ending Point  
  設置終了地点です。Unity Unitで指定します。
- Instantiate Parent Object  
  Instantiateしたオブジェクトを保存する親オブジェクトを指定します。
- Forward Axis  
  オブジェクトの前方系です。Object Y+はサポートしていません。

## 免責事項
1. このスクリプトは**AIを使って作成しました**。特に、MeshDeformについては私の能力不足でAIのみを使っていて、自力で修正をしていません。ただし、動作確認は十分に行いました。

2. 商用利用はMIT Licenseの通りですが、**商用利用する際や重要なプロジェクトに使用する場合は十分に検証をしてください**。

3. `SplineAdvanceSystem.GetOffsetOnSpline`について再度注意しますが、**求められる位置はあくまで近似点であり、実際の正確な値を保証するわけではありません。
複雑な形のSplineでは、大きくずれることがあります**。

4. メッシュ変形機能(Mesh Deform)は現時点ではβ版であり、複雑なオブジェクトなどでは不具合が生じる可能性があります。

## ご連絡
何かお問い合わせがある場合は、作者(Cotoráin)のXまたはGitHub, 本システム公開元のIssueからお願いいたします。それ以外からは回答いたしかねます。