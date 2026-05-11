# Re-kNN API Specification

This document describes how to call the Re-kNN library.

## 1. Requirements

This library is provided as an x64 DLL for Windows and as an `.so` file for Linux/Ubuntu.

**Note**: Although this library is designed to be thread-safe, it has not yet been sufficiently tested. When using it concurrently from multiple threads, please be aware that issues are likely to occur.

## 2. Basic Structure

Re-kNN is designed to deploy multiple independent indexes in memory. Therefore, the number of instances used by the system must be specified during initialization.

Search results are designed to return IDs such as the document ID to which a vector belongs. Therefore, when registering vectors, IDs (`Main` and `Sub`) must be specified.
`Sub` is assumed to be subordinate to `Main`.

For MNIST and similar datasets, the label number is registered as `Main`, and the data number is registered as `Sub`.
For BERT and similar datasets, the document number is registered as `Main`, and the sentence number within the document is registered as `Sub`.

## 3. DLL / .so I/F Definitions

To call the native library, the following definitions must be inserted on the caller side. This sample is written in C#.
The code containing these definitions is `RekNNUtility.cs` in the repository. Refer to it as needed.

Struct and enum definitions:

```C#
    public enum ModeEnum
    {
        BERT = 1,

        MNIST = 2,

        CIFAR10 = 3,
    }

    public enum StatusDetailEnum
    {
        Success = 0,

        ErrorBadInstanceNo = -1,

        ErrorUtf8TextIsNull = -2,

        ErrorBadTextLength = -3,

        ErrorNotInitialized = -4,

        ErrorFileNotFound = -5,

        ErrorIOException = -6,

        ErrorOtherException = -99,
    }

    public enum DebugModeEnum
    {
        None = 0,
        Console = 1,
        Debug = 2,
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

DLL / .so call definitions:

```C#
        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern bool Initialize(ModeEnum mode, int instanceNum);

        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe bool Load(int instanceNo, byte* utf8Text, int textLength);

        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe bool Add(int instanceNo, float* vec, int length, int mainId, int subId, int searchMax, double threshold);

        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe bool Refine(int instanceNo, float* vec, int length, int mainId, int subId, int searchMax, double threshold);

        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern int Delete(int instanceNo, int mainId, int subId);

        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe SearchResult* Search(int instanceNo, float* vec, int length, int kValue);

        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe double GetTotalVector(int instanceNo);

        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe bool Save(int instanceNo, byte* utf8Text, int textLength);

        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe bool SaveWithCount(int instanceNo, byte* utf8Text, int textLength, int count);

        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe SearchResult* SimpleClustering(int instanceNo);

        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe PredictResult* Predict(int instanceNo, float* vec, int length, int kValue, double detectThreshold);

        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe bool IsNeedRefine(int instanceNo, float* vec, int length, int searchMax, int mainId, int subId);

        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe StatusDetailEnum GetStatusDetail();

        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern bool RefineAll(int instanceNo, int limit);

        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern void SetDebugMode(int mode);
```

## 4. Function List

| Function                              | Description                                                      |
| :------------------------------------ | :--------------------------------------------------------------- |
| [Initialize](#Initialize)             | Initializes Re-kNN                                               |
| [Load](#Load)                         | Loads a saved DB                                                 |
| [Add](#Add)                           | Adds vector information                                          |
| [Refine](#Refine)                     | Optimizes the index structure                                    |
| [Delete](#Delete)                     | Deletes vector information                                       |
| [Search](#Search)                     | Searches vector information                                      |
| [GetTotalVector](#GetTotalVector)     | Gets the number of registered vectors                            |
| [Save](#Save)                         | Saves the DB to storage                                          |
| [SaveWithCount](#SaveWithCount)       | Saves the DB with count information (for debugging / deprecated) |
| [SimpleClustering](#SimpleClustering) | Gets cluster information                                         |
| [Predict](#Predict)                   | Performs inference using a specified vector                      |
| [IsNeedRefine](#IsNeedRefine)         | Checks whether a vector group is subject to Refine               |
| [GetStatusDetail](#GetStatusDetail)   | Gets detailed execution result information                       |
| [RefineAll](#RefineAll)               | Executes Refine in batch                                         |
| [SetDebugMode](#SetDebugMode)         | Sets the debug mode                                              |

---

### Initialize

Allocates independent index areas for the specified number of instances.
In the evaluation version, one of BERT (768 dimensions), MNIST (784 dimensions), or CIFAR10 (3072 dimensions) can be selected.

```C#
        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern bool Initialize(ModeEnum mode, int instanceNum);
