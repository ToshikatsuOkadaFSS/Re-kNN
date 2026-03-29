# ONNX モデル準備ガイド

本書では、Re-kNN の評価ツールで使用する ONNX モデルおよびベクトルデータの準備方法について説明します。

英語版: [onnx-preparation.md](./onnx-preparation.md)  
日本語版 README: [README.ja.md](../README.ja.md)

---

## 1. 本書の目的

本書は、Re-kNN の評価ツールで使用する ONNX モデルおよび入力データの準備方法を説明するものです。

主な対象読者は次のとおりです。

- 自身のデータセットからベクトルデータを生成したい利用者
- 自身の環境で Re-kNN を評価したい利用者
- Text Domain の評価フローを再現したい利用者
- 登録用に token-level または sentence-level のベクトルを準備したい利用者

本書は、主に ONNX ベースの埋め込み生成が関係する **Text Domain** のワークフローに焦点を当てています。

---

## 2. 対象範囲

Re-kNN は現在、以下の標準評価対象を定義しています。

- **Text Domain**: 768 次元  
  対象: `bert-base-multilingual-cased` により生成されたテキスト埋め込み
- **Pattern Domain**: 784 次元  
  対象: MNIST の生ピクセルベクトル
- **Vision Domain**: 3072 次元  
  対象: CIFAR-10 の生ピクセルベクトル

このうち、ONNX モデルの準備が主に必要となるのは **Text Domain** です。

MNIST および CIFAR-10 の評価では、Re-kNN は生ピクセルベクトルをそのまま使用するため、ONNX モデルの準備は不要です。

---

## 3. 動作確認環境

以下は、ONNX 関連評価における現在の参照環境です。

- OS: Windows 11 Professional
- ランタイム: .NET 8
- 動作確認済みテキストモデル対象: `bert-base-multilingual-cased`
- 想定出力次元: 768

なお、性能および互換性は次の要因により変化する可能性があります。

- ONNX Runtime のバージョン
- tokenizer の実装
- モデルの export オプション
- pooling 方法
- ハードウェアおよびメモリ条件

---

## 4. 想定ワークフロー

Text Domain における標準的なワークフローは次のとおりです。

1. 学習済み言語モデルを用意する
2. モデルを ONNX 形式に export する
3. tokenizer の設定を用意する
4. 元テキストをモデル入力へ変換する
5. ONNX 推論を実行する
6. モデル出力からベクトルを抽出する
7. Re-kNN が利用できる形式でベクトルを保存する
8. ベクトルを Re-kNN に登録し、検索および推論評価を行う

---

## 5. モデル準備方針

### 5.1 推奨モデル種別

現在の Text Domain 評価では、想定されるベクトル次元は **768** です。

そのため、適切なモデルは少なくとも以下を満たす必要があります。

- 出力ベクトル次元が固定で 768 であること
- ONNX Runtime 上で安定して推論できること
- 前処理が再現可能であること
- tokenizer 設定が利用可能であること

現在の参照対象は次のとおりです。

- `bert-base-multilingual-cased`

### 5.2 Sentence-Level ベクトルと Token-Level ベクトル

Transformer モデルからベクトルを抽出する方法は複数あります。

代表的な選択肢は次のとおりです。

- **sentence-level vector**
  - 例: pooled output
  - 例: token embedding に対する mean pooling
- **token-level vector**
  - 1 トークンごとに 1 ベクトル
  - 文書内の細粒度な登録が必要な場合に有効

Re-kNN は検索および推論の両方のワークフローをサポートしますが、どの方式が適切かは評価目的に依存します。

例えば次のように使い分けます。

- **文書 / 文単位の検索**
  - sentence-level または segment-level ベクトルで十分な場合がある
- **文書内の細粒度な検索**
  - token-level 登録の方が適切な場合がある

### 5.3 Wikipedia JP 評価における Token-Level 登録

Wikipedia JP の評価では、Re-kNN は対象テキストに含まれる **すべてのトークンベクトル** を登録します。

これは次を意味します。

- ベクトル数は **1 記事 1 ベクトル** を表しているわけではない
- 検索対象の各エントリは token-level ベクトルに対応する
- 最終的なベクトル数は token-level 登録を反映する

このため、ベクトル数は元の文書数より大きくなります。

---

## 6. ONNX モデルの準備

### 6.1 一般的な注意事項

Re-kNN は、生成された ONNX モデルが以下を満たす限り、特定の export ツールを必須とはしません。

- 対象環境で正しく実行できること
- 再現可能なベクトル出力を生成できること
- 想定した次元数と一致すること
- 互換性のある tokenizer パイプラインと組み合わせて使えること

本書では、`optimum` を用いた ONNX export 手順を説明します。

### 6.2 BERT モデルを ONNX に export する

必要な Python モジュールを以下のように導入します。  
バージョンは実行環境に応じて適切なものを選択してください。

```bash
pip install transformers==4.55.4
pip install fugashi==1.5.1
pip install ipadic==1.0.0

pip install onnx
pip install onnxruntime
pip install transformers[onnx]
pip install optimum[exporters]
````

コマンドラインから ONNX モデルを生成します。
以下の例では、`onnx-bert-base-multilingual-cased/` 配下に ONNX ファイル群を生成します。

```bash
optimum-cli export onnx --model bert-base-multilingual-cased --task feature-extraction onnx-bert-base-multilingual-cased/
```

---

## 7. 前処理要件

### 7.1 Tokenization

本書では tokenization の詳細は扱いません。
使用する BERT モデルに応じて、適切な tokenization を行ってください。

サンプルプログラムでは、`bert-base-multilingual-cased` 向けの tokenization を実装しています。

サンプルプログラムで使用する最大シーケンス長は 512 です。

具体的な実装例については、本リポジトリに含まれるサンプルプログラムを参照してください。

### 7.2 テキストエンコーディング

一貫したテキストエンコーディング形式を使用してください。

推奨:

* UTF-8

注意すべき点:

* 改行正規化
* 制御文字
* Unicode 正規化
* 不整合な空白処理

---

## 8. 出力ベクトルの抽出

### 8.1 一般的な抽出方法

用途に応じて、最終的なベクトルは次のいずれかから導出されます。

* CLS token embedding
* token embedding に対する mean pooling
* token ごとの hidden states
* 専用の pooled output head

### 8.2 推奨される記録事項

評価結果を公開または共有する場合は、少なくとも以下を明示してください。

* 使用した ONNX モデル
* tokenizer のバージョンまたは出典
* 最大シーケンス長
* pooling 戦略
* ベクトルが sentence-level か token-level か

これらが記録されていないと、結果を適切に解釈することが難しくなります。

---

## 9. Re-kNN における想定入出力

正確な形式は CLI やサンプルアプリケーションに依存しますが、概念的には以下の構造を推奨します。

### 9.1 最低限必要な登録項目

典型的な登録データには、少なくとも以下を含めます。

* `document_id`
* `sentence_id`
* token-level vectors (`float[token_count][768]`)

サンプルプログラムでは、同一センテンスから生成されたすべての token ベクトルが同じ `sentence_id` を共有します。
そのため、ベクトル欄は、センテンス内の各 token に対応する 768 次元ベクトル群として表現されます。

### 9.2 例

入力テキスト例:

```text
This is sample text.
Re-kNN stores both documentId and sentenceId.
```

概念的なデータ構造例:

```text
document_id = 0, sentence_id = 0, token_vectors = float[token_count0][768]
document_id = 0, sentence_id = 1, token_vectors = float[token_count1][768]
```


