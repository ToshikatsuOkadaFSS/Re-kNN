# Re-kNN Evaluation Report: CIFAR-10

> **"Deterministic logic reveals the truth hidden in high-dimensional noise."**

This report presents the results of evaluating Re-kNN inference performance on the CIFAR-10 dataset (10,000 test samples) for each unknown-detection threshold (decision threshold).

## Experimental Conditions

Unless otherwise noted, the results in this report were obtained under the following conditions.

- Indexed data: 50,000 CIFAR-10 samples
- Evaluation data: 10,000 CIFAR-10 test samples
- Number of neighbors: k = 1
- Similarity used at index construction: 0.98
- Unknown-detection threshold (decision threshold): shown in each table
- Refine: none
- Execution environment: Ryzen 9 5900HX / Windows 11 / .NET 8

For CIFAR-10, Refine is not applied because of the characteristics of the dataset.

### Note

This evaluation reflects results obtained by treating images directly as vectors. No image-specific processing, such as feature extraction, has been applied.

## 1. Re-kNN Performance

| Unknown-Detection Threshold (decision threshold) | Accuracy (Total Acc) | Unknown Rate | Misclassification Rate | Accuracy excluding Unknown | Correct | Unknown | Misclassified | Notes |
|:---|:---|:---|:---|:---|---:|---:|---:|:---|
| **0.5** | 0.3272 | 0.0000 | 0.6728 | 0.3272 | 3272 | 0 | 6728 | Intended for practical use cases |
| **0.7** | 0.3272 | 0.0003 | 0.6725 | 0.3273 | 3272 | 3 | 6725 | Reliability-oriented setting |
| **0.9** | 0.2957 | 0.1330 | 0.5713 | 0.3411 | 2957 | 1330 | 5713 | High-reliability setting |

> * Accuracy (Total Acc) = Correct / Total number of test samples (10,000)  
> * Accuracy excluding Unknown = Correct / (Correct + Misclassified)  
> * Unknown Rate = Unknown / Total number of test samples (10,000)  
> * Misclassification Rate = Misclassified / Total number of test samples (10,000)

![Re-kNN_CIFAR10_Performance_vs_Detection_Threshold_Similarity=0.98_Refine=none](Re-kNN_CIFAR10_Performance_vs_Detection_Threshold_Similarity=0.98_Refine=none.png)

In CIFAR-10, increasing the unknown-detection threshold from 0.5 to 0.7 causes almost no change in total accuracy and only a very small increase in Unknown predictions. On the other hand, raising it to 0.9 increases the Unknown rate, while the accuracy among samples judged as known improves slightly.

### Confusion Matrices

The confusion matrices are shown below.

#### Unknown-Detection Threshold: 0.5

| Test Label | Pred:0 | Pred:1 | Pred:2 | Pred:3 | Pred:4 | Pred:5 | Pred:6 | Pred:7 | Pred:8 | Pred:9 | Pred:Unknown |
|:--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|
| 0 | 328 | 30 | 59 | 18 | 37 | 17 | 115 | 52 | 184 | 21 | 139 |
| 1 | 65 | 200 | 33 | 53 | 43 | 35 | 67 | 36 | 145 | 71 | 252 |
| 2 | 81 | 9 | 167 | 70 | 157 | 37 | 152 | 61 | 40 | 5 | 221 |
| 3 | 48 | 12 | 67 | 152 | 103 | 109 | 134 | 50 | 45 | 16 | 264 |
| 4 | 64 | 8 | 100 | 63 | 286 | 33 | 164 | 60 | 30 | 8 | 184 |
| 5 | 35 | 8 | 81 | 131 | 105 | 184 | 94 | 52 | 44 | 14 | 252 |
| 6 | 28 | 13 | 68 | 97 | 155 | 65 | 275 | 45 | 24 | 5 | 225 |
| 7 | 52 | 7 | 75 | 64 | 134 | 56 | 78 | 242 | 46 | 29 | 217 |
| 8 | 141 | 44 | 16 | 20 | 25 | 9 | 56 | 24 | 472 | 19 | 174 |
| 9 | 89 | 67 | 40 | 58 | 51 | 26 | 54 | 44 | 137 | 196 | 238 |

#### Unknown-Detection Threshold: 0.7

