# Large-Scale Data Clustering in Re-kNN

Re-kNN performs dynamic clustering as part of its processing. As a by-product, it is able to output cluster information.

Cluster information can be obtained at any point when data registration or deletion is performed.
This is also a characteristic of the **dynamic clustering** carried out internally by Re-kNN.
In use cases where data is frequently added and deleted, this characteristic can be a significant advantage.

This document describes data related to cluster quality and discusses the results.

## 1. Characteristics of the Clustering

The characteristics of this clustering process are as follows.

- **Number of clusters**: Determined by the system based on the data distribution. In many cases, it is less than 1/20 of the total number of data points. Compared with k-means-based methods, the number of clusters tends to be larger.
- **Cluster quality**: Initial clustering results may contain noise. Quality is expected to improve by applying Refine.
- **Cluster update timing**: Continuous. Cluster information is always maintained whenever data is added or deleted.
- **Parallelization**: At present, no parallelization is implemented. Therefore, it does not assume an advanced parallel execution environment.

Because cluster information is obtained as a by-product, users cannot directly specify the number of clusters.

## 2. Clustering Results

Clustering 60,000 MNIST training samples produced 2,262 clusters.
The Similarity used at index construction was set to 0.9.
The characteristics of the generated clusters are shown below.

### Cluster Quality

Here, cluster quality is defined as the frequency of the most frequent label within a cluster divided by the total number of data points in that cluster.

$Cluster\ Quality = \frac{Frequency\ of\ the\ most\ frequent\ label\ in\ the\ cluster}{Total\ number\ of\ data\ points\ in\ the\ cluster}$

The histogram below shows cluster quality based on this definition.

![Re-kNN_MNIST_clustering_quality_histogram](Re-kNN_MNIST_clustering_quality_histogram.png)

| Cluster Quality (Start) | Cluster Quality (End) | Number of Clusters |
|---:|---:|---:|
| 0.00 | 0.05 |    0 |
| 0.05 | 0.10 |    0 |
| 0.10 | 0.15 |    0 |
| 0.15 | 0.20 |    0 |
| 0.20 | 0.25 |    0 |
| 0.25 | 0.30 |    3 |
| 0.30 | 0.35 |    3 |
| 0.35 | 0.40 |    7 |
| 0.40 | 0.45 |   10 |
| 0.45 | 0.50 |   12 |
| 0.50 | 0.55 |   52 |
| 0.55 | 0.60 |   35 |
| 0.60 | 0.65 |   15 |
| 0.65 | 0.70 |   50 |
| 0.70 | 0.75 |   31 |
| 0.75 | 0.80 |   58 |
| 0.80 | 0.85 |   85 |
| 0.85 | 0.90 |  122 |
| 0.90 | 0.95 |  239 |
| 0.95 | 1.00 | 1540 |

While high-quality clusters account for the majority, a certain number of low-quality clusters are also detected. Low-quality clusters tend to contain data that are difficult to distinguish.

### Cluster Size

The histogram below shows the sizes of the generated clusters.

![Re-kNN_MNIST_cluster_size_histogram](Re-kNN_MNIST_cluster_size_histogram.png)

| Cluster Size (Start) | Cluster Size (End) | Number of Clusters |
|---:|---:|---:|
| 1 | 3 | 233 |
| 4 | 6 | 104 |
| 7 | 9 |  73 |
| 10 | 12 |  91 |
| 13 | 15 |  92 |
| 16 | 18 | 101 |
| 19 | 21 | 115 |
| 22 | 24 | 138 |
| 25 | 27 | 147 |
| 28 | 30 | 156 |
| 31 | 33 | 158 |
| 34 | 36 | 147 |
| 37 | 39 | 177 |
| 40 | 42 | 154 |
| 43 | 45 | 147 |
| 46 | 48 | 161 |
| 49 | 51 |  62 |
| 52 | 54 |   3 |
| 55 | 57 |   1 |
| 58 | 60 |   2 |

Most cluster sizes are 50 or smaller. The proportion of clusters with size 51 or smaller is 99.7%.
This is reasonable given that dividing 60,000 data points by the number of clusters yields approximately 27 points per cluster.

### Examples of High-Quality Clusters

Here, a few examples with $Cluster\ Quality = 1.0$ are shown.
However, clusters containing fewer than 5 data points are excluded.

#### Case 1: Number of data points in the cluster = 12

![cluster-1.0-4](cluster-1.0-4.png)

#### Case 2: Number of data points in the cluster = 7

![cluster-1.0-9](cluster-1.0-9.png)

#### Case 3: Number of data points in the cluster = 6

![cluster-1.0-14](cluster-1.0-14.png)

### Examples of Clusters with Noise

Here, a few examples with $0.9 \le Cluster\ Quality < 1.0$ are shown.
However, clusters containing fewer than 5 data points are excluded.

#### Case 1: Number of data points in the cluster = 32

![cluster-0.9375-0](cluster-0.9375-0.png)

* Only 20 samples are shown in the image.

#### Case 2: Number of data points in the cluster = 45

![cluster-0.9111111111111112-12](cluster-0.9111111111111112-12.png)

* Only 20 samples are shown in the image.

#### Case 3: Number of data points in the cluster = 45

![cluster-0.9111111111111112-16](cluster-0.9111111111111112-16.png)

* Only 20 samples are shown in the image.

### Examples of Low-Quality Clusters

Here, a few examples with $Cluster\ Quality < 0.5$ are shown.
However, clusters containing fewer than 2 data points are excluded.

#### Case 1: Number of data points in the cluster = 31

![cluster-0.4516129032258064-28](cluster-0.4516129032258064-28.png)

* Only 20 samples are shown in the image.

#### Case 2: Number of data points in the cluster = 31

![cluster-0.2903225806451613-46](cluster-0.2903225806451613-46.png)

* Only 20 samples are shown in the image.

#### Case 3: Number of data points in the cluster = 13

![cluster-0.3846153846153846-66](cluster-0.3846153846153846-66.png)

## 3. Discussion

As far as the clustering results can be observed, no unnatural results have been obtained.
On the other hand, the current results reflect the state before applying Refine. It is expected that applying Refine will improve cluster quality.

Evaluation results for cluster quality after applying Refine will be added later.


