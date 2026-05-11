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
using RekNNUtility;
using RekNNCIFAR10;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

RekNNCIFAR10.CommonValues.LoadSettings();

foreach (string path in RekNNCIFAR10.CommonValues.Settings.TrainFilePaths)
{
    if (File.Exists(path) == false)
    {
        Console.WriteLine($"{path} file not found");
        return;
    }
}

if (File.Exists(RekNNCIFAR10.CommonValues.Settings.TestFilePath) == false)
{
    Console.WriteLine($"{RekNNCIFAR10.CommonValues.Settings.TestFilePath} file not found");
    return;
}

int imageNum = 50000;
int imageNumSub = 10000;
ImageData[] imgData = new ImageData[imageNum];

unsafe
{
    for (int i = 0; i < RekNNCIFAR10.CommonValues.Settings.TrainFilePaths.Count; i++)
    {
        byte* _data = null;
        long totalBytes = sizeof(ImageData) * imageNumSub;
        using SafeFileHandle fileHandle =
            File.OpenHandle(RekNNCIFAR10.CommonValues.Settings.TrainFilePaths[i], FileMode.Open, FileAccess.Read);
        _data = (byte*)NativeMemory.Alloc((nuint)totalBytes);
        Span<byte> dest = new Span<byte>(_data, (int)totalBytes);
        RandomAccess.Read(fileHandle, dest, 0);

        ImageData* images = (ImageData*)(_data);

        for (int j = 0; j < imageNumSub; j++)
        {
            imgData[i * imageNumSub + j] = images[j];
        }

        NativeMemory.Free(_data);
    }
}

int testNum = 10000;
ImageData[] testImgData = new ImageData[testNum];

unsafe
{
    byte* _data = null;
    long totalBytes = sizeof(ImageData) * testNum;
    using SafeFileHandle fileHandle =
        File.OpenHandle(RekNNCIFAR10.CommonValues.Settings.TestFilePath, FileMode.Open, FileAccess.Read);
    _data = (byte*)NativeMemory.Alloc((nuint)totalBytes);
    Span<byte> dest = new Span<byte>(_data, (int)totalBytes);
    RandomAccess.Read(fileHandle, dest, 0);

    ImageData* images = (ImageData*)(_data);

    for (int j = 0; j < testNum; j++)
    {
        testImgData[j] = images[j];
    }

    NativeMemory.Free(_data);
}


//Console.WriteLine($"Train Image {0} : label={labelData[0]}");
//DisplayImage(imgData, 0);

RekNNUtility.RekNNUtility utility = new RekNNUtility.RekNNUtility(ModeEnum.CIFAR10, 1);

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
    vector[0] = new float[32 * 32 * 3];
    unsafe
    {
        for (int j = 0; j < 32 * 32 * 3; j++)
        {
            vector[0][j] = imgData[pos].Image[j] / 255.0f;
        }
    }
    return vector;
};

utility.GetTrainLabel = (pos) =>
{
    return imgData[pos].Label;
};

utility.GetTrainNum = () => imageNum;

utility.GetTestVector = (pos) =>
{
    float[][] vector = new float[1][];
    vector[0] = new float[32 * 32 * 3];
    unsafe
    {
        for (int j = 0; j < 32 * 32 * 3; j++)
        {
            vector[0][j] = testImgData[pos].Image[j] / 255.0f;
        }
    }
    return vector;
};

utility.GetTestLabel = (pos) =>
{
    return testImgData[pos].Label;
};

utility.GetTestNum = () => testNum;


utility.LoadModel(0, RekNNCIFAR10.CommonValues.Settings.DatabasePath);


while (true)
{
    Console.WriteLine($"Single instance test. (All Labels)");
    Console.WriteLine("1 ... Learn All Labels");
    Console.WriteLine("2 ... Random Predict");
    Console.WriteLine("5 ... Evaluate All Data");
    Console.WriteLine("8 ... Refine Database(All Labels)");
    Console.WriteLine("10 ... Check Score(All Labels)");
    Console.WriteLine("12 ... Save (Current Model)");

    Console.WriteLine($"Single instance test. (0 to 8 Labels)");
    Console.WriteLine("3 ... Learn Limited Label(0 to 8)");
    Console.WriteLine("4 ... Random Predict(label=9)");
    Console.WriteLine("9 ... Refine Database(0 to 8)");
    Console.WriteLine("11 ... Check Score(0 to 8)");
    Console.WriteLine("12 ... Save (Current Model)");

    Console.WriteLine("Batch test.");
    //Console.WriteLine("6 ... Test(Specified Margin)");
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
                        RekNNCIFAR10.CommonValues.Settings.SearchMaxNumForAddVector,
                        RekNNCIFAR10.CommonValues.Settings.SimilarityThreshold);
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
                        RekNNCIFAR10.CommonValues.Settings.SearchMaxNumForAddVector,
                        RekNNCIFAR10.CommonValues.Settings.SimilarityThreshold);
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

                case 7:
                    utility.Test_by_Pattern(
                        RekNNCIFAR10.CommonValues.Settings.SearchMaxNumForAddVector,
                        DateTime.Now.Ticks.ToString(),
                        RekNNCIFAR10.CommonValues.Settings.MaxThread,
                        RekNNCIFAR10.CommonValues.Settings.KValues,
                        RekNNCIFAR10.CommonValues.Settings.DetectThresholds,
                        RekNNCIFAR10.CommonValues.Settings.AllLabels,
                        RekNNCIFAR10.CommonValues.Settings.DropLabels,
                        RekNNCIFAR10.CommonValues.Settings.AddVectorThresholds,
                        RekNNCIFAR10.CommonValues.Settings.RefineThresholds,
                        RekNNCIFAR10.CommonValues.Settings.Refine);
                    break;

                case 8:
                    utility.RefineDatabaseForCurrentModel(
                        5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                        RekNNCIFAR10.CommonValues.Settings.SimilarityThreshold,
                        0.01);
                    break;

                case 9:
                    utility.RefineDatabaseForCurrentModel(
                        5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
                        RekNNCIFAR10.CommonValues.Settings.SimilarityThreshold,
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
                    utility.Save(0, RekNNCIFAR10.CommonValues.Settings.DatabasePath);
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
    for (int row = 0; row < 32; row++)
    {
        for (int col = 0; col < 32; col++)
        {
            int v = img[i].Image[(row * 32 + col)] + img[i].Image[1024 + (row * 32 + col)] + img[i].Image[2048 + (row * 32 + col)];

            if (v < 64 * 3)
            {
                Console.Write("  ");
            }
            else if (v < 128 * 3)
            {
                Console.Write("..");
            }
            else if (v < 192 * 3)
            {
                Console.Write(";;");
            }
            else
            {
                Console.Write("**");
            }
        }
        Console.WriteLine();
    }
}