| Test Label | Pred:0 | Pred:1 | Pred:2 | Pred:3 | Pred:4 | Pred:5 | Pred:6 | Pred:7 | Pred:8 | Pred:9 | Pred:Unknown |
|:--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|
| 0 | 252 | 16 | 29 | 8 | 17 | 8 | 111 | 23 | 133 | 8 | 395 |
| 1 | 38 | 121 | 17 | 20 | 12 | 14 | 52 | 16 | 93 | 30 | 587 |
| 2 | 50 | 5 | 97 | 28 | 99 | 17 | 127 | 29 | 17 | 1 | 530 |
| 3 | 29 | 5 | 31 | 76 | 60 | 42 | 86 | 25 | 31 | 5 | 610 |
| 4 | 37 | 4 | 66 | 22 | 184 | 19 | 129 | 38 | 22 | 5 | 474 |
| 5 | 17 | 4 | 53 | 59 | 48 | 96 | 65 | 25 | 25 | 2 | 606 |
| 6 | 15 | 6 | 33 | 46 | 105 | 22 | 199 | 27 | 14 | 2 | 531 |
| 7 | 33 | 5 | 47 | 28 | 77 | 26 | 62 | 160 | 27 | 15 | 520 |
| 8 | 89 | 18 | 9 | 9 | 14 | 4 | 54 | 17 | 342 | 7 | 437 |
| 9 | 49 | 27 | 18 | 22 | 28 | 10 | 43 | 18 | 92 | 115 | 578 |

#### Unknown-Detection Threshold: 0.9

| Test Label | Pred:0 | Pred:1 | Pred:2 | Pred:3 | Pred:4 | Pred:5 | Pred:6 | Pred:7 | Pred:8 | Pred:9 | Pred:Unknown |
|:--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|
| 0 | 133 | 4 | 5 | 1 | 4 | 2 | 85 | 9 | 53 | 2 | 702 |
| 1 | 5 | 58 | 3 | 1 | 0 | 0 | 34 | 2 | 26 | 9 | 862 |
| 2 | 14 | 1 | 41 | 5 | 29 | 2 | 101 | 4 | 5 | 0 | 798 |
| 3 | 3 | 1 | 11 | 28 | 14 | 14 | 62 | 8 | 6 | 1 | 852 |
| 4 | 15 | 1 | 20 | 5 | 80 | 2 | 96 | 13 | 14 | 2 | 752 |
| 5 | 7 | 0 | 12 | 21 | 17 | 57 | 48 | 5 | 12 | 1 | 820 |
| 6 | 4 | 4 | 9 | 8 | 33 | 4 | 137 | 3 | 6 | 1 | 791 |
| 7 | 12 | 0 | 9 | 5 | 18 | 9 | 48 | 104 | 8 | 5 | 782 |
| 8 | 26 | 4 | 1 | 4 | 2 | 1 | 48 | 6 | 196 | 4 | 708 |
| 9 | 12 | 7 | 2 | 2 | 2 | 2 | 30 | 5 | 22 | 76 | 840 |

## 2. Cases Misclassified by Re-kNN

Below are analyses of misclassified cases observed when the unknown-detection threshold was set to 0.7.

Total number of misclassified cases: 6725

### Example 1: A sample with true label `cat` misclassified as `horse`

#### Test Label
![cifar10-miss-0-testLabel](cifar10-miss-0-testLabel.png)

#### Neighbor Data Used as Evidence
![cifar10-miss-0-voteLabel](cifar10-miss-0-voteLabel.png)

### Example 2: A sample with true label `frog` misclassified as `cat`

#### Test Label
![cifar10-miss-4-testLabel](cifar10-miss-4-testLabel.png)

#### Neighbor Data Used as Evidence
![cifar10-miss-4-voteLabel](cifar10-miss-4-voteLabel.png)

### Example 3: A sample with true label `automobile` misclassified as `frog`

#### Test Label
![cifar10-miss-6-testLabel](cifar10-miss-6-testLabel.png)

#### Neighbor Data Used as Evidence
![cifar10-miss-6-voteLabel](cifar10-miss-6-voteLabel.png)

## 3. Cases Classified as Unknown by Re-kNN

Below are analyses of cases classified as Unknown when the unknown-detection threshold was set to 0.7.

