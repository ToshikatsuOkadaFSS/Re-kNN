# Re-kNN API Specification

The Re-kNN library is provided in the form of a DLL. This document describes how to call the library.

## 1. Conditions

This library is provided as a DLL built for win-x64.

**Warning**: This library is intended to be thread-safe, but it has not yet been tested sufficiently. If it is used concurrently from multiple threads, there is a high possibility that defects may occur.

## 2. Basic Structure

Re-kNN is designed on the assumption that multiple independent indexes will be deployed in memory. Therefore, it is necessary to specify the number of instances to be used by the system at initialization time.

The search result is designed to return the IDs of the document or other entity to which the vector belongs. Therefore, when registering a vector, it is necessary to specify IDs (Main and Sub).
Sub is assumed to be subordinate to Main.
For MNIST and similar datasets, the label number is registered as Main and the data number as Sub.
For BERT and similar datasets, the document number is registered as Main and the sentence number within the document as Sub.

## 3. DLL I/F Definitions

To call the DLL, the following definitions must be added on the caller side. This sample is written in C#.
The code containing these definitions is `RekNNUtility.cs` in the repository. Refer to it as needed.

Definition of structs:

```C#
    /// <summary>
    /// Operation mode
    /// </summary>
    public enum ModeEnum
    {
        BERT = 1,

        MNIST = 2,

        CIFAR10 = 3,
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
````

Definition for calling the DLL:

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern bool Initialize(ModeEnum mode, int instanceNum);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool Load(int instanceNo, byte* utf8Text, int textLength);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool Add(int instanceNo, float* vec, int length, int mainId, int subId, int searchMax, double threshold);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool Refine(int instanceNo, float* vec, int length, int mainId, int subId, int searchMax, double threshold);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern int Delete(int instanceNo, int mainId, int subId);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe SearchResult* Search(int instanceNo, float* vec, int length, int kValue);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe double GetTotalVector(int instanceNo);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool Save(int instanceNo, byte* utf8Text, int textLength);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool SaveWithCount(int instanceNo, byte* utf8Text, int textLength, int count);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe SearchResult* SimpleClustering(int instanceNo);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe PredictResult* Predict(int instanceNo, float* vec, int length, int kValue, double detectThreshold);

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool IsNeedRefine(int instanceNo, float* vec, int length, int searchMax, int docId, int subDocId);
```

## 4. List of Functions