```

| Argument Type | Argument Name | Description                                    |
| :------------ | :------------ | :--------------------------------------------- |
| ModeEnum      | mode          | Initialization mode: BERT, MNIST, or CIFAR10   |
| int           | instanceNum   | Number of index areas / DB instances to create |

| Return Value | Description              |
| :----------- | :----------------------- |
| true         | Initialization succeeded |
| false        | Initialization failed    |

---

### Load

Loads index information from the specified storage into the specified instance.

```C#
        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe bool Load(int instanceNo, byte* utf8Text, int textLength);
```

| Argument Type | Argument Name | Description                                   |
| :------------ | :------------ | :-------------------------------------------- |
| int           | instanceNo    | Instance number                               |
| byte*         | utf8Text      | Path name of the folder where the DB is saved |
| int           | textLength    | String length of `utf8Text`                   |

| Return Value | Description    |
| :----------- | :------------- |
| true         | Load succeeded |
| false        | Load failed    |

---

### Add

Adds vector information to the specified instance.

```C#
        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe bool Add(int instanceNo, float* vec, int length, int mainId, int subId, int searchMax, double threshold);
```

| Argument Type | Argument Name | Description                                                                                                      |
| :------------ | :------------ | :--------------------------------------------------------------------------------------------------------------- |
| int           | instanceNo    | Instance number                                                                                                  |
| float*        | vec           | Vectors to register. The number of vectors is `length`, and each vector has the size specified at initialization |
| int           | length        | Number of vectors to register                                                                                    |
| int           | mainId        | Main ID of the vectors to register                                                                               |
| int           | subId         | Sub ID of the vectors to register                                                                                |
| int           | searchMax     | Search width used when searching for the registration destination. `5` is recommended                            |
| double        | threshold     | Threshold for index match judgment                                                                               |

| Return Value | Description            |
| :----------- | :--------------------- |
| true         | Registration succeeded |
| false        | Registration failed    |

---

### Refine

Optimizes the index structure of the specified vector information in the specified instance.

```C#
        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe bool Refine(int instanceNo, float* vec, int length, int mainId, int subId, int searchMax, double threshold);
```

| Argument Type | Argument Name | Description                                                                                                      |
| :------------ | :------------ | :--------------------------------------------------------------------------------------------------------------- |
| int           | instanceNo    | Instance number                                                                                                  |
| float*        | vec           | Vectors to optimize. The number of vectors is `length`, and each vector has the size specified at initialization |
| int           | length        | Number of vectors to optimize                                                                                    |
| int           | mainId        | Main ID of the vectors to optimize                                                                               |
| int           | subId         | Sub ID of the vectors to optimize                                                                                |
| int           | searchMax     | Search width used when searching for the registration destination. `5` is recommended                            |
| double        | threshold     | Threshold for index match judgment                                                                               |

| Return Value | Description                                                                        |
| :----------- | :--------------------------------------------------------------------------------- |
| true         | Optimization was executed                                                          |
| false        | Optimization was not executed, including cases where optimization is not necessary |

---

### Delete

Deletes the specified vector information from the specified instance.

```C#
        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern int Delete(int instanceNo, int mainId, int subId);
