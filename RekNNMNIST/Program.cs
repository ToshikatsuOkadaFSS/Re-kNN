// Copyright 2026 Fuuta System Service LLC.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using FuutaSystemSvcCommonLibrary;
using Microsoft.Win32.SafeHandles;
using RekNNMNIST;
using RekNNUtility;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

RekNNMNIST.CommonValues.LoadSettings();

foreach (string path in new string[] {
    RekNNMNIST.CommonValues.Settings.TrainImageFilePath,
    RekNNMNIST.CommonValues.Settings.TrainLabelFilePath,
    RekNNMNIST.CommonValues.Settings.TestImageFilePath,
    RekNNMNIST.CommonValues.Settings.TestLabelFilePath,
})
{
    if (File.Exists(path) == false)
    {
        Console.WriteLine($"{path} file not found");
        return;
    }
}

int imageNum = 60000;
ImageHeader imgHeader = new ImageHeader();
ImageData[] imgData = new ImageData[imageNum];
LabelHeader labelHeader = new LabelHeader();
byte[] labelData = new byte[imageNum];

unsafe
{
    byte* _data = null;
    long totalBytes = sizeof(ImageHeader) + sizeof(ImageData) * imageNum;
    using SafeFileHandle fileHandle =
        File.OpenHandle(RekNNMNIST.CommonValues.Settings.TrainImageFilePath, FileMode.Open, FileAccess.Read);
    _data = (byte*)NativeMemory.Alloc((nuint)totalBytes);
    Span<byte> dest = new Span<byte>(_data, (int)totalBytes);
    RandomAccess.Read(fileHandle, dest, 0);
    
    ImageHeader* header = (ImageHeader*)_data;
    ImageData* images = (ImageData*)(_data + sizeof(ImageHeader));

    imgHeader = *header;

    for (int i = 0; i < imageNum; i++)
    {
        imgData[i] = images[i];
    }

    NativeMemory.Free(_data);
}

unsafe
{
    byte* _data = null;
    long totalBytes = sizeof(LabelHeader) + sizeof(byte) * imageNum;
    using SafeFileHandle fileHandle =
        File.OpenHandle(RekNNMNIST.CommonValues.Settings.TrainLabelFilePath, FileMode.Open, FileAccess.Read);
    _data = (byte*)NativeMemory.Alloc((nuint)totalBytes);
    Span<byte> dest = new Span<byte>(_data, (int)totalBytes);
    RandomAccess.Read(fileHandle, dest, 0);

    LabelHeader* header = (LabelHeader*)_data;
    byte* labels = _data + sizeof(LabelHeader);

    labelHeader = *header;

    for (int i = 0; i < imageNum; i++)
    {
        labelData[i] = labels[i];
    }

    NativeMemory.Free(_data);
}

int testNum = 10000;
ImageHeader testImgHeader = new ImageHeader();
ImageData[] testImgData = new ImageData[testNum];
LabelHeader testLabelHeader = new LabelHeader();
byte[] testLabelData = new byte[testNum];

unsafe
{
    byte* _data = null;
    long totalBytes = sizeof(ImageHeader) + sizeof(ImageData) * testNum;
    using SafeFileHandle fileHandle =
        File.OpenHandle(RekNNMNIST.CommonValues.Settings.TestImageFilePath, FileMode.Open, FileAccess.Read);
    _data = (byte*)NativeMemory.Alloc((nuint)totalBytes);
    Span<byte> dest = new Span<byte>(_data, (int)totalBytes);
    RandomAccess.Read(fileHandle, dest, 0);

    ImageHeader* header = (ImageHeader*)_data;
    ImageData* images = (ImageData*)(_data + sizeof(ImageHeader));

    testImgHeader = *header;

    for (int i = 0; i < testNum; i++)
    {
        testImgData[i] = images[i];
    }

    NativeMemory.Free(_data);
}

unsafe
{
    byte* _data = null;
    long totalBytes = sizeof(LabelHeader) + sizeof(byte) * testNum;
    using SafeFileHandle fileHandle =
        File.OpenHandle(RekNNMNIST.CommonValues.Settings.TestLabelFilePath, FileMode.Open, FileAccess.Read);
    _data = (byte*)NativeMemory.Alloc((nuint)totalBytes);
    Span<byte> dest = new Span<byte>(_data, (int)totalBytes);
    RandomAccess.Read(fileHandle, dest, 0);

    LabelHeader* header = (LabelHeader*)_data;
    byte* labels = _data + sizeof(LabelHeader);

    testLabelHeader = *header;

    for (int i = 0; i < testNum; i++)
    {
        testLabelData[i] = labels[i];
    }

    NativeMemory.Free(_data);
}

//Console.WriteLine($"Train Image {0} : label={labelData[0]}");
//DisplayImage(imgData, 0);

