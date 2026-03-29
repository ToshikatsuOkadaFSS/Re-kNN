# Re-kNN Evaluation Report: MNIST

> **"Deterministic logic reveals the truth hidden in high-dimensional noise."**

This report presents the results of evaluating Re-kNN inference performance on the MNIST handwritten digit dataset (10,000 test samples) for each unknown-detection threshold.

## Experimental Conditions

Unless otherwise noted, the results in this report were obtained under the following conditions.

- Indexed data: 60,000 MNIST samples
- Evaluation data: 10,000 MNIST test samples
- Number of neighbors: k = 3
- Similarity used at index construction: 0.95
- Unknown-detection threshold: shown in each table
- Refine: none
- Execution environment: Ryzen 9 5900HX / Windows 11 / .NET 8

## 1. Re-kNN Performance

| Unknown-Detection Threshold | Accuracy (Total Acc) | Unknown Rate | Misclassification Rate | Accuracy excluding Unknown | Correct | Unknown | Misclassified | Notes |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **0.5** | 0.9599 | 0.0105 | 0.0296 | 0.9701 | 9599 | 105 | 296 | Intended for practical use cases |
| **0.7** | 0.9312 | 0.0554 | 0.0134 | 0.9858 | 9312 | 554 | 134 | Reliability-oriented setting |
| **0.9** | 0.3814 | 0.6184 | **0.0002** | **0.9995** | 3814 | 6184 | 2 | **High-reliability setting** |

> * Accuracy (Total Acc) = Correct / Total number of test samples (10,000)  
> * Accuracy excluding Unknown = Correct / (Correct + Misclassified)
> * Unknown Rate = Unknown / Total number of test samples (10,000) 
> * Misclassification Rate = Misclassified / Total number of test samples (10,000) 

![Re-kNN_MNIST_Performance_vs_Detection_Threshold_Similarity=0.95_Refine=none](Re-kNN_MNIST_Performance_vs_Detection_Threshold_Similarity=0.95_Refine=none.png)

As the unknown-detection threshold increases, the misclassification rate decreases, while the unknown rate increases.

### Confusion Matrices

The confusion matrices are shown below.

#### Unknown-Detection Threshold: 0.5

| Test Label | Pred:0 | Pred:1 | Pred:2 | Pred:3 | Pred:4 | Pred:5 | Pred:6 | Pred:7 | Pred:8 | Pred:9 | Pred:Unknown |
|:--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|
| 0 | 969 | 1 | 0 | 0 | 0 | 0 | 2 | 1 | 2 | 0 | 5 |
| 1 | 0 | 1124 | 2 | 2 | 0 | 0 | 1 | 0 | 0 | 0 | 6 |
| 2 | 11 | 2 | 986 | 4 | 0 | 0 | 0 | 11 | 5 | 0 | 13 |
| 3 | 2 | 1 | 3 | 963 | 0 | 11 | 0 | 5 | 13 | 5 | 7 |
| 4 | 2 | 5 | 0 | 0 | 931 | 0 | 8 | 0 | 1 | 24 | 11 |
| 5 | 4 | 0 | 0 | 6 | 1 | 838 | 7 | 2 | 8 | 6 | 20 |
| 6 | 10 | 3 | 0 | 0 | 5 | 5 | 928 | 0 | 1 | 0 | 6 |
| 7 | 0 | 13 | 5 | 0 | 3 | 0 | 0 | 980 | 0 | 16 | 11 |
| 8 | 6 | 1 | 1 | 11 | 2 | 3 | 2 | 5 | 920 | 5 | 18 |
| 9 | 6 | 6 | 2 | 9 | 7 | 2 | 1 | 5 | 3 | 960 | 8 |

#### Unknown-Detection Threshold: 0.7