Total number of Unknown cases: 3

### Example 1: A sample with true label `truck` classified as Unknown

#### Test Label
![cifar10-unknown-2754-testLabel](cifar10-unknown-2754-testLabel.png)

#### Neighbor Data Used as Evidence
![cifar10-unknown-2754-voteLabel](cifar10-unknown-2754-voteLabel.png)

### Example 2: A sample with true label `airplane` classified as Unknown

#### Test Label
![cifar10-unknown-6885-testLabel](cifar10-unknown-6885-testLabel.png)

#### Neighbor Data Used as Evidence
![cifar10-unknown-6885-voteLabel](cifar10-unknown-6885-voteLabel.png)

### Example 3: A sample with true label `cat` classified as Unknown

#### Test Label
![cifar10-unknown-9246-testLabel](cifar10-unknown-9246-testLabel.png)

#### Neighbor Data Used as Evidence
![cifar10-unknown-9246-voteLabel](cifar10-unknown-9246-voteLabel.png)

## 4. Cases Correctly Classified by Re-kNN

Below are analyses of correctly classified cases observed when the unknown-detection threshold was set to 0.7.

Total number of correctly classified cases: 3272

### Example 1: A sample with true label `ship` correctly classified

#### Test Label
![cifar10-correct-1-testLabel](cifar10-correct-1-testLabel.png)

#### Neighbor Data Used as Evidence
![cifar10-correct-1-voteLabel](cifar10-correct-1-voteLabel.png)

### Example 2: A sample with true label `ship` correctly classified

#### Test Label
![cifar10-correct-2-testLabel](cifar10-correct-2-testLabel.png)

#### Neighbor Data Used as Evidence
![cifar10-correct-2-voteLabel](cifar10-correct-2-voteLabel.png)

### Example 3: A sample with true label `airplane` correctly classified

#### Test Label
![cifar10-correct-3-testLabel](cifar10-correct-3-testLabel.png)

#### Neighbor Data Used as Evidence
![cifar10-correct-3-voteLabel](cifar10-correct-3-voteLabel.png)

## 5. Inference Time

The processing time for running inference on all CIFAR-10 test samples (10,000 samples) is shown below.

* **Measurement Environment**: Ryzen 9 5900HX (3.3GHz) / Windows 11 / .NET 8
* **Target Data**: CIFAR-10 (3072 dimensions)
* **Total Processing Time (10,000 samples)**: $27,369.73 ms$
* **Average Latency per Sample**: **$2.74 ms$**

## 6. Unknown Detection

The following results were obtained by evaluating CIFAR-10 while excluding each label from index registration, one at a time. These results were measured with the unknown-detection threshold set to 0.7 and similarity used at index construction set to 0.95.

### Evaluation Results Including Unknown Predictions

| Unknown Label | Accuracy (Total Acc) | Unknown Rate | Misclassification Rate | Correct | Unknown | Misclassified | Notes |
|:---:|:---|:---|:---|---:|---:|---:|:---|
| 0 | 0.1615 | 0.5489 | 0.2896 | 1615 | 5489 | 2896 |  |
| 1 | 0.1565 | 0.5422 | 0.3013 | 1565 | 5422 | 3013 |  |
| 2 | 0.1724 | 0.5233 | 0.3043 | 1724 | 5233 | 3043 |  |
| 3 | 0.1765 | 0.5200 | 0.3035 | 1765 | 5200 | 3035 |  |
| 4 | 0.1678 | 0.5274 | 0.3048 | 1678 | 5274 | 3048 |  |
| 5 | 0.1696 | 0.5256 | 0.3048 | 1696 | 5256 | 3048 |  |
| 6 | 0.1553 | 0.5387 | 0.3060 | 1553 | 5387 | 3060 |  |
| 7 | 0.1629 | 0.5197 | 0.3174 | 1629 | 5197 | 3174 |  |
| 8 | 0.1502 | 0.5586 | 0.2912 | 1502 | 5586 | 2912 |  |
| 9 | 0.1528 | 0.5429 | 0.3043 | 1528 | 5429 | 3043 |  |
| none | 0.1642 | 0.5268 | 0.3090 | 1642 | 5268 | 3090 |  |

