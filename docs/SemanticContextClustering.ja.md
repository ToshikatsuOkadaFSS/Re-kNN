# Semantic Context Clustering / 類似単語のクラスタリング

数百万の大規模 word vectors に対して、Re-kNN でクラスタを生成したものを公開する。
この出力が、類義語辞書や関連語を調べる際の候補として活用できることを期待している。
ただし、この出力はあくまでも類似単語をクラスタリングした結果であり、人手で整備された辞書とは異なる点に注意されたい。

## 対象データ

本評価は、以下のデータを対象とした。

- fastText English vectors

crawl-300d-2M.vec.zip (https://dl.fbaipublicfiles.com/fasttext/vectors-english/crawl-300d-2M.vec.zip) : 2 million word vectors trained on Common Crawl (600B tokens).

## ライセンス

元データのライセンスに従い、クラスタリング結果は Creative Commons Attribution-Share-Alike License 3.0 (https://creativecommons.org/licenses/by-sa/3.0/) の下で公開する。
サンプルコード等のライセンスとは異なる点に注意すること。

## 実行結果

### 実行条件

- CPU : AMD Ryzen 9 5900HX
- Physical Memory : 64 GiB
- OS : Windows 11 Pro
- GPU : **Not used**
- Execution : Single thread
- Dataset : fastText `crawl-300d-2M.vec`
- Vector dimensions : 300
- Number of vectors : 2,000,000

### 評価条件

Refine 処理を行ったものと行っていないもの両方で評価を行った。
Refine 処理は、データ登録時に不適切な位置に配置された可能性のあるデータを再評価し、より適切な位置へ再配置する処理である。
推論時には大きな問題になりにくい場合でも、クラスタリングでは配置の揺れがクラスタ構造に影響するため、本評価では Refine 前後の結果を比較した。

### クラスタリング結果

* crawl-300d-2M.vec (After registration): [crawl-300d-2M-095-AfterRegistration.txt](../SemanticContextClustering/crawl-300d-2M-095-AfterRegistration.txt)
* crawl-300d-2M.vec (After Refine): [crawl-300d-2M-095-AfterRefine.txt](../SemanticContextClustering/crawl-300d-2M-095-AfterRefine.txt)

#### 統計情報

| Step | Time | Clusters | Avg size | Median size | Max size | Standard Deviation | Singleton clusters | Singleton Ratio |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| After registration | 6 h 44 m | 82,463 | 24.3 | 25 | 190 | 14.75 | 2,120 | 2.6% |
| After Refine | 11 h 02 m total<br>(6 h 44 m + 4 h 18 m) | 92,051 | 21.7 | 22 | 190 | 13.47 | 3,574 | 3.9% |

#### `vector` を含むクラスタの例 (After registration)

```text
array matrix pipelined divider loop loops comparators Comparators Comparator output outputs input inputs sub-arrays subarrays sub-array Array arrays Arrays carry-save subtractor 3-input three-input noninverted row-column fan-out vectors vector matrices buffers buffer FIFO partition comparitor comparator arrays.The
```

#### `vector` を含むクラスタの例 (After Refine)

```text
vectorized vectoring Bresenham Vectorizing Vectorize Vectorized vectored vectorize vectorizing vectorised Vectorization Vectored non-vector Vector-based vectorise Vector- vector vectors Vector vector-based vector- vectors.The vectorial
```


`vector` を含むクラスタについて例示した。
この結果より、Refine 処理によって、クラスタ内に含まれていた関連性が低いと推測される単語（`loop` など）が分離している。
これは、Refine 処理によりクラスタの内容がより適切に修正されたことを示唆している。

#### 最大クラスタの例 (After registration):

クラスタリング結果の完全版は `samples/largest_cluster_after_registration.txt` に保存している。

```text
28.4Km 29.2Km 28.6Km 28.3Km 27.5Km ... 4.6Km 4.2Km kmLa 0.2Km 0.3Km
```

#### 最大クラスタの例 (After Refine):
```text
28.4Km 29.2Km 28.6Km 28.3Km 27.5Km ... 4.6Km 4.2Km 0.2Km 0.3Km Péniche
```

最大クラスタは、`28.4Km`, `29.2Km`, `28.6Km` といった距離表現のトークンを主に含んでいる。
これは、Re-kNN が類似した意味、用法、または表記パターンを持つトークンをクラスタ化していることを示している。
また、ListLe, HotelsLe, kmLa といったノイズも含まれている。これは、元となった Common Crawl の文書内容が原因であると推測している。

このようなノイズは、Common Crawl 由来の表記揺れ、パースミス、複合語、言語混在トークンなどが元ベクトルに含まれているために発生したものと考えられる。
特に `Km`, `Le`, `La` などの文字列パターンを含むトークンが近い位置に配置されていることから、元コーパスおよび元ベクトルの語彙特性がクラスタに反映された可能性がある。

##### After registration でのクラスタ分布
![Cluster histogram after registration](Re-kNN_words_vector_cluster_size_histogram_before_refine.png)

##### After Refine でのクラスタ分布
![Cluster histogram after refine](Re-kNN_words_vector_cluster_size_histogram_after_refine.png)

##### クラスタ分布の比較

| クラスタサイズ | After registration | After Refine |
|---:|---:|---:|
| 1-4 | 11,386 | 13,822 |
| 5-9 | 7,511 | 8,907 |
| 10-14 | 6,373 | 8,485 |
| 15-19 | 7,289 | 9,671 |
| 20-24 | 8,247 | 10,545 |
| 25-29 | 8,459 | 10,843 |
| 30-34 | 8,627 | 10,618 |
| 35-39 | 8,480 | 9,081 |
| 40-44 | 8,014 | 6,646 |
| 45-49 | 7,950 | 3,354 |
| 50-54 | 73 | 40 |
| 55-59 | 15 | 14 |
| 60+ | 39 | 25 |

##### 消費リソース (最大値)

| Step | WorkingSet64 | PrivateMemorySize64 | PeakWorkingSet64 | GC.GetTotalMemory(false) |
|---|---:|---:|---:|---:|
| After registration | 6.19 GiB | 6.23 GiB | 6.27 GiB | 2.28 GiB |
| After Refine | 6.93 GiB | 6.97 GiB | 6.93 GiB | 2.34 GiB |

## 考察

Re-kNN は、200万件の 300 次元 fastText English word vectors から、
82,463 個の semantic clusters を 6 h 44 m で生成した。
この処理は Ryzen 9 5900HX のシングルスレッド環境で実行され、GPU は使用していない。

Refine 処理後、クラスタ数は 92,051 に増加し、平均クラスタサイズは 24.3 から 21.7 に低下した。
また、標準偏差も 14.75 から 13.47 に低下しており、Refine によってクラスタサイズ分布がやや安定したと考えられる。
Refine 処理によって、中〜大規模のクラスタの一部がより細かいクラスタに分割され、クラスタサイズ分布が全体としてやや小さい方向へ移動したことが確認できる。

Singleton Ratio は 2.6% から 3.9% に増加したが、依然として低い水準である。
これは、Refine によって一部の語がより細かく分離された一方で、大量の孤立クラスタが発生したわけではないことを示している。

最大クラスタはいずれも 190 個の word vectors であり、その内容は主に `28.4Km`, `29.2Km`, `27.5Km` などの距離表現で構成されていた。
これは、元データに含まれる表記パターンや Common Crawl 由来の語彙特性がクラスタに反映された例と考えられる。

本出力は、人手で整備された類義語辞書ではない。
類義語、関連語、表記揺れ、固有表現、コーパス由来のノイズを含む semantic clustering output である。
そのため、実用上は類義語・関連語候補の生成や、ドメイン辞書構築の初期候補として利用することが適切である。

200万件の word vectors からなる大規模データでありながら、ピーク時のメモリ使用量（PeakWorkingSet64）を 7 GiB 未満に抑え、シングルスレッドでも現実的な時間でクラスタリングを完了できた点は、本手法（Re-kNN）の省リソース性の高さを示している。


