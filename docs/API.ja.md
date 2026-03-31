# Re-kNN API Specification

Re-kNN のライブラリは、DLL の形で提供している。ここでは、そのライブラリの呼び出し方法について記述する。

## 1. 条件

本ライブラリは、win-x64 用にビルドした DLL として提供している。

**注意**: 複数スレッドからの呼び出しは推奨しません。

## 2. 基本的な構成

Re-kNN は、複数の独立した Index をメモリ上に展開することを想定して設計している。そのため、初期化時にシステムが使用するインスタンス数を指定する必要がある。

検索結果はベクトルの所属するドキュメントなどの ID を返す設計としている。そのため、ベクトル登録時に、ID (Main と Sub) を指定する必要がある。
なお、Sub は Main に従属する想定である。
MNIST 等では、Main にラベル番号、Sub にデータ番号を登録する形となる。
BERT 等では、Main に文書番号、Sub に文書内のセンテンス番号を登録する形となる。

## 3. DLL I/F 定義

DLL を呼び出すため、呼び出し側に以下の定義を追加する必要がある。このサンプルは、C# にて記述してある。
また、これを記述したコードは、リポジトリ内にある RekNNUtility.cs となる。必要に応じて参照すること。

struct の定義

```C#
    /// <summary>
    /// 動作モード
    /// </summary>
    public enum ModeEnum
    {
        BERT = 1,

        MNIST = 2,

        CIFAR10 = 3,
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SearchResult
    {
        public nuint Size;

        public int ResultItemMainNum;

        public ResultItemMain* ResultItemMains;

        public int ResultItemMainAndSubNum;

        public ResultItemMainAndSub* ResultItemMainAndSubs;

        public int ResultItemMainDetailNum;

        public ResultItemMainDetail* ResultItemMainDetails;

        public int ResultItemMainAndSubDetailNum;

        public ResultItemMainAndSubDetail* ResultItemMainAndSubDetails;
    }




    [StructLayout(LayoutKind.Sequential)]
    public struct ResultItemMain
    {
        public int MainId;

        public double Score;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ResultItemMainDetail
    {
        public int key;

        public int valueNum;

        public ResultItemMain* values;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct ResultItemMainAndSub
    {
        public int MainId;

        public int SubId;

        public double Score;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ResultItemMainAndSubDetail
    {
        public int key;

        public int valueNum;

        public ResultItemMainAndSub* values;
    }


    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PredictResult
    {
        public int ScoreListMainNum;

        public ResultItemMain* ScoreListMain;

        public int ScoreListMainAndSubNum;

        public ResultItemMainAndSub* ScoreListMainAndSub;

        public int PredictedLabel;
    }

    public class SearchResultForManaged
    {
        public List<ResultItemMain> ResultItemMains { get; } = new();

        public List<ResultItemMainAndSub> ResultItemMainAndSubs { get; } = new();

        public Dictionary<int, List<ResultItemMain>> ResultItemMainDetails { get; } = new();

        public Dictionary<int, List<ResultItemMainAndSub>> ResultItemMainAndSubDetails { get; } = new();
    }
```

DLL 呼び出しの定義
```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern bool Initialize(ModeEnum mode, int instanceNum);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool Load(int instanceNo, byte* utf8Text, int textLength);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool Add(int instanceNo, float* vec, int length, int mainId, int subId, int searchMax, double threshold);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool Refine(int instanceNo, float* vec, int length, int mainId, int subId, int searchMax, double threshold);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern int Delete(int instanceNo, int mainId, int subId);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe SearchResult* Search(int instanceNo, float* vec, int length, int kValue);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe double GetTotalVector(int instanceNo);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool Save(int instanceNo, byte* utf8Text, int textLength);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool SaveWithCount(int instanceNo, byte* utf8Text, int textLength, int count);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe SearchResult* SimpleClustering(int instanceNo);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe PredictResult* Predict(int instanceNo, float* vec, int length, int kValue, double detectThreshold);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool IsNeedRefine(int instanceNo, float* vec, int length, int searchMax, int mainId, int subId);
```

## 4. 一覧

