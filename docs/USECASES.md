# Re-kNN Use Cases

> **Related Documents:**
>
> - [Performance Evaluation Evidence with MNIST (EVIDENCE.MNIST.md)](EVIDENCE.MNIST.md)
> - [Performance Evaluation Evidence with CIFAR-10 (EVIDENCE.CIFAR-10.md)](EVIDENCE.CIFAR-10.md)

The following are example use cases in which Re-kNN's characteristics - low memory usage, low latency, flexibility, and unknown-detection capability - may be effectively utilized.

Applying Re-kNN to each of these use cases remains a subject for future work.

| Use Case | Performance Focus | Recommended Unknown-Detection Threshold | Characteristics |
| :--- | :--- | :--- | :--- |
| **Inspection Process Improvement** | Flexibility / Explainability | $\ge 0.9$ | **Small-start** system operation |
| **Edge Inference** | Low resource usage / Speed | $\ge 0.5$ | **Real-time decision-making** and **edge-only operation** on a standalone device |
| **Anomaly Detection** | Unknown-detection capability | $\ge 0.9$ | Anomaly detection centered on **normal data** |
| **Knowledge Search** | Explainability / Reproducibility | - | Presentation of search evidence |
| **Decision Support Systems** | Explainability / Reproducibility | $\ge 0.9$ or case-dependent | Evidence-based decision support |

* In knowledge search, search-condition design is prioritized over the unknown-detection threshold.

## 1. Improvement of Inspection Processes

A key challenge in inspection processes is that it is difficult to prepare a large amount of decision data in advance.
Re-kNN's flexibility and unknown-detection capability may provide an effective approach to this challenge.

- **Scenario**: A small-start inspection system
- **Key Point**: By repeatedly adding data detected as Unknown, performance can be **improved during operation**.
- **Application Policy**: Start operation with a small dataset. Add misdetected data and Unknown data generated during operation to the index as needed.
- **Parameter Setting**: Set the unknown-detection threshold high ($\ge 0.9$). This enables aggressive detection of unknown data.
- **Advantage 1**: Operation can begin even with a **small amount of data**.
- **Advantage 2**: **Downtime-free** operation through incremental data addition and deletion.
- **Advantage 3**: Provision of **evidence** for defect judgments, enabling defect analysis based on that evidence.
- **Disadvantage 1**: When data is insufficient, decision accuracy may become unstable. Many samples may be judged as **Unknown**.
- **Disadvantage 2**: Initial data registration cost. A large amount of data must be added at the early stage of operation.
- **Strength 1**: Detection of unknown data.
- **Strength 2**: Incremental data addition and deletion.

## 2. Inference on Edge Devices

Edge devices are required to perform inference under limited resources. In many cases, the data handled are low-dimensional.
Because such devices are often deployed in different environments, index adjustment on a per-device basis is also required.
Re-kNN's low memory usage and low latency may provide an effective approach to this challenge.

- **Scenario**: Inference on autonomous devices equipped with sensors
- **Key Point**: Index adjustment can be performed directly on the edge device.
- **Application Policy**: Incremental registration and inference using relatively small CPU and memory resources.
- **Parameter Setting**: Set the unknown-detection threshold for operational use ($\ge 0.5$). This reduces the rate of Unknown occurrences.
- **Advantage 1**: Because resource consumption is kept low, the unit cost of edge devices can be reduced.
- **Advantage 2**: Adjustments tailored to the operating environment can be completed on the edge device itself.
- **Disadvantage 1**: As the amount of registered data increases, resource consumption may rise in the future.
- **Strength**: Low-cost index maintenance.

## 3. Anomaly Detection

A major challenge in anomaly detection is that anomalous data are difficult to collect.
Re-kNN's unknown-detection capability may provide an effective approach to this challenge.

- **Scenario**: Systems that require detection of unknown states (for example, in the security domain)
- **Key Point**: Unknown data can be identified simply by registering known data.
- **Application Policy**: Add unknown anomalies to the index as anomalous data as they are discovered.
- **Parameter Setting**: Set the unknown-detection threshold for anomaly detection ($\ge 0.9$). This enables aggressive detection of unknown data.
- **Advantage 1**: By actively detecting unknown data, missed anomalies (false negatives) can be reduced.
- **Advantage 2**: Known anomalies can be detected by storing them with labels.
- **Disadvantage 1**: Not all unknown data can necessarily be detected, so combination with other detection methods may sometimes be required.
- **Strength**: Anomaly-detection performance may be improved through incremental registration rather than retraining.

## 4. Knowledge Search

A key challenge in knowledge search is that, even when results are returned, the reasoning behind them is often unclear.  
Re-kNN's explainability and deterministic search behavior may provide an effective approach to this challenge.

- **Scenario**: Internal knowledge search, document search, FAQ search
- **Key Point**: Similar sentences and neighboring data can be presented as evidence for each search result.
- **Application Policy**: Register documents and sentences as vectors, and return sentence-level scores together with nearest-neighbor information at search time.
- **Parameter Setting**: Adjust Similarity and the number of returned results according to the search use case. When reliability is important, prioritize evidence presentation.
- **Advantage 1**: Search results can be **verified**.
- **Advantage 2**: Users can more easily understand **why this result was returned**.
- **Advantage 3**: It becomes easier to analyze and improve incorrect or inappropriate results.
- **Disadvantage 1**: Because evidence must be presented, response design may become more complex than in simple full-text search.
- **Disadvantage 2**: Search quality may vary depending on the vectorization method and registration granularity.
- **Strength 1**: Fine-grained evidence through sentence-level scoring.
- **Strength 2**: Explainable search through nearest-neighbor evidence.
- **Strength 3**: Reproducibility through deterministic outputs.

## 5. Decision Support Systems

A key challenge in decision support systems is that AI outputs may be used as a basis for judgment even when their justification is unclear.  
Re-kNN's explainability and deterministic outputs may provide an effective approach to this challenge.

- **Scenario**: Business systems that support human judgment, review support systems, and support systems that require auditability
- **Key Point**: Neighboring data and scores can be presented as evidence for each output.
- **Application Policy**: Use the results not as automatically finalized decisions, but as supporting information for human final judgment.
- **Parameter Setting**: Use settings that prioritize reliability and evidence presentation. When necessary, set the unknown-detection threshold relatively high.
- **Advantage 1**: Makes **auditing** of decisions easier.
- **Advantage 2**: Makes it easier to **explain** incorrect or inappropriate outputs.
- **Advantage 3**: Fits well with workflows in which humans make the final decision.
- **Disadvantage 1**: Because evidence presentation is required, the system design may become more complex than that of a simple automated decision system.
- **Disadvantage 2**: In cases where human final judgment is assumed, the system may be less suitable for full automation.
- **Strength 1**: Evidence-based outputs.
- **Strength 2**: Reproducibility through deterministic outputs.
- **Strength 3**: Explainable decision support.


