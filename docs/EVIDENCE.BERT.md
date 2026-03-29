# Re-kNN Evaluation Report: BERT

> **"Deterministic logic reveals the truth hidden in high-dimensional noise."**

This report evaluates BERT-tokenized articles extracted from Wikipedia JP.
In this evaluation, all tokens are registered. Therefore, when indexing 10,000 Wikipedia articles, the total number of vectors handled reaches approximately 11.6 million.
This evaluation targets raw vector information without chunking or similar preprocessing.

## 1. Evaluation Conditions

Unless otherwise noted, the results in this report were obtained under the following conditions.

- BERT model: `bert-base-multilingual-cased`
- Evaluation text data: text extracted from Wikipedia JP (10,000 articles)
- Number of neighbors: k = 5
- Execution environment: Ryzen 9 5900HX / Windows 11 / .NET 8

### Notes

At present, this library performs search in a single-threaded manner. 
No distributed or parallelized search processing has been implemented.

## 2. Evaluation Setup

The evaluation uses searches targeting the Wikipedia JP article for *Saint Young Men* (`聖☆おにいさん`), and the resulting outputs are used as evidence.

### Query Sentences

The following three sentences are used as search queries.
Because the target data consists of Wikipedia JP articles, the text is also presented in Japanese.

| No. | Number of Vectors | Text |
| ---: | ---: | :--- |
| 1 | 101 | 世紀末を無事に乗り越えたブッダとイエスが有休を取得して下界でのバカンスを満喫しようと、日本の東京都立川の安アパート「松田ハイツ」の一室に居を構え、「聖」（せい）という名字でルームシェアして暮らすという設定で描かれる日常コメディ。 |
| 2 | 96 | 本人は痩せていたいらしいが、天部は「ブッダははんぺん系（太っている）がいい」と考えているらしく、毎回ダイエットしたい彼自身とあの手この手で栄養補給（食べないなら毛穴から直接流し込む）をさせる天部という熾烈な争いが繰り広げられている。 |
| 3 | 114 | 初めてイエスに会った際、信仰の証として自分が踏むことを拒んだ踏み絵をイエスに渡すが、踏み絵の意味を知らずに勘違いしたイエスに踏み絵を玄関マットにされて号泣する（現在ではイエスも正しく理解しており、立川で消防団に協力した際は泣きながら踏み絵を拒まないよう訴えた）。 |

### Evaluation Items

The following items are evaluated.

* **Sentence level**: Whether the exact same sentence as the query appears in the Top-5 search results. If it does, its score is also recorded.
* **Article level**: Whether the article from which the query sentence was extracted appears in the Top-5 search results. If it does, its score is also recorded.

## 3. Evaluation Results

The evaluation was conducted under the following parameter settings.

| Item | Value |
| :---: | ---: |
| Similarity used at index construction | 0.8, 0.9 |
| Refine | none, once |

### Sentence-Level Evaluation

| Similarity used at index construction | Refine | Query No. | Score | Response Time ( $ms$ ) | Response Time per Vector ( $ms / vector$ ) |
| ---: | :---: | ---: | ---: | ---: | ---: |
| 0.8 | none | 1 | - | 742.13 | 7.35 |
| 0.8 | none | 2 | - | 668.19 | 6.96 |
| 0.8 | none | 3 | - | 990.05 | 8.68 |
| 0.8 | 1 | 1 | - | 1039.69 | 10.29 |
| 0.8 | 1 | 2 | - | 716.74 | 7.47 |
| 0.8 | 1 | 3 | - | 1431.46 | 12.56 |
| 0.9 | none | 1 | 1.85 | 676.90 | 6.70 |
| 0.9 | none | 2 | 17.50 | 194.51 | 2.03 |
| 0.9 | none | 3 | - | 515.52 | 4.52 |
| 0.9 | 1 | 1 | 2.15 | 668.02 | 6.61 |
| 0.9 | 1 | 2 | 2.08 | 208.49 | 2.17 |
| 0.9 | 1 | 3 | 6.88 | 503.85 | 4.42 |

The score is a custom metric defined specifically for this evaluation. It may be regarded as an aggregate derived from the search results. For details, please refer to the code in the repository.

Although some decreases in score can be observed, when the index-construction Similarity is 0.9 and Refine is applied, all three query sentences appear in the Top-5 results.
This suggests that Refine may contribute to improving index accuracy.

When the Similarity used at index construction is set to 0.8, the target sentence is not retrieved among the top candidates. This is considered to be due to reduced search resolution.

### Article-Level Evaluation

| Similarity used at index construction | Refine | Query No. | Score | Response Time ( $ms$ ) | Response Time per Vector ( $ms / vector$ ) |
| ---: | :---: | ---: | ---: | ---: | ---: |
| 0.8 | none | 1 | 20.36 | 742.13 | 7.35 |
| 0.8 | none | 2 | 11.23 | 668.19 | 6.96 |
| 0.8 | none | 3 | - | 990.05 | 8.68 |
| 0.8 | 1 | 1 | 5.79 | 1039.69 | 10.29 |
| 0.8 | 1 | 2 | 16.87 | 716.74 | 7.47 |
| 0.8 | 1 | 3 | - | 1431.46 | 12.56 |
| 0.9 | none | 1 | 19.90 | 676.90 | 6.70 |
| 0.9 | none | 2 | 23.51 | 194.51 | 2.03 |
| 0.9 | none | 3 | 21.27 | 515.52 | 4.52 |
| 0.9 | 1 | 1 | 15.33 | 668.02 | 6.61 |
| 0.9 | 1 | 2 | 9.32 | 208.49 | 2.17 |
| 0.9 | 1 | 3 | 38.67 | 503.85 | 4.42 |

When the Similarity used at index construction is 0.9, the articles containing the query sentences appear in the Top-5 results in all cases, regardless of whether Refine is applied.
The scores show both increases and decreases, but this is considered to be influenced by how frequently similar token sequences appear in other documents, so no detailed analysis has been performed at this stage.
Based on these results, token-based search using Re-kNN appears to function effectively.

### Discussion on Similarity Used at Index Construction

When the Similarity used at index construction is reduced to 0.8, search accuracy deteriorates.
At least in the case of BERT, the results suggest that lowering this Similarity to 0.8 is not desirable.

### Discussion on Processing Time

No major change in response time is observed due to Refine. Although there may be a tendency for response time to improve due to better index structure, it is considered premature to draw a conclusion at this stage.

However, when the Similarity used at index construction is 0.8, an increase in search response time is observed. It is assumed that a rougher data structure has a negative effect on performance.

## 4. Additional Information

### Memory Consumption

| Similarity used at index construction | Refine | Memory Consumption (MB) |
| ---: | :---: | ---: |
| 0.8 | none | 4,026.6 |
| 0.8 | 1 | 4,526.4 |
| 0.9 | none | 9,921.2 |
| 0.9 | 1 | 10,776.1 |

It is assumed that Refine improves the index structure, and that this leads to increased memory consumption.
When the Similarity used at index construction is smaller, more data are grouped together, which reduces memory consumption.