RekNNUtility.RekNNUtility utility = new RekNNUtility.RekNNUtility(ModeEnum.MNIST, 1);

utility.DisplayTrainImage = (a) =>
{
    DisplayImage(imgData, a);
};

utility.DisplayTestImage = (a) =>
{
    DisplayImage(testImgData, a);
};

utility.GetTrainVector = (pos) =>
{
    float[][] vector = new float[1][];
    vector[0] = new float[28 * 28];
    unsafe
    {
        for (int j = 0; j < 28 * 28; j++)
        {
            vector[0][j] = imgData[pos].Image[j] / 255.0f;
        }
    }
    return vector;
};

utility.GetTrainLabel = (pos) =>
{
    return labelData[pos];
};

utility.GetTrainNum = () => imageNum;

utility.GetTestVector = (pos) =>
{
    float[][] vector = new float[1][];
    vector[0] = new float[28 * 28];
    unsafe
    {
        for (int j = 0; j < 28 * 28; j++)
        {
            vector[0][j] = testImgData[pos].Image[j] / 255.0f;
        }
    }
    return vector;
};

utility.GetTestLabel = (pos) =>
{
    return testLabelData[pos];
};

utility.GetTestNum = () => testNum;

utility.LoadModel(0, RekNNMNIST.CommonValues.Settings.DatabasePath);

