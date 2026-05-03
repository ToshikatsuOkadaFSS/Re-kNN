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

using DocuSleuthBertLibrary;
using FuutaSystemSvcCommonLibrary;
using RekNNUtility;
using Microsoft.ML.OnnxRuntime;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security.AccessControl;
using RekNNBERT;

const int BERT_VECTOR_SIZE = 768;

RekNNBERT.CommonValues.LoadSettings();

if (!Directory.Exists(RekNNBERT.CommonValues.Settings.DatabasePath))
{
    Directory.CreateDirectory(RekNNBERT.CommonValues.Settings.DatabasePath);
}

if (File.Exists(RekNNBERT.CommonValues.Settings.OnnxPath) == false)
{
    Console.WriteLine("onnx file not found");
    return;
}

if (File.Exists(RekNNBERT.CommonValues.Settings.VocabPath) == false)
{
    Console.WriteLine("vocab file not found");
    return;
}


RekNNBERT.CommonValues.TextVectorizer = new TextVectorizer(
    RekNNBERT.CommonValues.Settings.OnnxPath,
    RekNNBERT.CommonValues.Settings.VocabPath,
    BERT_VECTOR_SIZE,
    RekNNBERT.CommonValues.Settings.MaxTokenSize
    );

RekNNUtility.RekNNUtility utility = new RekNNUtility.RekNNUtility(ModeEnum.BERT, 1);

utility.LoadModel(0, RekNNBERT.CommonValues.Settings.DatabasePath);

DateTime lapStartTime = DateTime.Now;

