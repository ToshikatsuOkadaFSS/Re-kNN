# Architecture of Re-kNN

## Core Philosophy

### Deterministic Geometry

Re-kNN performs search based on **Deterministic Geometry**, without relying on random numbers or random seeds.
Under identical input conditions, the system always yields identical results.

### Density-Based Partitioning

Spatial partitioning is performed based on the **natural density** of the data.

### No Data Truncation

**Clustering instead of quantization.**
Unlike traditional methods that truncate vector precision to save memory, Re-kNN employs high-fidelity clustering based on density. 
We preserve the original integrity of your data while organizing it with a structurally efficient approach.

### Sequential Order Acceptance

Re-kNN accepts that the tree structure may vary depending on the order of data insertion.
We embrace the "flow of time" as a structural reality.
Insertion order affects the resulting structure, but the system remains deterministic under a fixed insertion order.

### No Sort

Re-kNN does not rely on 1D sorting as the primary organizing principle for high-dimensional data.
Instead, it organizes data according to the structural properties of the space itself.

### Honest Predict

Re-kNN provides supporting information for each inference, such as matched neighbors, distances, and unknown or warning indicators.

### Honest Unknown

Explicitly notifies when the target data is unknown. We prioritize "intellectual honesty" over forcing a guess.

### Dynamic Knowledge Maintenance

Knowledge is not static. Re-kNN allows for real-time modifications—addition, deletion, and optimization—ensuring the database evolves alongside the information it stores.

### Post-Build Self-Correction

After database construction, Re-kNN can re-evaluate elements that were placed in structurally inappropriate positions in the search tree and correct them through refinement.