while (true)
{
    Console.WriteLine($"Single instance test. (All Labels)");
    Console.WriteLine("1 ... Learn All Labels");
    Console.WriteLine("2 ... Random Predict");
    Console.WriteLine("5 ... Evaluate All Data");
    Console.WriteLine("8 ... Refine Database(All Labels)");
    Console.WriteLine("10 ... Check Score(All Labels)");
    Console.WriteLine("14 ... Save (Current Model)");
    Console.WriteLine("13 ... Get Clustering Results");

    Console.WriteLine($"Single instance test. (0 to 8 Labels)");
    Console.WriteLine("3 ... Learn Limited Label(0 to 8)");
    Console.WriteLine("4 ... Random Predict(label=9)");
    Console.WriteLine("9 ... Refine Database(0 to 8)");
    Console.WriteLine("11 ... Check Score(0 to 8)");
    Console.WriteLine("14 ... Save (Current Model)");

    Console.WriteLine("Batch test.");
    //Console.WriteLine("6 ... Test(Specified Margin)");
    //Console.WriteLine("12 ... Test(Subset)");
    Console.WriteLine("7 ... Test(ALL)");

    Console.WriteLine("Others.");
    Console.WriteLine("99 ... Exit");
    Console.Write($"Input Command No:");

    if (Console.ReadLine() is string cmd)
    {
        if (int.TryParse(cmd, out int v))
        {
            if (v == 99)
            {
                break;
            }

            switch (v)
            {
                case 1:
                    utility.LearnAllLabelsForCurrentModel(
                        RekNNMNIST.CommonValues.Settings.SearchMaxNumForAddVector, 
                        RekNNMNIST.CommonValues.Settings.SimilarityThreshold);
                    break;

                case 2:
                    Console.Write($"Search Range (recommended : 10) : ");
                    {
                        if (Console.ReadLine() is string rangeStr)
                        {
                            if (int.TryParse(rangeStr, out int range))
                            {
                                if (range <= 0)
                                {
                                    Console.WriteLine("Invalid Search Range");
                                }
                                else
                                {
                                    utility.RandomPredictForCurrentModel(range, 0.7);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid Search Range");
                            }
                        }
                    }
                    break;

                case 3:
                    utility.LearnLimitedLabelsForCurrentModel(
                        new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
                        RekNNMNIST.CommonValues.Settings.SearchMaxNumForAddVector,
                        RekNNMNIST.CommonValues.Settings.SimilarityThreshold);
                    break;

                case 4:
                    Console.Write($"Search Range (recommended : 10) : ");
                    {
                        if (Console.ReadLine() is string rangeStr)
                        {
                            if (int.TryParse(rangeStr, out int range))
                            {
                                if (range <= 0)
                                {
                                    Console.WriteLine("Invalid Search Range");
                                }
                                else
                                {
                                    utility.RandomPredictLimitedLabelsForCurrentModel(range, 0.7, new int[] { 9 });
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid Search Range");
                            }
                        }
                    }
                    break;

                case 5:
                    Console.Write($"Search Range (recommended : 10) : ");
                    {
                        if (Console.ReadLine() is string rangeStr)
                        {
                            if (int.TryParse(rangeStr, out int range))
                            {
                                if (range <= 0)
                                {
                                    Console.WriteLine("Invalid Search Range");
                                }
                                else
                                {
                                    utility.EvaluateAllDataForCurrentModel(range, 0.7);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid Search Range");
                            }
                        }
                    }
                    break;

                //case 6:
                //    Console.Write($"Margin (0.8～0.99) : ");
                //    {
                //        if (Console.ReadLine() is string marginStr)
                //        {
                //            if (double.TryParse(marginStr, out double margin))
                //            {
                //                if ( margin < 0.8 || margin > 0.99)
                //                {
                //                    Console.WriteLine("Invalid Margin");
                //                    return;
                //                }
                //                utility.TestWithMargin(
                //                    margin, 
                //                    RekNNMNIST.CommonValues.Settings.SearchMaxNumForAddVector, 
                //                    DateTime.Now.Ticks.ToString(),
                //                    RekNNMNIST.CommonValues.Settings.MaxThread);
                //            }
                //            else
                //            {
                //                Console.WriteLine("Invalid Search Range");
                //            }
                //        }
                //    }
                //    break;


                case 7:
                    utility.Test_by_Pattern(
                        RekNNMNIST.CommonValues.Settings.SearchMaxNumForAddVector,
                        DateTime.Now.Ticks.ToString(),
                        RekNNMNIST.CommonValues.Settings.MaxThread,
                        CommonValues.Settings.KValues,
                        CommonValues.Settings.DetectThresholds,
                        CommonValues.Settings.AllLabels,
                        CommonValues.Settings.DropLabels,
                        CommonValues.Settings.AddVectorThresholds,
                        CommonValues.Settings.RefineThresholds);
                    break;

                case 8:
                    utility.RefineDatabaseForCurrentModel(
                        5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                        RekNNMNIST.CommonValues.Settings.SimilarityThreshold,
                        0.01);
                    break;

                case 9:
                    utility.RefineDatabaseForCurrentModel(
                        5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
                        RekNNMNIST.CommonValues.Settings.SimilarityThreshold,
                        0.01);
                    break;

                case 10:
                    utility.CheckScoreForCurrentModel(
                        5, 0.7, 99, 
                        new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 
                        new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
                    break;

                case 11:
                    utility.CheckScoreForCurrentModel(
                        5, 0.7, 9, 
                        new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 
                        new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
                    break;

                case 12:
                    utility.Test_by_Pattern(
                        RekNNMNIST.CommonValues.Settings.SearchMaxNumForAddVector,
                        DateTime.Now.Ticks.ToString(),
                        RekNNMNIST.CommonValues.Settings.MaxThread,
                        new int[] { 1, 2, 3, 4, 5, 7, 10 },
                        new double[] { 0.5, 0.7, 0.8, 0.9},
                        new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                        new int[] { 0, 1, 2, 4, 8, 9 },
                        new double[] { 0.9, 0.95, 0.98 },
                        new double?[] { null, 0.01 }
                    );
                    break;

                case 13:
                    {
                        FileInfo fileInfo = new("SimpleClustering.csv");
                        using StreamWriter writer = fileInfo.CreateText();

                        writer.WriteLine("ClusterId,quality,label,id");

                        Dictionary<int, List<ResultItemMainAndSub>> result = utility.Clustering(0);
                        // まず、純度の測定
                        foreach (int key in result.Keys)
                        {
                            Dictionary<int, int> labelCount = new Dictionary<int, int>();
                            foreach(ResultItemMainAndSub item in result[key])
                            {
                                int label = item.MainId;
                                labelCount.TryAdd(label, 0);
                                labelCount[label]++;
                            }

                            double quality = 1;
                            if ( labelCount.Count == 0)
                            {
                                quality = 0;
                            }
                            else
                            {
                                int diff = labelCount.Values.Max();
                                quality = (double)diff / labelCount.Values.Sum();
                            }

                            // これで純度がわかった。
                            foreach (ResultItemMainAndSub item in result[key])
                            {
                                writer.WriteLine($"{key},{quality},{item.MainId},{item.SubId}");
                            }
                        }

                        writer.Flush();
                        writer.Close();
                    }
                    break;

                case 14:
                    utility.Save(0, RekNNMNIST.CommonValues.Settings.DatabasePath);
                    break;

                default:
                    Console.WriteLine("Invalid Command No");
                    break;
            }
        }
        else
        {
            Console.WriteLine("Invalid Command No");
        }
    }
}













unsafe void DisplayImage(ImageData[] img, int i)
{
    for (int row = 0; row < 28; row++)
    {
        for (int col = 0; col < 28; col++)
        {
            byte v = img[i].Image[row*28+col];
            if ( v < 64)
            {
                Console.Write("**");
            }else if (v < 128)
            {
                Console.Write(";;");
            }
            else if (v < 192)
            {
                Console.Write("..");
            }
            else
            {
                Console.Write("  ");
            }
        }
        Console.WriteLine();
    }
}





