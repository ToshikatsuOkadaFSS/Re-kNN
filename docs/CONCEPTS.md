# Concepts of Re-kNN

## Key Concept

Re-kNN is not just a fast vector search engine.

It is designed to make search results:
- Explainable (why this result?)
- Reproducible (same input → same output)
- Verifiable (can we trust this result?)

## Core Value

- Explicit evidence for each result
- Deterministic behavior
- Stable explanations (no randomness)
- Support for unknown data
- Suitable for evaluation in real-world decision-making contexts

## Explainability & Hallucination Verification

### Problem

In many knowledge retrieval systems, results can be returned,
but the reason behind those results is often unclear.

This becomes critical in environments where hallucination is a concern:

- Users cannot verify whether the result is reliable
- Incorrect results cannot be analyzed or improved
- The system cannot be trusted for decision-making or auditing
- When hallucination is suspected, verification is difficult

### Our Approach

Re-kNN provides explicit and reproducible evidence for each result:

- Sentence-level similarity scores
- Nearest neighbors as reasoning evidence
- Deterministic outputs (same input → same result)

### Impact

- Enables verification when hallucination is suspected
- Allows users to understand *why* a result is returned
- Makes error analysis and system improvement possible
- Supports decision-making and audit workflows