```

| Argument Type | Argument Name | Description       |
| :------------ | :------------ | :---------------- |
| int           | instanceNo    | Instance number   |
| int           | mainId        | Main ID to delete |
| int           | subId         | Sub ID to delete  |

| Return Value  | Description               |
| :------------ | :------------------------ |
| Numeric value | Number of deleted vectors |

---

### Search

Searches the specified instance using the specified vector information.
The number of returned data items may differ from the number specified by `kValue`.

```C#
        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe SearchResult* Search(int instanceNo, float* vec, int length, int kValue);
```

| Argument Type | Argument Name | Description                                                                                                         |
| :------------ | :------------ | :------------------------------------------------------------------------------------------------------------------ |
| int           | instanceNo    | Instance number                                                                                                     |
| float*        | vec           | Vectors to search with. The number of vectors is `length`, and each vector has the size specified at initialization |
| int           | length        | Number of vectors to search with                                                                                    |
| int           | kValue        | Search range                                                                                                        |

| Return Value | Description                                                                                                                                  |
| :----------- | :------------------------------------------------------------------------------------------------------------------------------------------- |
| not null     | Search succeeded. The details are stored in the structure and should be parsed by the caller. **The caller must free the returned pointer.** |
| null         | Search failed                                                                                                                                |

The following is an example of memory release processing.
Only the top-level pointer needs to be freed.

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

---

### GetTotalVector

Gets the number of vectors registered in the specified instance.

```C#
        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe double GetTotalVector(int instanceNo);
```

| Argument Type | Argument Name | Description     |
| :------------ | :------------ | :-------------- |
| int           | instanceNo    | Instance number |

| Return Value | Description                  |
| :----------- | :--------------------------- |
| -            | Number of registered vectors |

---

### Save

Writes the index information of the specified instance to the specified storage.

```C#
        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe bool Save(int instanceNo, byte* utf8Text, int textLength);
```

| Argument Type | Argument Name | Description                                        |
| :------------ | :------------ | :------------------------------------------------- |
| int           | instanceNo    | Instance number                                    |
| byte*         | utf8Text      | Path name of the folder where the DB will be saved |
| int           | textLength    | String length of `utf8Text`                        |

| Return Value | Description     |
| :----------- | :-------------- |
| true         | Write succeeded |
| false        | Write failed    |

---

### SaveWithCount

Writes the index information of the specified instance to the specified storage with count information.

**Note**: Use of this function is not recommended. Even when this function is used, the DB is not saved in a completely separated form.

```C#
        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe bool SaveWithCount(int instanceNo, byte* utf8Text, int textLength, int count);
```

| Argument Type | Argument Name | Description                                        |
| :------------ | :------------ | :------------------------------------------------- |
| int           | instanceNo    | Instance number                                    |
| byte*         | utf8Text      | Path name of the folder where the DB will be saved |
| int           | textLength    | String length of `utf8Text`                        |
| int           | count         | Count information used when generating file names  |

| Return Value | Description     |
| :----------- | :-------------- |
| true         | Write succeeded |
| false        | Write failed    |

---

### SimpleClustering

Gets cluster information from the specified instance at the time of the call.

```C#
        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe SearchResult* SimpleClustering(int instanceNo);
```

| Argument Type | Argument Name | Description     |
| :------------ | :------------ | :-------------- |
| int           | instanceNo    | Instance number |

| Return Value | Description                                                                                             |
| :----------- | :------------------------------------------------------------------------------------------------------ |
| not null     | Cluster information. Check the `SearchResult` structure. **The caller must free the returned pointer.** |
| null         | Failed to get cluster information                                                                       |

The following is an example of memory release processing.
Only the top-level pointer needs to be freed.

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

---

### Predict

Performs inference on which category the specified vector belongs to in the specified instance.
This inference engine also returns evidence information for the inference. This makes it possible to verify the validity of the judgment.

```C#
        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe PredictResult* Predict(int instanceNo, float* vec, int length, int kValue, double detectThreshold);