| 関数名 | 機能 |
| :--- | :--- |
| [Initialize](#Initialize) | Re-kNN の初期化 |
| [Load](#Load) | 保存したDBの読み込み |
| [Add](#Add) | ベクトル情報の追加 |
| [Refine](#Refine) | Index 構造の最適化 |
| [Delete](#Delete) | ベクトル情報の削除 | 
| [Search](#Search) | ベクトル情報の検索 |
| [GetTotalVector](#GetTotalVector) | 登録されているベクトル数を獲得 |
| [Save](#Save) | DB をストレージに保存 |
| [SaveWithCount](#SaveWithCount) | DB をカウント情報付きで保存(デバッグ用・非推奨) |
| [SimpleClustering](#SimpleClustering) | クラスタ情報の獲得 |
| [Predict](#Predict) | 指示ベクトルでの推論 |
| [IsNeedRefine](#IsNeedRefine) | ベクトル群が Refine 対象であるかを確認 |

----

### Initialize

指示したインスタンス数だけの独立したインデックス領域を確保する。
試行版では、BERT (768次元), MNIST(784次元), CIFAR10(3072次元) のいずれかから選択する。

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern bool Initialize(ModeEnum mode, int instanceNum);
```

| 引数の型 | 引数名 | 意味 |
| :--- | :--- | :--- |
| ModeEnum | mode | 初期化モード(BERT, MNIST, CIFAR10 のいずれか) |

| 戻り値 | 意味 |
| :--- | :--- |
| true | 初期化成功 |
| false | 初期化失敗 |

----

### Load 

指示したインスタンスに、指示したストレージからインデックス情報を読み込む。

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool Load(int instanceNo, byte* utf8Text, int textLength);
```

| 引数の型 | 引数名 | 意味 |
| :--- | :--- | :--- |
| int | instanceNo | インスタンス番号 |
| byte* | utf8Text | DB を保存したフォルダのパス名 |
| int | textLength | utf8Text の文字列長 |

| 戻り値 | 意味 |
| :--- | :--- |
| true | 読み込み成功 |
| false | 読み込み失敗 |

----

### Add 

指示したインスタンスに、ベクトル情報を追加する。

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool Add(int instanceNo, float* vec, int length, int mainId, int subId, int searchMax, double threshold);
```

| 引数の型 | 引数名 | 意味 |
| :--- | :--- | :--- |
| int | instanceNo | インスタンス番号 |
| float* | vec | 登録するベクトル(length個の初期化時に指定したサイズのベクトル) |
| int | length | 登録するベクトルの数 |
| int | mainId | 登録するベクトルのメインID |
| int | subId | 登録するベクトルのサブID |
| int | searchMax | 登録先を検索する際の探索幅(5を推奨) |
| double  | threshold | インデックスの一致判定閾値 |

| 戻り値 | 意味 |
| :--- | :--- |
| true | 登録成功 |
| false | 登録失敗 |

----

### Refine

指示したインスタンスの、指示したベクトル情報のインデックス構造を最適化する。

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool Refine(int instanceNo, float* vec, int length, int mainId, int subId, int searchMax, double threshold);
```

| 引数の型 | 引数名 | 意味 |
| :--- | :--- | :--- |
| int | instanceNo | インスタンス番号 |
| float* | vec | 最適化するベクトル(length個の初期化時に指定したサイズのベクトル) |
| int | length | 登録するベクトルの数 |
| int | mainId | 登録するベクトルのメインID |
| int | subId | 登録するベクトルのサブID |
| int | searchMax | 登録先を検索する際の探索幅(5を推奨) |
| double | threshold | インデックスの一致判定閾値 |

| 戻り値 | 意味 |
| :--- | :--- |
| true | 最適化を実行した |
| false | 最適化を実行しなかった(最適化不要の場合を含む) |

----

### Delete

指示したインスタンスから、指示したベクトル情報を削除する。

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern int Delete(int instanceNo, int mainId, int subId);
```

| 引数の型 | 引数名 | 意味 |
| :--- | :--- | :--- |
| int | instanceNo | インスタンス番号 |
| int | mainId | 登録するベクトルのメインID |
| int | subId | 登録するベクトルのサブID |

| 戻り値 | 意味 |
| :--- | :--- |
| 数値 | 削除したベクトルの数 |

----

### Search

指示したインスタンスに対して、指示したベクトル情報で検索を行う。
引数で指示した kValue の個数とは異なるデータが得られる場合がある。

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe SearchResult* Search(int instanceNo, float* vec, int length, int kValue);
```

| 引数の型 | 引数名 | 意味 |
| :--- | :--- | :--- |
| int | instanceNo | インスタンス番号 |
| float* | vec | 検索するベクトル(length個の初期化時に指定したサイズのベクトル) |
| int | length | 登録するベクトルの数 |
| int | kValue | 探索範囲 |

| 戻り値 | 意味 |
| :--- | :--- |
| ! null | 検索に成功した。詳細情報は構造体にあるのでそこを解析する。 **呼び出し側で解放が必要** |
| null | 検索に失敗した。 |

以下にメモリ解放処理の例を示す。先頭のポインタを解放するだけで良い。

```C#
SearchResult* result = null;

try
{
    // your codes here

    result = Search(0, vector, len, k);

    // your codes here
}
finally
{
    if ( result != null)
    {
        NativeMemory.Free(result);
    }
}
```

----

### GetTotalVector 

指示したインスタンスに対して、登録されているベクトル数を求める。

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe double GetTotalVector(int instanceNo);
```

| 引数の型 | 引数名 | 意味 |
| :--- | :--- | :--- |
| int | instanceNo | インスタンス番号 |

| 戻り値 | 意味 |
| :--- | :--- |
| - | 登録されているベクトル数 |

----

### Save

指示したインスタンスのインデックス情報を、指示したストレージに書き込む。

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool Save(int instanceNo, byte* utf8Text, int textLength);
```

| 引数の型 | 引数名 | 意味 |
| :--- | :--- | :--- |
| int | instanceNo | インスタンス番号 |
| byte* | utf8Text | DB を保存するフォルダのパス名 |
| int | textLength | utf8Text の文字列長 |

| 戻り値 | 意味 |
| :--- | :--- |
| true | 書き込み成功 |
| false | 書き込み失敗 |

----

### SaveWithCount 

指示したインスタンスのインデックス情報を、カウント情報付きで指示したストレージに書き込む。

** 注意 **: 本機能の利用は推奨しない。本機能を利用しても、DBは完全に分離した形では保存されない。

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool SaveWithCount(int instanceNo, byte* utf8Text, int textLength, int count);
```

| 引数の型 | 引数名 | 意味 |
| :--- | :--- | :--- |
| int | instanceNo | インスタンス番号 |
| byte* | utf8Text | DB を保存するフォルダのパス名 |
| int | textLength | utf8Text の文字列長 |
| int | count | カウント情報(ファイル名生成時に利用) |

| 戻り値 | 意味 |
| :--- | :--- |
| true | 書き込み成功 |
| false | 書き込み失敗 |

----

### SimpleClustering 

指示したインスタンスから、呼び出し時点でのクラスタ情報を獲得する。

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe SearchResult* SimpleClustering(int instanceNo);
```

| 引数の型 | 引数名 | 意味 |
| :--- | :--- | :--- |
| int | instanceNo | インスタンス番号 |

| 戻り値 | 意味 |
| :--- | :--- |
| ! null | クラスタ情報(SearchResult 構造体を確認する) **呼び出し側で解放が必要** |
| null | クラスタ情報獲得失敗 |

以下にメモリ解放処理の例を示す。先頭のポインタを解放するだけで良い。

```C#
SearchResult* result = null;

try
{
    // your codes here

    result = SimpleClustering(0);

    // your codes here
}
finally
{
    if ( result != null)
    {
        NativeMemory.Free(result);
    }
}
```

----

### Predict 

指示したインスタンスに対して、指示したベクトルがどこに所属するかを推論する。
本推論エンジンでは、推論の根拠情報も返す。これにより、判断の妥当性の検証が可能となる。

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe PredictResult* Predict(int instanceNo, float* vec, int length, int kValue, double detectThreshold);
```

| 引数の型 | 引数名 | 意味 |
| :--- | :--- | :--- |
| int | instanceNo | インスタンス番号 |
| float* | vec | 推論するベクトル(length個の初期化時に指定したサイズのベクトル) |
| int | length | 登録するベクトルの数 |
| int | kValue | 投票対象とする類似データの数 |
| double | detectThreshold | 投票結果を採用するための閾値。閾値を超え、最大の評価値を持つものを推論結果とする。閾値を超えるデータが無い場合は「未知」と判断する。 |

| 戻り値 | 意味 |
| :--- | :--- |
| ! null | 推論情報(PredictResult 構造体を確認する) **呼び出し側で解放が必要** |
| null | 推論失敗(未知判定はこちらに含む) |

以下にメモリ解放処理の例を示す。先頭のポインタを解放するだけで良い。

```C#
PredictResult* result = null;

try
{
    // your codes here

    result = Predict(0, vector, len, k, th);

    // your codes here
}
finally
{
    if ( result != null)
    {
        NativeMemory.Free(result);
    }
}
```

----

### IsNeedRefine 

指示したインスタンスに対して、指示ベクトルが最適化対象であるかどうかを問い合わせる。

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool IsNeedRefine(int instanceNo, float* vec, int length, int searchMax, int mainId, int subId);
```

| 引数の型 | 引数名 | 意味 |
| :--- | :--- | :--- |
| int | instanceNo | インスタンス番号 |
| float* | vec | 登録するベクトル(length個の初期化時に指定したサイズのベクトル) |
| int | length | 登録するベクトルの数 |
| int | searchMax | 登録先を検索する際の探索幅(5を推奨) |
| int | mainId | 登録するベクトルのメインID |
| int | subId | 登録するベクトルのサブID |

| 戻り値 | 意味 |
| :--- | :--- |
| true | 最適化対象である |
| false | 最適化対象ではない |

