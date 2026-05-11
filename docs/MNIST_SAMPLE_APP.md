# MNIST Sample Application

## Overview

- This is a sample application for handwritten digit recognition using the MNIST dataset.
- It is a Windows application implemented with WinUI.
- Please draw a digit, or any other shape or character, in the black area on the left side of the screen using a mouse or similar input device.
- When you press the `Predict` button, inference starts and the result is displayed on the right side of the screen.
- Since the source data used for judgment is MNIST, the inference result will be either a digit or `Unknown`.

## Features

In this sample application, if the input data cannot be sufficiently recognized as a digit, it returns `Unknown`.  
It also displays not only the inference result, but also the data used as evidence for the inference.

## Usage

### Preparing the MNIST Data

1. Download the MNIST data.

Example:  
https://www.kaggle.com/datasets/hojjatk/mnist-dataset

2. Modify the `settings.json` file.

Please modify the following file:

```text
RekNN_MNIST_Sample/settings.json
````

Example:

```json
{
  "TrainImageFilePath": "C:\\Users\\tokad\\Documents\\FuutaSystemService\\MNIST\\train-images.idx3-ubyte",
  "TrainLabelFilePath": "C:\\Users\\tokad\\Documents\\FuutaSystemService\\MNIST\\train-labels.idx1-ubyte",
  "TestImageFilePath": "C:\\Users\\tokad\\Documents\\FuutaSystemService\\MNIST\\t10k-images.idx3-ubyte",
  "TestLabelFilePath": "C:\\Users\\tokad\\Documents\\FuutaSystemService\\MNIST\\t10k-labels.idx1-ubyte",
  "DatabasePath": "C:\\Users\\tokad\\Documents\\FuutaSystemService\\MNIST-Sample\\database",
  "SimilarityThreshold": 0.9,
  "SearchMaxNumForAddVector": 5
}
```

`SimilarityThreshold` and `SearchMaxNumForAddVector` usually do not need to be changed.

## Screen Images

![SampleApp01](SampleApp01.png)

1. Prepare the MNIST data and press the `Study from MNIST` button to register it into the DB. After DB registration, the save process is executed automatically. The location of the MNIST data is defined in `settings.json`. After the DB has been registered, the saved DB is automatically loaded at startup, so this button is normally not used.
2. Immediately after startup, sample data is displayed in the handwriting area. Press the `Clear` button to reset the handwriting area.
3. Draw a digit, or any other shape or character, in the handwriting area using the mouse.
4. When you have finished drawing, press the `Predict` button. Inference will be executed.

![SampleApp02](SampleApp02.png)

5. The inference result is displayed here. In this example, the application predicts `8`. The score for the result is displayed on the right side. If this score does not satisfy the threshold, the result will be `Unknown`.
6. The inference evidence is displayed here. Evidence data is displayed according to the number of votes specified. Each item of evidence is assigned a score.

The following describes buttons and other UI elements not explained above:

* `Refine Database`: Executes Refine processing on the entire DB. After processing, the DB is saved automatically.
* `Load Database`: Loads the DB.
* `Save Database`: Saves the DB.
* TextBox at the bottom right: Displays the operation log.

## Options

In this application, the threshold and the number of votes can be changed using sliders.

### Threshold

This is the threshold used to determine whether to accept the inference result.
A value between 10 and 99 can be set.

If the `Score` exceeds `setting value × 0.01`, the inference result is accepted.
If this condition is not satisfied, the result is `Unknown`.

### Number of Votes

This is the number of data items used as references when making a judgment.
A value between 1 and 10 can be specified.

This corresponds to `k` in k-NN. However, since this application performs judgment based on scores, small values such as `k=2` can also be used.

## Notes

* Even when the number of votes is 1, the result will be `Unknown` if the score is low.
* Although it includes ideas similar to k-NN, it is not the same. Please consider it a different mechanism.
* This application is a sample for checking evidence-based inference. Incremental addition / deletion of data is not implemented in this application.
* `Refine` processing is also implemented, but its impact on the result is minor in this sample. This is because the MNIST dataset is relatively small.