| Test Label | Pred:0 | Pred:1 | Pred:2 | Pred:3 | Pred:4 | Pred:5 | Pred:6 | Pred:7 | Pred:8 | Pred:9 | Pred:Unknown |
|:--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|
| 0 | 966 | 1 | 0 | 0 | 0 | 0 | 0 | 1 | 2 | 0 | 10 |
| 1 | 0 | 1123 | 2 | 1 | 0 | 0 | 1 | 0 | 0 | 0 | 8 |
| 2 | 8 | 1 | 956 | 2 | 0 | 0 | 0 | 8 | 2 | 0 | 55 |
| 3 | 1 | 0 | 1 | 923 | 0 | 2 | 0 | 4 | 2 | 3 | 74 |
| 4 | 1 | 1 | 0 | 0 | 883 | 0 | 6 | 0 | 0 | 6 | 85 |
| 5 | 2 | 0 | 0 | 2 | 1 | 785 | 2 | 0 | 2 | 2 | 96 |
| 6 | 3 | 2 | 0 | 0 | 5 | 0 | 916 | 0 | 1 | 0 | 31 |
| 7 | 0 | 8 | 4 | 0 | 2 | 0 | 0 | 955 | 0 | 9 | 50 |
| 8 | 4 | 0 | 0 | 2 | 2 | 1 | 1 | 4 | 877 | 2 | 81 |
| 9 | 3 | 5 | 0 | 1 | 1 | 0 | 1 | 4 | 2 | 928 | 64 |

#### Unknown-Detection Threshold: 0.9

| Test Label | Pred:0 | Pred:1 | Pred:2 | Pred:3 | Pred:4 | Pred:5 | Pred:6 | Pred:7 | Pred:8 | Pred:9 | Pred:Unknown |
|:--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|--:|
| 0 | 623 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 357 |
| 1 | 0 | 1079 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 56 |
| 2 | 0 | 0 | 93 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 939 |
| 3 | 0 | 0 | 0 | 153 | 0 | 0 | 0 | 0 | 0 | 0 | 857 |
| 4 | 0 | 0 | 0 | 0 | 180 | 0 | 0 | 0 | 0 | 1 | 801 |
| 5 | 0 | 0 | 0 | 0 | 0 | 49 | 0 | 0 | 0 | 0 | 843 |
| 6 | 0 | 0 | 0 | 0 | 0 | 0 | 451 | 0 | 0 | 0 | 507 |
| 7 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 546 | 0 | 0 | 482 |
| 8 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 143 | 1 | 830 |
| 9 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 497 | 512 |

## 2. Cases Misclassified by Re-kNN

Below are analyses of misclassified cases observed when the unknown-detection threshold was set to 0.7.

Total number of misclassified cases: 134

### Example 1: A sample with true label 9 misclassified as label 8

#### Test Label
![miss-241-testLabel](miss-241-testLabel.png)

#### Neighbor Data Used as Evidence
![miss-241-voteLabel](miss-241-voteLabel.png)

### Example 2: A sample with true label 4 misclassified as label 6

#### Test Label
![miss-247-testLabel](miss-247-testLabel.png)

#### Neighbor Data Used as Evidence
![miss-247-voteLabel](miss-247-voteLabel.png)

### Example 3: A sample with true label 2 misclassified as label 7

#### Test Label
![miss-321-testLabel](miss-321-testLabel.png)

#### Neighbor Data Used as Evidence
![miss-321-voteLabel](miss-321-voteLabel.png)

## 3. Cases Classified as Unknown by Re-kNN

Below are analyses of cases classified as Unknown when the unknown-detection threshold was set to 0.7.

Total number of Unknown cases: 554

### Example 1: A sample with true label 5 classified as Unknown

#### Test Label
![unknown-8-testLabel](unknown-8-testLabel.png)

#### Neighbor Data Used as Evidence
![unknown-8-voteLabel](unknown-8-voteLabel.png)

### Example 2: A sample with true label 4 classified as Unknown

#### Test Label
![unknown-33-testLabel](unknown-33-testLabel.png)

#### Neighbor Data Used as Evidence
![unknown-33-voteLabel](unknown-33-voteLabel.png)

### Example 3: A sample with true label 2 classified as Unknown

#### Test Label
![unknown-43-testLabel](unknown-43-testLabel.png)