while (true)
{
    Console.WriteLine("1 ... Scan And Vectorize Text & Update Database & Save");
    Console.WriteLine("2 ... Scan And Vectorize Text & Update Database (Refine 10min. every 100 documents) & Save");
    Console.WriteLine("3 ... Check current database information");
    Console.WriteLine("4 ... Search Text (from Console input)");
    Console.WriteLine("5 ... Check Database");
    Console.WriteLine("6 ... Refine Database (limit : 10minutes) & Save");
    Console.WriteLine("7 ... Refine Database (all documents) & Save");
    Console.WriteLine("8 ... Refine Database (Specified File , no save");
    Console.WriteLine("9 ... Save (Current Model)");
    Console.WriteLine("10 ... Refine Database (BULK) & Save");
    Console.WriteLine("99 ... Exit");
    Console.WriteLine($"Input Command No:");

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
                    ScanAndVectorizeTextAndUpdateDatabase(utility, null, TimeSpan.FromMinutes(10));
                    break;

                case 2:
                    ScanAndVectorizeTextAndUpdateDatabase(utility, 100, TimeSpan.FromMinutes(10));
                    break;

                case 3:
                    CheckCurrentDatabaseInformation(utility);
                    break;

                case 4:
                    SearchTextFromConsoleInput(utility);
                    break;

                case 5:
                    CheckDatabase(utility);
                    break;

                case 6:
                    RefineDatabase(utility, TimeSpan.FromMinutes(10)); 
                    break;

                case 7:
                    RefineDatabase(utility, null);
                    break;

                case 8:
                    {
                        Console.Write($"Input Refine FileName:");
                        if (Console.ReadLine() is string fname)
                        {
                            if (File.Exists(fname))
                            {
                                if (RekNNBERT.CommonValues.Settings.TextFile2IdDictionary.ContainsKey(fname))
                                {
                                    RefineFile(utility, fname);
                                }
                                else
                                {
                                    Console.WriteLine("File not registered in database");
                                }
                            }
                            else
                            {
                                Console.WriteLine("File not found");
                            }
                        }
                    }
                    break;

                case 9:
                    utility.Save(0, RekNNBERT.CommonValues.Settings.DatabasePath);
                    RekNNBERT.CommonValues.SaveSettings();
                    break;

                case 10:
                    utility.RefineDatabase(0, -1);
                    utility.Save(0, RekNNBERT.CommonValues.Settings.DatabasePath);
                    RekNNBERT.CommonValues.SaveSettings();
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


////////////////////////////////////////////
/// ここから先は関数定義
/// 

/// ランダムに count だけ refine を行う。
void RefineDatabaseRandomWithCount(RekNNUtility.RekNNUtility utility, int count)
{
    if (RekNNBERT.CommonValues.TextVectorizer == null)
    {
        Console.WriteLine("TextVectorizer is not initialized");
        return;
    }

    Random rnd = new Random();

    List<int> targets = RekNNBERT.CommonValues.Settings.Id2TextFileDictionary.Keys.Select(a => (a, rnd.NextDouble())).OrderBy(a => a.Item2).Select(a => a.Item1).Take(count).ToList();

    int pos = 0;
    foreach(int id in targets)
    {
        string fullPath = RekNNBERT.CommonValues.Settings.Id2TextFileDictionary[id];

        List<string> lines = ReadFile(fullPath);

        for (int i = 0; i < lines.Count; i++)
        {
            //Console.WriteLine($"Line {i}: {lines[i]}");

            float[][] vector = RekNNBERT.CommonValues.TextVectorizer.GetVector(new string[] { lines[i] });

            if (utility.RefineBERTDatabaseForCurrentModel(
                vector,
                RekNNBERT.CommonValues.Settings.SearchMaxNumForAddVector,
                id, i,
                RekNNBERT.CommonValues.Settings.SimilarityThreshold))
            {
                Console.WriteLine($"Refined : {pos}/{targets.Count} : {id} : {i}/{lines.Count} : {vector.GetLength(0)} : {lines[i]}");
            }
            //else
            //{
            //    Console.WriteLine($"Good! : {pos}/{targets.Count} : {id} : {i}/{lines.Count} : {vector.GetLength(0)} : {lines[i]}");
            //}
        }

        pos++;
    }
}

void RefineDatabaseRandomWithSpan(RekNNUtility.RekNNUtility utility, TimeSpan span)
{
    if (RekNNBERT.CommonValues.TextVectorizer == null)
    {
        Console.WriteLine("TextVectorizer is not initialized");
        return;
    }

    Random rnd = new Random();

    List<int> targets = RekNNBERT.CommonValues.Settings.Id2TextFileDictionary.Keys.Select(a => (a, rnd.NextDouble())).OrderBy(a => a.Item2).Select(a => a.Item1).ToList();

    DateTime limit = DateTime.Now + span;

    int pos = 0;
    foreach (int id in targets)
    {
        if ( DateTime.Now > limit)
        {
            break;
        }

        string fullPath = RekNNBERT.CommonValues.Settings.Id2TextFileDictionary[id];

        List<string> lines = ReadFile(fullPath);

        for (int i = 0; i < lines.Count; i++)
        {
            //Console.WriteLine($"Line {i}: {lines[i]}");

            float[][] vector = RekNNBERT.CommonValues.TextVectorizer.GetVector(new string[] { lines[i] });

            if (utility.RefineBERTDatabaseForCurrentModel(
                vector,
                RekNNBERT.CommonValues.Settings.SearchMaxNumForAddVector,
                id, i,
                RekNNBERT.CommonValues.Settings.SimilarityThreshold))
            {
                Console.WriteLine($"Refined : {pos}/{targets.Count} : {id} : {i}/{lines.Count} : {vector.GetLength(0)} : {lines[i]}");
            }
            //else
            //{
            //    Console.WriteLine($"Good! : {pos}/{targets.Count} : {id} : {i}/{lines.Count} : {vector.GetLength(0)} : {lines[i]}");
            //}
        }

        pos++;
    }
}

void RefineDatabase(RekNNUtility.RekNNUtility utility, TimeSpan? limit)
{
    if ( RekNNBERT.CommonValues.TextVectorizer == null)
    {
        Console.WriteLine("TextVectorizer is not initialized");
        return;
    }

    Random rnd = new Random();

    FileInfo finfo = new FileInfo(System.IO.Path.Combine(RekNNBERT.CommonValues.Settings.DatabasePath, $"log-{DateTime.Now.Ticks}.log"));
    using StreamWriter writer = new(finfo.Open(FileMode.Create, FileAccess.Write, FileShare.Read));

    Console.WriteLine($"Total {RekNNBERT.CommonValues.Settings.Id2TextFileDictionary.Count} documents.");
    writer.WriteLine($"Total {RekNNBERT.CommonValues.Settings.Id2TextFileDictionary.Count} documents.");

    List<int> targets = RekNNBERT.CommonValues.Settings.Id2TextFileDictionary.Keys.Select(a => (a, rnd.NextDouble())).OrderBy(a => a.Item2).Select(a=>a.Item1).ToList();

    DateTime? limitTime = null;
    if (limit.HasValue)
    {
        limitTime = DateTime.Now + limit.Value;
    }
    DateTime startTime = DateTime.Now;

    double beforeVectorNum = utility.GetTotalVectorCount(0) ?? -1;

    int count = 0;
    int max = targets.Count;
    while (targets.Count > 0)
    {
        int id = targets.FirstOrDefault();

        string fullPath = RekNNBERT.CommonValues.Settings.Id2TextFileDictionary[id];

        List<string> lines = ReadFile(fullPath);

        for (int i = 0; i < lines.Count; i++)
        {
            //Console.WriteLine($"Line {i}: {lines[i]}");

            float[][] vector = RekNNBERT.CommonValues.TextVectorizer.GetVector(new string[] { lines[i] });

            if ( utility.RefineBERTDatabaseForCurrentModel(
                vector,
                RekNNBERT.CommonValues.Settings.SearchMaxNumForAddVector,
                id, i,
                RekNNBERT.CommonValues.Settings.SimilarityThreshold))
            {
                Console.WriteLine($"Refined : {count}/{max} : {id} : {i}/{lines.Count} : {vector.GetLength(0)} : {lines[i]}");
                writer.WriteLine($"Refined : {count}/{max} : {id} : {i}/{lines.Count} : {vector.GetLength(0)} : {lines[i]}");
            }
        }

        targets.RemoveAt(0);
        count ++;

        if ( limitTime.HasValue && DateTime.Now > limitTime.Value)
        {
            Console.WriteLine($"Time limit reached, stopping refinement ...");
            writer.WriteLine($"Time limit reached, stopping refinement ...");
            break;
        }
    }

    // 終わったら保存
    utility.Save(0, RekNNBERT.CommonValues.Settings.DatabasePath);
    RekNNBERT.CommonValues.SaveSettings();

    Console.WriteLine($"Finished. Used {(DateTime.Now - startTime).TotalHours.ToString("0.00")}hours ({(DateTime.Now - startTime).TotalMinutes.ToString("0.00")}minutes)");
    writer.WriteLine($"Finished. Used {(DateTime.Now - startTime).TotalHours.ToString("0.00")}hours ({(DateTime.Now - startTime).TotalMinutes.ToString("0.00")}minutes)");

    writer.Close();

    double afterVectorNum = utility.GetTotalVectorCount(0) ?? -1;

    Console.WriteLine($"Vector : {beforeVectorNum} -> {afterVectorNum}");
}

void RefineFile(RekNNUtility.RekNNUtility utility, string fullPath)
{
    if (RekNNBERT.CommonValues.TextVectorizer == null)
    {
        Console.WriteLine("TextVectorizer is not initialized");
        return;
    }

    if ( RekNNBERT.CommonValues.Settings.TextFile2IdDictionary.ContainsKey(fullPath) == false)
    {
        Console.WriteLine("File not registered in database");
        return;
    }

    int id = RekNNBERT.CommonValues.Settings.TextFile2IdDictionary[fullPath];

    List<string> lines = ReadFile(fullPath);

    for (int i = 0; i < lines.Count; i++)
    {
        //Console.WriteLine($"Line {i}: {lines[i]}");

        float[][] vector = RekNNBERT.CommonValues.TextVectorizer.GetVector(new string[] { lines[i] });

        if (utility.RefineBERTDatabaseForCurrentModel(
            vector,
            RekNNBERT.CommonValues.Settings.SearchMaxNumForAddVector,
            id, i,
            RekNNBERT.CommonValues.Settings.SimilarityThreshold))
        {
            Console.WriteLine($"Refined : {id} : {i}/{lines.Count} : {vector.GetLength(0)} : {lines[i]}");
        }
    }

    // 終わったら保存
    utility.Save(0, RekNNBERT.CommonValues.Settings.DatabasePath);
    RekNNBERT.CommonValues.SaveSettings();
}

void CheckDatabase(RekNNUtility.RekNNUtility utility)
{
    if (RekNNBERT.CommonValues.TextVectorizer == null)
    {
        throw new FSCLBugException("TextVectorizer is not initialized");
    }

    int fileNum = RekNNBERT.CommonValues.Settings.TextFile2IdDictionary.Count;
    Console.WriteLine($"Number of text files : {fileNum}");
    Console.Write($"Input file no : ");

    if (Console.ReadLine() is string cmd)
    {
        if (int.TryParse(cmd, out int id))
        {
            if (RekNNBERT.CommonValues.Settings.Id2TextFileDictionary.ContainsKey(id))
            {
                string fullPath = RekNNBERT.CommonValues.Settings.Id2TextFileDictionary[id];
                Console.WriteLine($"Text file : {fullPath}");
                List<string> lines = ReadFile(fullPath);

                for (int num = 0; num < lines.Count; num++)
                {
                    Console.WriteLine($"Line {num}: {lines[num]}");

                    float[][] vector = RekNNBERT.CommonValues.TextVectorizer.GetVector(
                        new string[] { lines[num] });

                    Console.WriteLine($"Vectorized : {vector.GetLength(0)} : {lines[num]}");
                }
            }
            else
            {
                Console.WriteLine("Invalid file no");
            }
        }
    }
}

void SearchTextFromConsoleInput(RekNNUtility.RekNNUtility utility)
{
    if (RekNNBERT.CommonValues.TextVectorizer == null)
    {
        Console.WriteLine("TextVectorizer is not initialized");
        return;
    }

    Console.WriteLine("Input search text:");

    if (Console.ReadLine() is string text)
    {
        List<string> lines = new();
        lines.AddRange(FSCLUnicodeLibrary.DivideTextBySentence(text, RekNNBERT.CommonValues.Settings.MaxTokenSize));

        foreach (string line in lines)
        {
            float[][] vector = RekNNBERT.CommonValues.TextVectorizer.GetVector(new string[] { line });

            Console.WriteLine($"Vector Size : {vector.GetLength(0)}");

            DateTime startTime = DateTime.Now;

            SearchResultForManaged? result
                = utility.Search(0, vector, RekNNBERT.CommonValues.Settings.SearchMaxNumForAddVector);

            if ( result == null)
            {
                Console.WriteLine("No result found");
                continue;
            }

            Console.WriteLine($"Search Time : total = {(DateTime.Now - startTime).TotalMilliseconds.ToString("0.00")} ms / {(DateTime.Now - startTime).TotalMilliseconds/vector.GetLength(0)} ms / query");

            List<ResultItemMain> resultDoc = result.ResultItemMains;
            List<ResultItemMainAndSub> resultSubDoc = result.ResultItemMainAndSubs;
            Dictionary<int, List<ResultItemMain>> resultDocByVector = result.ResultItemMainDetails;
            Dictionary<int, List<ResultItemMainAndSub>> resultSubDocByVector = result.ResultItemMainAndSubDetails;

            foreach (ResultItemMain chk in resultDoc.OrderByDescending(a => a.Score).Take(5))
            {
                RekNNBERT.CommonValues.Settings.Id2TextFileDictionary.TryGetValue(chk.MainId, out string? docPath);
                if (docPath != null)
                {
                    Console.WriteLine($"Document : {docPath}, Score: {chk.Score}");
                    List<string> foundLines = ReadFile(docPath);
                    for (int i = 0; i < int.Min(foundLines.Count, 3); i++)
                    {
                        Console.WriteLine($"Line {i}: {foundLines[i]}");
                    }
                }
            }

            foreach (ResultItemMainAndSub chk in resultSubDoc.OrderByDescending(a => a.Score).Take(5))
            {
                RekNNBERT.CommonValues.Settings.Id2TextFileDictionary.TryGetValue(chk.MainId, out string? docPath);
                if (docPath != null)
                {
                    List<string> foundLines = ReadFile(docPath);
                    Console.WriteLine($"Document : {docPath}, Score: {chk.Score}");
                    Console.WriteLine($"Line : {foundLines[chk.SubId]}");
                }
            }
        }
    }
    else
    {
        Console.WriteLine("Invalid text");
    }
}


void ScanAndVectorizeTextAndUpdateDatabase(RekNNUtility.RekNNUtility utility, int? refineStep, TimeSpan refineTime)
{
    if (Directory.Exists(RekNNBERT.CommonValues.Settings.TextFilePath) == false)
    {
        Console.WriteLine("Text file root path not found");
        return;
    }

    lapStartTime = DateTime.Now;

    FileInfo finfo = new FileInfo(System.IO.Path.Combine(RekNNBERT.CommonValues.Settings.DatabasePath, $"log-{DateTime.Now.Ticks}.csv"));
    using StreamWriter writer = new(finfo.Open(FileMode.Create, FileAccess.Write, FileShare.Read));

    writer.WriteLine($"docs, vectos, lap(minutes)");

    ScanAndVectorizeTextAndUpdateDatabaseFolder(utility, RekNNBERT.CommonValues.Settings.TextFilePath, writer, refineStep, refineTime);

    writer.Close();

    utility.Save(0, RekNNBERT.CommonValues.Settings.DatabasePath);
    RekNNBERT.CommonValues.SaveSettings();
}

void ScanAndVectorizeTextAndUpdateDatabaseFolder(RekNNUtility.RekNNUtility utility, string path, StreamWriter writer, int? refineStep, TimeSpan refineTime)
{
    if (RekNNBERT.CommonValues.TextVectorizer is null)
    {
        Console.WriteLine("TextVectorizer is not initialized");
        return;
    }

    DirectoryInfo directoryInfo = new DirectoryInfo(path);

    foreach (DirectoryInfo directoryInfo1 in directoryInfo.GetDirectories())
    {
        ScanAndVectorizeTextAndUpdateDatabaseFolder(utility, directoryInfo1.FullName, writer, refineStep, refineTime);
    }

    foreach (FileInfo fileInfo in directoryInfo.GetFiles())
    {
        if (fileInfo.Extension.ToLower() == ".txt")
        {
            if (!RekNNBERT.CommonValues.Settings.TextFile2IdDictionary.ContainsKey(fileInfo.FullName))
            {
                Console.WriteLine($"Processing {fileInfo.FullName} ...");

                int id = 0;
                if (RekNNBERT.CommonValues.Settings.TextFile2IdDictionary.Count > 0)
                {
                    id = RekNNBERT.CommonValues.Settings.TextFile2IdDictionary.Values.Max() + 1;
                }
                RekNNBERT.CommonValues.Settings.TextFile2IdDictionary.Add(fileInfo.FullName, id);
                RekNNBERT.CommonValues.Settings.Id2TextFileDictionary.Add(id, fileInfo.FullName);

                List<string> lines = ReadFile(fileInfo.FullName);

                for (int i = 0; i < lines.Count; i++)
                {
                    //Console.WriteLine($"Line {i}: {lines[i]}");

                    float[][] vector = RekNNBERT.CommonValues.TextVectorizer.GetVector(new string[] { lines[i] });

                    Console.WriteLine($"Vectorized : {id} : {i}/{lines.Count} : {vector.GetLength(0)} : {lines[i]}");

                    utility.AddVectorForCurrentModel(
                        vector, id, i, 
                        RekNNBERT.CommonValues.Settings.SearchMaxNumForAddVector,
                        RekNNBERT.CommonValues.Settings.SimilarityThreshold);
                }

                // ランダム Refine
                //RefineDatabaseRandomWithCount(utility, 2);

                if ( refineStep != null)
                {
                    if (RekNNBERT.CommonValues.Settings.TextFile2IdDictionary.Count % refineStep.Value == 0)
                    {
                        writer.WriteLine($"Refine Start!!");
                        RefineDatabaseRandomWithSpan(utility, refineTime);
                        writer.WriteLine($"Refine Finished!!");
                    }
                }

                int step = 500;
                if (RekNNBERT.CommonValues.Settings.TextFile2IdDictionary.Count > 10000)
                {
                    step = 1000;
                    if (RekNNBERT.CommonValues.Settings.TextFile2IdDictionary.Count > 50000)
                    {
                        step = 2000;
                    }
                }

                if (RekNNBERT.CommonValues.Settings.TextFile2IdDictionary.Count % step == 0)
                {
                    utility.Save(0, RekNNBERT.CommonValues.Settings.DatabasePath);
                    utility.Save(0, RekNNBERT.CommonValues.Settings.DatabasePath, RekNNBERT.CommonValues.Settings.TextFile2IdDictionary.Count);
                    RekNNBERT.CommonValues.SaveSettings();
                    RekNNBERT.CommonValues.SaveSettings(RekNNBERT.CommonValues.Settings.TextFile2IdDictionary.Count);

                    writer.WriteLine($"{RekNNBERT.CommonValues.Settings.TextFile2IdDictionary.Count},{(utility.GetTotalVectorCount(0) ?? -1).ToString("0")},{(DateTime.Now - lapStartTime).TotalMinutes.ToString("0.00")}");

                    // sleep for a while to avoid high CPU usage    
                    Thread.Sleep(60 * 1000);

                    lapStartTime = DateTime.Now;
                }
            }
            else
            {
                Console.WriteLine($"Already processed {fileInfo.FullName}, skipping ...");
            }

            Console.WriteLine($"Total Vector : {(utility.GetTotalVectorCount(0) ?? -1).ToString("0")}");
        }
    }

    return;
}

void CheckCurrentDatabaseInformation(RekNNUtility.RekNNUtility utility)
{
    Console.WriteLine($"Total Vector : {(utility.GetTotalVectorCount(0) ?? -1).ToString("0")}");
}

List<string> ReadFile(string fullPath)
{
    string fulltext = File.ReadAllText(fullPath);
    List<string> splitTexts = fulltext.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    List<string> lines = new();
    foreach (string text in splitTexts)
    {
        lines.AddRange(FSCLUnicodeLibrary.DivideTextBySentence(text, RekNNBERT.CommonValues.Settings.MaxTokenSize));
    }

    return lines;
}