| Function Name                         | Feature                                                                   |
| :------------------------------------ | :------------------------------------------------------------------------ |
| [Initialize](#initialize)             | Initialize Re-kNN                                                         |
| [Load](#load)                         | Load a saved database                                                     |
| [Add](#add)                           | Add vector information                                                    |
| [Refine](#refine)                     | Optimize the index structure                                              |
| [Delete](#delete)                     | Delete vector information                                                 |
| [Search](#search)                     | Search vector information                                                 |
| [GetTotalVector](#gettotalvector)     | Get the number of registered vectors                                      |
| [Save](#save)                         | Save the database to storage                                              |
| [SaveWithCount](#savewithcount)       | Save the database with count information (for debugging, not recommended) |
| [SimpleClustering](#simpleclustering) | Get clustering information                                                |
| [Predict](#predict)                   | Run inference with the specified vector                                   |
| [IsNeedRefine](#isneedrefine)         | Check whether a vector group is subject to Refine                         |

---

### Initialize

Allocates the specified number of independent index regions.
In the trial version, choose one of BERT (768 dimensions), MNIST (784 dimensions), or CIFAR10 (3072 dimensions).

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern bool Initialize(ModeEnum mode, int instanceNum);
```

| Argument Type | Argument Name | Meaning                                              |
| :------------ | :------------ | :--------------------------------------------------- |
| ModeEnum      | mode          | Initialization mode (one of BERT, MNIST, or CIFAR10) |

| Return Value | Meaning                  |
| :----------- | :----------------------- |
| true         | Initialization succeeded |
| false        | Initialization failed    |

---

### Load

Loads index information from the specified storage into the specified instance.

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool Load(int instanceNo, byte* utf8Text, int textLength);
```

| Argument Type | Argument Name | Meaning                                         |
| :------------ | :------------ | :---------------------------------------------- |
| int           | instanceNo    | Instance number                                 |
| byte*         | utf8Text      | Path of the folder where the database was saved |
| int           | textLength    | String length of utf8Text                       |

| Return Value | Meaning        |
| :----------- | :------------- |
| true         | Load succeeded |
| false        | Load failed    |

---

### Add

Adds vector information to the specified instance.

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool Add(int instanceNo, float* vec, int length, int mainId, int subId, int searchMax, double threshold);
```

| Argument Type | Argument Name | Meaning                                                                              |
| :------------ | :------------ | :----------------------------------------------------------------------------------- |
| int           | instanceNo    | Instance number                                                                      |
| float*        | vec           | Vectors to register (length vectors, each with the size specified at initialization) |
| int           | length        | Number of vectors to register                                                        |
| int           | mainId        | Main ID of the vectors to register                                                   |
| int           | subId         | Sub ID of the vectors to register                                                    |
| int           | searchMax     | Search width used when finding the registration destination (5 recommended)          |
| double        | threshold     | Index match threshold                                                                |

| Return Value | Meaning                |
| :----------- | :--------------------- |
| true         | Registration succeeded |
| false        | Registration failed    |

---

### Refine

Optimizes the index structure of the specified vector information in the specified instance.

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool Refine(int instanceNo, float* vec, int length, int mainId, int subId, int searchMax, double threshold);
```

| Argument Type | Argument Name | Meaning                                                                              |
| :------------ | :------------ | :----------------------------------------------------------------------------------- |
| int           | instanceNo    | Instance number                                                                      |
| float*        | vec           | Vectors to register (length vectors, each with the size specified at initialization) |
| int           | length        | Number of vectors to register                                                        |
| int           | mainId        | Main ID of the vectors to register                                                   |
| int           | subId         | Sub ID of the vectors to register                                                    |
| int           | searchMax     | Search width used when finding the registration destination (5 recommended)          |
| int           | threshold     | Index match threshold                                                                |

| Return Value | Meaning                                                                             |
| :----------- | :---------------------------------------------------------------------------------- |
| true         | Optimization was performed                                                          |
| false        | Optimization was not performed (including cases where optimization was unnecessary) |

---

### Delete

Deletes the specified vector information from the specified instance.

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern int Delete(int instanceNo, int mainId, int subId);
```

| Argument Type | Argument Name | Meaning                            |
| :------------ | :------------ | :--------------------------------- |
| int           | instanceNo    | Instance number                    |
| int           | mainId        | Main ID of the vectors to register |
| int           | subId         | Sub ID of the vectors to register  |

| Return Value | Meaning |
| :--- | :--- |
| Numeric value | Number of deleted vectors |

---

### Search

Searches the specified instance using the specified vector information.
The amount of data returned may differ from the kValue specified in the argument.

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe SearchResult* Search(int instanceNo, float* vec, int length, int kValue);
```

| Argument Type | Argument Name | Meaning                                                                              |
| :------------ | :------------ | :----------------------------------------------------------------------------------- |
| int           | instanceNo    | Instance number                                                                      |
| float*        | vec           | Vectors to register (length vectors, each with the size specified at initialization) |
| int           | length        | Number of vectors to register                                                        |
| int           | kValue        | Search range                                                                         |

| Return Value | Meaning                                                                            |
| :----------- | :--------------------------------------------------------------------------------- |
| ! null       | Search succeeded. Parse the structure for details. **Must be freed by the caller** |
| null         | Search failed                                                                      |

An example of memory release is shown below. It is sufficient to free only the head pointer.

```C#
SearchResult* result = null;

try
{
    // your codes here

    result = Search(0, vector, len, k)

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

Gets the number of registered vectors in the specified instance.

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe double GetTotalVector(int instanceNo);
```

| Argument Type | Argument Name | Meaning         |
| :------------ | :------------ | :-------------- |
| int           | instanceNo    | Instance number |

| Return Value | Meaning                      |
| :----------- | :--------------------------- |
| -            | Number of registered vectors |

---

### Save

Writes the index information of the specified instance to the specified storage.

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool Save(int instanceNo, byte* utf8Text, int textLength);
```

| Argument Type | Argument Name | Meaning                                             |
| :------------ | :------------ | :-------------------------------------------------- |
| int           | instanceNo    | Instance number                                     |
| byte*         | utf8Text      | Path of the folder where the database will be saved |
| int           | textLength    | String length of utf8Text                           |

| Return Value | Meaning         |
| :----------- | :-------------- |
| true         | Write succeeded |
| false        | Write failed    |

---

### SaveWithCount

Writes the index information of the specified instance to the specified storage together with count information.

**Warning**: Use of this function is not recommended. Even when using this function, the database is not saved in a completely separated form.

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool SaveWithCount(int instanceNo, byte* utf8Text, int textLength, int count);
```

| Argument Type | Argument Name | Meaning                                             |
| :------------ | :------------ | :-------------------------------------------------- |
| int           | instanceNo    | Instance number                                     |
| byte*         | utf8Text      | Path of the folder where the database will be saved |
| int           | textLength    | String length of utf8Text                           |
| int           | count         | Count information (used when generating file names) |

| Return Value | Meaning         |
| :----------- | :-------------- |
| true         | Write succeeded |
| false        | Write failed    |

---

### SimpleClustering

Gets clustering information from the specified instance at the time of the call.

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe SearchResult* SimpleClustering(int instanceNo);
```

| Argument Type | Argument Name | Meaning         |
| :------------ | :------------ | :-------------- |
| int           | instanceNo    | Instance number |

| Return Value | Meaning                                                                                |
| :----------- | :------------------------------------------------------------------------------------- |
| ! null       | Cluster information (check the SearchResult structure) **Must be freed by the caller** |
| null         | Failed to obtain cluster information                                                   |

An example of memory release is shown below. It is sufficient to free only the head pointer.

```C#
SearchResult* result = null;

try
{
    // your codes here

    result = SimpleClustering(0)

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

Infers where the specified vector belongs in the specified instance.
This inference engine also returns the evidence for the inference, making it possible to verify the validity of the decision.

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe PredictResult* Predict(int instanceNo, float* vec, int length, int kValue, double detectThreshold);
```

| Argument Type | Argument Name   | Meaning                                                                                                                                                                                                           |
| :------------ | :-------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| int           | instanceNo      | Instance number                                                                                                                                                                                                   |
| float*        | vec             | Vectors to register (length vectors, each with the size specified at initialization)                                                                                                                              |
| int           | length          | Number of vectors to register                                                                                                                                                                                     |
| int           | kValue          | Number of similar data items used for voting                                                                                                                                                                      |
| double        | detectThreshold | Threshold for accepting the voting result. The item whose evaluation value is the highest and exceeds the threshold is used as the inference result. If no item exceeds the threshold, it is judged as "Unknown". |

| Return Value | Meaning                                                                                   |
| :----------- | :---------------------------------------------------------------------------------------- |
| ! null       | Inference information (check the PredictResult structure) **Must be freed by the caller** |
| null         | Inference failed (including Unknown judgment)                                             |

An example of memory release is shown below. It is sufficient to free only the head pointer.

```C#
PredictResult* result = null;

try
{
    // your codes here

    result = Predict(0, vector, len, k, th)

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

Queries whether the specified vector is subject to optimization in the specified instance.

```C#
        [DllImport("DLL\\FuutaSystemSvcVectorLibrary.dll")]
        private static extern unsafe bool IsNeedRefine(int instanceNo, float* vec, int length, int searchMax, int mainId, int subId);
```

| Argument Type | Argument Name | Meaning                                                                              |
| :------------ | :------------ | :----------------------------------------------------------------------------------- |
| int           | instanceNo    | Instance number                                                                      |
| float*        | vec           | Vectors to register (length vectors, each with the size specified at initialization) |
| int           | length        | Number of vectors to register                                                        |
| int           | searchMax     | Search width used when finding the registration destination (5 recommended)          |
| int           | mainId        | Main ID of the vectors to register                                                   |
| int           | subId         | Sub ID of the vectors to register                                                    |

| Return Value | Meaning                           |
| :----------- | :-------------------------------- |
| true         | It is subject to optimization     |
| false        | It is not subject to optimization |