#### Neighbor Data Used as Evidence
![unknown-43-voteLabel](unknown-43-voteLabel.png)

## 4. Performance Improvement by Refine

Refine is a process that relocates data placed in inappropriate positions to more appropriate positions.

Refine can be seen to have the effect of reassigning some data that would otherwise be classified as Unknown into the known category when a high decision threshold is used.
At a decision threshold of 0.9, many data points are filtered as Unknown; however, after applying Refine, the net change is a reduction of 36 Unknown cases, corresponding to an increase of 35 correct predictions and 1 additional misclassification.
On the other hand, the accuracy among samples judged as known decreases slightly, from 99.95% to 99.92%. Further verification is needed, but Refine may have the effect of reallocating some Unknown data to the known category under a high unknown-detection threshold.

| Refine | Unknown-Detection Threshold | Accuracy (Total Acc) | Unknown Rate | Misclassification Rate | Accuracy excluding Unknown | Correct | Unknown | Misclassified | Notes |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| none | 0.5 | 0.9599 | 0.0105 | 0.0296 | 0.9700859019706922 | 9599 | 105 | 296 | - |
| none | 0.7 | 0.9312 | 0.0554 | 0.0134 | 0.9858141012068601 | 9312 | 554 | 134 | - |
| none | 0.9 | 0.3814 | 0.6184 | 0.0002 | 0.999475890985325 | 3814 | 6184 | 2 | - |
| 0.01 | 0.5 | 0.9593 | 0.0105 | 0.0302 | 0.9694795351187468 | 9593 | 105 | 302 | - |
| 0.01 | 0.7 | 0.9325 | 0.0535 | 0.014 | 0.9852086634970946 | 9325 | 535 | 140 | - |
| 0.01 | 0.9 | 0.3849 | 0.6148 | 0.0003 | 0.9992211838006231 | 3849 | 6148 | 3 | - |

![Re-kNN_MNIST_Performance_Pre-Refine_vs_Post-Refine_(Similarity=0.95)](Re-kNN_MNIST_Performance_Pre-Refine_vs_Post-Refine_(Similarity=0.95).png)

* **Note**: `Refine = 0.01` indicates the termination criterion for the Refine process. The process stops when the proportion of data subject to Refine falls below 1% of the total.

## 5. Inference Time

The processing time for running inference on all MNIST test samples (10,000 samples) is shown below.

* **Measurement Environment**: Ryzen 9 5900HX (3.3GHz) / Windows 11 / .NET 8
* **Target Data**: MNIST (784 dimensions)
* **Total Processing Time (10,000 samples)**: $4,350.51 ms$
* **Average Latency per Sample**: **$435\ \mu s$**

## 6. Unknown Detection

The following results were obtained by evaluating MNIST while excluding each label from index registration, one at a time.
These results were measured with the unknown-detection threshold set to 0.7.

### Evaluation Results Including Unknown Predictions

| Unknown Label | Accuracy (Total Acc) | Unknown Rate | Misclassification Rate | Correct | Unknown | Misclassified | Notes |
|:---:|:---|:---|:---|---:|---:|---:|:---|
| 0 | 0.8388 | 0.1178 | 0.0434 | 8388 | 1178 | 434 |  |
| 1 | 0.8218 | 0.1290 | 0.0492 | 8218 | 1290 | 492 |  |
| 2 | 0.8366 | 0.1131 | 0.0503 | 8366 | 1131 | 503 |  |
| 3 | 0.8454 | 0.0901 | 0.0645 | 8454 | 901 | 645 |  |
| 4 | 0.8456 | 0.0587 | 0.0957 | 8456 | 587 | 957 |  |
| 5 | 0.8569 | 0.0875 | 0.0556 | 8569 | 875 | 556 |  |
| 6 | 0.8419 | 0.1034 | 0.0547 | 8419 | 1034 | 547 |  |
| 7 | 0.8374 | 0.0808 | 0.0818 | 8374 | 808 | 818 |  |
| 8 | 0.8500 | 0.0935 | 0.0565 | 8500 | 935 | 565 |  |
| 9 | 0.8489 | 0.0719 | 0.0792 | 8489 | 719 | 792 |  |
| none | 0.9312 | 0.0554 | 0.0134 | 9312 | 554 | 134 |  |