![CIFAR10_TotalScore_includeexcludedlabel(Accuracy)](CIFAR10_TotalScore_includeexcludedlabel(Accuracy).png)

![CIFAR10_TotalScore_includeexcludedlabel(unknown,miss)](CIFAR10_TotalScore_includeexcludedlabel(unknown,miss).png)

### Evaluation Results Excluding Unknown Predictions

| Unknown Label | Accuracy (Total Acc) | Accuracy excluding Unknown | Correct | Misclassified | Unknown | Notes |
|:---:|:---|:---|---:|---:|---:|:---|
| 0 | 0.1794 | 0.4019 | 1615 | 2403 | 4982 |  |
| 1 | 0.1739 | 0.3702 | 1565 | 2662 | 4773 |  |
| 2 | 0.1916 | 0.4034 | 1724 | 2550 | 4726 |  |
| 3 | 0.1961 | 0.4017 | 1765 | 2629 | 4606 |  |
| 4 | 0.1864 | 0.3943 | 1678 | 2578 | 4744 |  |
| 5 | 0.1884 | 0.3898 | 1696 | 2655 | 4649 |  |
| 6 | 0.1726 | 0.3711 | 1553 | 2632 | 4815 |  |
| 7 | 0.1810 | 0.3716 | 1629 | 2755 | 4616 |  |
| 8 | 0.1669 | 0.3811 | 1502 | 2439 | 5059 |  |
| 9 | 0.1698 | 0.3682 | 1528 | 2622 | 4850 |  |
| none | 0.1642 | 0.3470 | 1642 | 3090 | 5268 |  |

![CIFAR10_TotalScore_withoutexcludedlabel(Accuracy)](CIFAR10_TotalScore_withoutexcludedlabel(Accuracy).png)

![CIFAR10_TotalScore_withoutexcludedlabel(Unknown,miss)](CIFAR10_TotalScore_withoutexcludedlabel(Unknown,miss).png)

### Evaluation Results for Unknown Labels

For labels that were not registered in the index (i.e., unknown labels), the result must be either **Unknown** or **misclassified**.

| Unknown Label | Unknown Rate | Misclassification Rate | Unknown | Misclassified | Notes |
|:---:|:---|:---|---:|---:|:---|
| 0 | 0.507 | 0.493 | 507 | 493 |  |
| 1 | 0.649 | 0.351 | 649 | 351 |  |
| 2 | 0.507 | 0.493 | 507 | 493 |  |
| 3 | 0.594 | 0.406 | 594 | 406 |  |
| 4 | 0.530 | 0.470 | 530 | 470 |  |
| 5 | 0.607 | 0.393 | 607 | 393 |  |
| 6 | 0.572 | 0.428 | 572 | 428 |  |
| 7 | 0.581 | 0.419 | 581 | 419 |  |
| 8 | 0.527 | 0.473 | 527 | 473 |  |
| 9 | 0.579 | 0.421 | 579 | 421 |  |

![CIFAR10_UnknownLabelScore(byRate)](CIFAR10_UnknownLabelScore(byRate).png)

## 7. Discussion

With CIFAR-10, accuracy remains low when raw pixel vectors are used directly. One likely reason is that, for a 3,072-dimensional space, a dataset size of 50,000 is not dense enough.

In Re-kNN, high-scoring search results are used as evidence for the final decision. In the misclassified cases observed here, the high-scoring nearest neighbors belonged to different labels. In other words, sufficiently similar data with the same label may not have existed in the local neighborhood. This suggests that the data density in the space is sparse.

Because of these data characteristics, the scores used for Unknown detection also tend to remain low. This may indicate not so much a limitation specific to Re-kNN itself as a strong effect of the data characteristics caused by using CIFAR-10 directly as raw pixel vectors.

## 8. Memory Consumption

The memory consumption after loading all CIFAR-10 data is shown below.

* **Measurement Environment**: Ryzen 9 5900HX (3.3GHz) / Windows 11 / .NET 8
* **Measurement Condition**: Immediately after loading all index information
* **Other Running Processes**: Other Windows processes were active during measurement
* **Target Data**: CIFAR-10 (3072 dimensions)
* **Memory Consumption**: 2,000.0 MB (measured from the Memory column in the Task Manager process tab, provisional value)

---
*Created by Re-kNN Evaluation Suite (2026)*
