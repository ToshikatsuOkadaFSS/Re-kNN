# リリースノート

## 2026.5.11 (Version 1.1)

### 追加

- MNIST のサンプルアプリケーション（WinUI）を追加
- Semantic Context Clustering の結果を追加
- DB 全体を一括で Refine する API を追加
- Linux（Ubuntu）用ライブラリを追加

### 変更

- エラーメッセージの表示内容を改善
- 一部 API の戻り値を見直し
  - DB の `Load` / `Save` エラー時に `false` を返すように変更
- DLL および `.so` ファイルの保存位置を見直し
  - ライブラリ関連ファイルを library ディレクトリに集約
- これらの変更に合わせて各種ドキュメントを修正

## 2026.4.1 (Version 1.0)

- 初期リリース