![TotalScore_includeexcludedlabel(Accuracy)](TotalScore_includeexcludedlabel(Accuracy).png)

![TotalScore_includeexcludedlabel(unknown,miss)](TotalScore_includeexcludedlabel(unknown,miss).png)

### Evaluation Results Excluding Unknown Predictions

| Unknown Label | Accuracy (Total Acc) | Accuracy excluding Unknown | Correct | Misclassified | Unknown | Notes |
|:---:|:---|:---|---:|---:|---:|:---|
| 0 | 0.9299 | 0.9865 | 8388 | 115 | 517 |  |
| 1 | 0.9270 | 0.9871 | 8218 | 107 | 540 |  |
| 2 | 0.9329 | 0.9857 | 8366 | 121 | 481 |  |
| 3 | 0.9404 | 0.9874 | 8454 | 108 | 428 |  |
| 4 | 0.9377 | 0.9860 | 8456 | 120 | 442 |  |
| 5 | 0.9408 | 0.9866 | 8569 | 116 | 423 |  |
| 6 | 0.9311 | 0.9870 | 8419 | 111 | 512 |  |
| 7 | 0.9333 | 0.9883 | 8374 | 99 | 499 |  |
| 8 | 0.9417 | 0.9884 | 8500 | 100 | 426 |  |
| 9 | 0.9442 | 0.9897 | 8489 | 88 | 414 |  |
| none | 0.9312 | 0.9858 | 9312 | 134 | 554 |  |

![TotalScore_withoutexcludedlabel(Accuracy)](TotalScore_withoutexcludedlabel(Accuracy).png)

![TotalScore_withoutexcludedlabel(Unknown,miss)](TotalScore_withoutexcludedlabel(Unknown,miss).png)

### Evaluation Results for Unknown Labels

For labels that were not registered in the index (= unknown labels), the result must be either **Unknown** or **misclassified**.

| Unknown Label | Unknown Rate | Misclassification Rate | Unknown | Misclassified | Notes |
|:---:|:---|:---|---:|---:|:---|
| 0 | 0.6745 | 0.3255 | 661 | 319 |  |
| 1 | 0.6608 | 0.3392  | 750 | 385 |  |
| 2 | 0.6298 | 0.3702 | 650 | 382 |  |
| 3 | 0.4683 | 0.5317 | 473 | 537 |  |
| 4 | 0.1477 | 0.8523 | 145 | 837 |  |
| 5 | 0.5067 | 0.4933 | 452 | 440 |  |
| 6 | 0.5449 | 0.4551 | 522 | 436 |  |
| 7 | 0.3006 | 0.6994 | 309 | 719 |  |
| 8 | 0.5226 | 0.4774 | 509 | 465 |  |
| 9 | 0.3023 | 0.6977 | 305 | 704 |  |

![UnknownLabelScore(byRate)](UnknownLabelScore(byRate).png)

### Discussion

In MNIST, labels with similar shapes (4, 7, and 9) have lower Unknown rates.  
This is likely caused by shape similarity.  
Because Re-kNN uses nearest-neighbor search, visually similar samples tend to be detected with relatively high similarity in these cases.  
In practical use, the decision threshold should be adjusted according to the use case to control sensitivity.  
This tendency is particularly notable for label 4. It likely reflects the fact that visually similar shapes tend to produce higher similarity scores.

## 7. Memory Consumption

The memory consumption after loading all MNIST data is shown below.

* **Measurement Environment**: Ryzen 9 5900HX (3.3GHz) / Windows 11 / .NET 8
* **Measurement Condition**: Immediately after loading all index information
* **Other Running Processes**: Other Windows processes were active during measurement.
* **Target Data**: MNIST (784 dimensions)
* **Memory Consumption**: 473.0 MB (measured from the Memory column in the Task Manager process tab)

---
*Created by Re-kNN Evaluation Suite (2026)*

