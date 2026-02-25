# Unity Spline Advance v1.2.2
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
### SplineGuide.cs
- Spline上の設置ガイドです。**インスペクター上に追加して使用します**。
- 指定したSplineの長さや位置毎の座標などを計測することができます。
### RouteManager.cs
- Splineの移動を拡張するコードです。**インスペクター上に追加して使用しますが、スクリプトからも扱います**。
- 分岐やワールドサイズ最適化などに応用できます。
### README.md
- READMEです。このファイルです。
### LICENSE
- MIT Licenseです。
### CHANGELOG
- パッケージのバージョン情報などの変更履歴(Change Log)です。

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
- `float distance`の値が`SplineContainer spline`で設定されたSplineの長さをはみ出すと、クランプされます。閉スプラインの挙動を作成したい場合、`distance`を0に起点に戻すなどの処理はユーザーがスクリプトにて設定する必要があります。

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
### `GetIncrineParmill(SplineContainer spline, float distance, out float parmill)`
- 指定されたSpline上の点の勾配をパーミル(‰)で返すAPIです。
- 高低差は、0.001unit(=1mUnit)の精度で計算します。
### 各値について
- `SplineContainer spline`  
  計算の元となるSplineを指定します。
- `float distance`  
  Spline上の位置(`Knot[0]`からの距離)をUnity Unitで指定します。
- `out float parmill`  
  勾配をパーミル(‰)で返します。

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
  メッシュ変形を行うかどうかを選択します。（実行中はEditor上で何も操作できなくなるため、高ポリのオブジェクトへの適用には注意してください。）
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

## SplineGuideの使用方法
コンポーネントを使用したオブジェクト自身を、Editモード上でもSpline上で動かすことができるスクリプトです。座標などのメモや後述のRouteManagerのテストにお使いください。
### 使用方法
1. Spline ContainerがアタッチされたGameObjectにアタッチします。
2. 各値（後述）を設定します。
### 各値について
- Spline  
  移動したいSplineを指定します。`IsFork`がFalseの時のみ有効です。
- Distance  
　Spline上の位置を指定します。
- SplineLength  
  Splineの長さを表示します。`IsFork`がFalseの時は、`Spline`にアタッチされているSplineコンテナにあるSplineの長さを、そうでない時はRouteManagerで設定したルートを設定します。
- IsFork  
  RouteManagerを使用するか指定します。
- RouteManager  
  RouteManagerを使用する場合は、適応したいRouteManagerをドラッグもしくはプルダウン選択してください。

## RouteManagerの使用方法
Splineの移動経路を設定し、始点のSplineからの距離の座標や回転を取得できます。取得方法は後述します。複数のSplineを次々と移動したり、分岐点を設定することができます。
### 各値について
- Recalculate Route  
  値の更新時に再計算を手動で行います。変更後は必ず押すようにしてください。前回のものが適応されたままの場合はここを押してください。
--JointInfo--
- Spline  
  経路として組み込みたいSplineを指定します。
- Start  
  `Spline`にて指定した経路の始点を入力します。
- End  
  `Spline`にて指定した経路の終点を入力します。
### 利用できる値について
値を利用するには、APIとは別の方法が必要になります。特定のRouteManagerの値を利用できる代わりに、ユーザーは個別で利用したいRouteManagerを指定する必要があります。例えば、以下のような記述をして、RouteManagerを指定してください。  
分岐点を作成したい場合、`SplineGuide`を使用して分岐を開始したい地点の座標と回転をコピーし、その点からSplineの編集画面にてその値を入力し、分岐をがたつきなく作成することができます。この方法は少し特殊ですが、分岐点だけでなくワールドスケールの最適化など、様々な使用方法と拡張性を期待できます。
```
public RouteManager routeManager;
```
なお、値の説明には、値にアクセスするために`routeManager.`を使用するものとします。
- `routeManager.distance = `  
  RouteManagerでは、複数のSplineを一本のように扱うイメージです。distanceは、`element[0]`に入力したSplineのStart値を起点として指定してください。
- `routeManager.calcPos`&`routeManager.calcRot`  
  distanceで指定したSpline上の位置や回転を取得できます。**この値は`Vector3`です。利用者側からは改変できません。**
- `routeManager.SplineLength`  
  Editor上で設定したSplineの合同全長を計測して利用できます。**この値は利用者側から改変できません。**


## 免責事項
1. このスクリプトは**AIを使って作成しました**。特に、MeshDeformについては私の能力不足でAIのみを使っていて、自力で修正をしていません。ただし、動作確認は十分に行いました。

2. 商用利用はMIT Licenseの通りですが、**商用利用する際や重要なプロジェクトに使用する場合は十分に検証をしてください**。

3. `SplineAdvanceSystem.GetOffsetOnSpline`について再度注意しますが、**求められる位置はあくまで近似点であり、実際の正確な値を保証するわけではありません。複雑な形のSplineでは、大きくずれることがあります**。

4. メッシュ変形機能(Mesh Deform)は実行すると、完了するまで何も操作できなくなるため、高ポリのオブジェクトなど負荷のかかるオブジェクトを複製する場合は十分注意してください。

## ご連絡
何かお問い合わせがある場合は、作者(Cotoráin)のXまたはGitHub, 本システム公開元のIssueからお願いいたします。それ以外からは回答いたしかねます。