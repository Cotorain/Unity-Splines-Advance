# Changelog

## - 2026-03-01
### Changed
- `SplineAdvanceSystem`の名称を`SplinesSystem`に変更
- `SplinesSystem.CalcSpline`の計算方法を変更。微量ながら軽量化を施した。

## - 2026-02-25
### Changed
- `SplineAdvanceSystem.CalcSpline`での値のクランプするようにした
- RouteManagerにて`Update()`処理で複数の呼び出しでも戻り値の更新ができるようにした

## - 2026-02-24
### Added
- SplineGuideを追加しました。
- RouteManagerを追加しました。分岐の制御などが可能になります。
### Changed
- `SplineAdvanceSystem.CalcSpline`で、ログが大量発生するバグの修正
- `SplineAdvanceSystem.GetOffsetOnSpline`のバグ修正
- `SplineAdvanceInstantiate`を外部参照不可化

## - 2026-01-24
### Added
- 指定された点から勾配を取得するAPIを追加しました。

## - 2025-12-21
### Added
- 初版リリース