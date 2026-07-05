# Python 用サンプルアプリケーション

Re-kNN を Python で利用するためのライブラリ及びサンプルプログラムについて解説します。

## 前提

### 環境

Python 3.12 で動作確認を行っています。また、ライブラリの組み合わせで問題が発生する可能性もあります。
適用する環境に合わせて、必要に応じて修正を行ってください。

### パッケージ

現時点では評価用としての提供です。そのため、ローカルフォルダからの導入が必要となります。

### ライセンス

サンプルコードおよびドキュメントに該当するため、**Apache License 2.0** の下で提供されています。


## 準備

コマンドラインで、package を導入してください。

> pip install -e <reknn-path>

サンプル)
> pip install -e ./reknn

## コード例

以下のような形で利用してください

```Python
from reknn import ReKNN

model = ReKNN('bert')
```

## サンプルプログラムについて

指示したフォルダ以下にあるテキストファイルをトークン化して検索するというサンプルです。
サンプルプログラムでは、評価用途としてファイルの登録、更新、削除に対応しています。
あくまでもサンプルコードなので、これを参考に自身のプログラムを作成してください。

### 使用方法

#### 修正項目

以下の記述を環境に合わせて修正してください。
このサンプルでは、日本語を ja, 英語を en として環境を設定しています。
また、basePath に元となるテキストファイルを格納したフォルダを指定してください。


##### 環境設定関連

```Python
if lang_mode == 'ja':
    model_name = "tohoku-nlp/bert-base-japanese-v3"
    tokenizer = AutoTokenizer.from_pretrained(model_name)
    bert_model = AutoModel.from_pretrained(model_name)
    db_path = 'db-ja'
    dict_path = 'textDict-ja.json'
    cluster_path = 'cluster-ja.json'
    basePath = Path("/local/tokada/jawikiout-txt")

elif lang_mode == 'en':
    #model_name = "./bert-base-uncased"
    model_name = "google-bert/bert-base-uncased"
    tokenizer = AutoTokenizer.from_pretrained(model_name)
    bert_model = AutoModel.from_pretrained(model_name)
    db_path = 'db-en'
    dict_path = 'textDict-en.json'
    cluster_path = 'cluster-en.json'
    basePath = Path("/local/tokada/enwikiout-txt")
else:
    print(f"bad lang_mode={lang_mode}")
    exit()
```

##### テスト動作関連

ここでは、検索する際の key を設定しています。ja のみコードが記述してあります。
必要に応じて修正してください。

```Python
elif mode == 'test':
    if lang_mode == 'ja':
        for keywords in ['週刊少年サンデー', '聖☆おにいさん']:
            print("##### keywords = {0} #####".format(keywords))
            print("### Search ###")
            results = search(model, bert_model, keywords, 5, text_rev_dict)
            print("### Explain ###")
            results = explain(model, bert_model, keywords, 5, text_rev_dict)
    elif lang_mode == 'en':
        pass
```


### Version 1.2.1 での更新内容

- Python のパッケージに、クラスタリング結果出力機能を追加しました。
- Python のパッケージに、mainId のみでデータを削除する機能を追加しました。
- Python 用のサンプルアプリケーションにファイルの更新や削除に対応する機能を追加しました。
- 検索結果の詳細情報の出力処理に問題があったため修正しました。


### 制限事項

現時点では以下の制限事項があります。順次対応予定です。

- Re-kNN の全ての機能が実装されているわけではありません。