```

| Argument Type | Argument Name   | Description                                                                                                                                                                                                    |
| :------------ | :-------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| int           | instanceNo      | Instance number                                                                                                                                                                                                |
| float*        | vec             | Vectors to infer. The number of vectors is `length`, and each vector has the size specified at initialization                                                                                                  |
| int           | length          | Number of vectors to infer                                                                                                                                                                                     |
| int           | kValue          | Number of similar data items used for voting                                                                                                                                                                   |
| double        | detectThreshold | Threshold for accepting the voting result. The item that exceeds the threshold and has the highest evaluation value becomes the inference result. If no data exceeds the threshold, it is judged as “unknown.” |

| Return Value | Description                                                                                                                                                                                                                                                               |
| :----------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| not null     | Inference information. Check the `PredictResult` structure.<br>A `PredictResult` is also returned for an unknown judgment. Check `PredictedLabel` for details of the unknown judgment.<br>**The returned pointer must be freed by the caller using `NativeMemory.Free`.** |
| null         | Inference processing failed                                                                                                                                                                                                                                               |

The following is an example of memory release processing.
Only the top-level pointer needs to be freed.

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

---

### IsNeedRefine

Queries whether the specified vector is subject to optimization for the specified instance.

```C#
        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe bool IsNeedRefine(int instanceNo, float* vec, int length, int searchMax, int mainId, int subId);
```

| Argument Type | Argument Name | Description                                                                                                   |
| :------------ | :------------ | :------------------------------------------------------------------------------------------------------------ |
| int           | instanceNo    | Instance number                                                                                               |
| float*        | vec           | Vectors to check. The number of vectors is `length`, and each vector has the size specified at initialization |
| int           | length        | Number of vectors to check                                                                                    |
| int           | searchMax     | Search width used for the check. `5` is recommended                                                           |
| int           | mainId        | Main ID of the vectors to check                                                                               |
| int           | subId         | Sub ID of the vectors to check                                                                                |

| Return Value | Description                 |
| :----------- | :-------------------------- |
| true         | Subject to optimization     |
| false        | Not subject to optimization |

---

### GetStatusDetail

Gets detailed execution result information.

```C#
        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern unsafe StatusDetailEnum GetStatusDetail();
```

| Return Value              | Description                          |
| :------------------------ | :----------------------------------- |
| Success = 0               | Completed successfully               |
| ErrorBadInstanceNo = -1   | Invalid instance number              |
| ErrorUtf8TextIsNull = -2  | Null was specified as the UTF-8 text |
| ErrorBadTextLength = -3   | Invalid text length                  |
| ErrorNotInitialized = -4  | Initialization has not been executed |
| ErrorFileNotFound = -5    | File, such as the DB, does not exist |
| ErrorIOException = -6     | I/O error                            |
| ErrorOtherException = -99 | Other error                          |

---

### RefineAll

Executes Refine in batch.
Instead of specifying individual vectors, this function executes Refine on the entire DB.
The number of Refine executions is specified by the argument. If the count is `-1`, Refine is repeated until it is no longer necessary.

```C#
        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern bool RefineAll(int instanceNo, int limit);
```

| Argument Type | Argument Name | Description                                                                                                                          |
| :------------ | :------------ | :----------------------------------------------------------------------------------------------------------------------------------- |
| int           | instanceNo    | Instance number                                                                                                                      |
| int           | limit         | Upper limit of the number of Refine executions. If `-1`, Refine is repeated until it is no longer necessary.<br>`-1` is recommended. |

| Return Value | Description            |
| :----------- | :--------------------- |
| true         | Completed successfully |
| false        | Ended with an error    |

---

### SetDebugMode

Sets the debug mode.

```C#
        [DllImport("FuutaSystemSvcVectorLibrary")]
        private static extern void SetDebugMode(int mode);
```

| Argument Type | Argument Name | Description                                                                                              |
| :------------ | :------------ | :------------------------------------------------------------------------------------------------------- |
| int           | mode          | Debug mode<br>None = 0 : Do not output<br>Console = 1 : Output to Console<br>Debug = 2 : Output to Debug |
