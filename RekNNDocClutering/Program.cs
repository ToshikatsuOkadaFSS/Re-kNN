using FuutaSystemSvcCommonLibrary;
using RekNNUtility;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security.AccessControl;
using RekNNDocClutering;
using System.Runtime.InteropServices;
using System.Text;

const int BERT_VECTOR_SIZE = 768;


RekNNDocClutering.CommonValues.LoadSettings();

if (!Directory.Exists(RekNNDocClutering.CommonValues.Settings.DatabasePath))
{
    Directory.CreateDirectory(RekNNDocClutering.CommonValues.Settings.DatabasePath);
}


RekNNUtility.RekNNUtility utility = new RekNNUtility.RekNNUtility(ModeEnum.BERT, 1);

utility.LoadModel(0, RekNNDocClutering.CommonValues.Settings.DatabasePath);

DateTime lapStartTime = DateTime.Now;

while (true)
{
    Console.WriteLine("1 ... Vectorize (with skip added files)");
    Console.WriteLine("2 ... Refine Database (BULK) & Save");
    Console.WriteLine("3 ... Output cluster info");
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
                    {
                        Console.Write("Input max documents (default 10000): ");
                        int maxDocuments = 10000;
                        if (Console.ReadLine() is string input1)
                        {
                            if (int.TryParse(input1, out int result))
                            {
                                maxDocuments = result;
                            }
                        }

                        Console.Write("Input max add documents (default 1000): ");
                        int maxAddDocuments = 1000;
                        if (Console.ReadLine() is string input2)
                        {
                            if (int.TryParse(input2, out int result))
                            {
                                maxAddDocuments = result;
                            }
                        }

                        ScanAndVectorizeTextAndUpdateDatabase(utility, maxDocuments,maxAddDocuments);
                    }
                    break;

                case 2:
                    utility.RefineDatabase(0, -1);
                    utility.Save(0, RekNNDocClutering.CommonValues.Settings.DatabasePath);
                    RekNNDocClutering.CommonValues.SaveSettings();
                    break;

                case 3:
                    {
                        Console.WriteLine("Checking...");

                        Dictionary<int, List<ResultItemMainAndSub>> result = utility.Clustering(0);

                        // クラスタリングの結果は、text で cluster-id, main-id, sub-id をカンマ区切りで出力する
                        if ( Directory.Exists(RekNNDocClutering.CommonValues.Settings.DatabasePath) == false)
                        {
                            Directory.CreateDirectory(RekNNDocClutering.CommonValues.Settings.DatabasePath);
                        }

                        FileInfo finfo = new FileInfo(System.IO.Path.Combine(RekNNDocClutering.CommonValues.Settings.DatabasePath, $"clustering_result_{DateTime.Now:yyyyMMdd_HHmmss}.txt"));
                        using FileStream st = finfo.Create();
                        using StreamWriter sw = new StreamWriter(st, Encoding.UTF8);

                        foreach(int cls in result.Keys)
                        {
                            foreach(ResultItemMainAndSub item in result[cls])
                            {
                                string textFilePath = RekNNDocClutering.CommonValues.Settings.Id2TextFileDictionary.ContainsKey(item.MainId) ? RekNNDocClutering.CommonValues.Settings.Id2TextFileDictionary[item.MainId] : "Unknown";
                                sw.WriteLine(string.Join(",", new int[] { cls, item.MainId, item.SubId }.Select(a=>a.ToString())));
                            }
                        }
                    }
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

RekNNDocClutering.CommonValues.SaveSettings();


////////////////////////////////////////////
/// ここから先は関数定義
/// 


void ScanAndVectorizeTextAndUpdateDatabase(RekNNUtility.RekNNUtility utility, int maxDocuments, int maxAddDocuments)
{
    if (Directory.Exists(RekNNDocClutering.CommonValues.Settings.TextFilePath) == false)
    {
        Console.WriteLine("Text file root path not found");
        return;
    }

    if (Directory.Exists(RekNNDocClutering.CommonValues.Settings.VectorFilePath) == false)
    {
        Console.WriteLine("Vector file root path not found");
        return;
    }

    lapStartTime = DateTime.Now;

    ScanAndVectorizeTextAndUpdateDatabaseFolder(
        utility, 
        RekNNDocClutering.CommonValues.Settings.TextFilePath,
        RekNNDocClutering.CommonValues.Settings.VectorFilePath,
        maxDocuments,
        maxAddDocuments,
        0);

    utility.Save(0, RekNNDocClutering.CommonValues.Settings.DatabasePath);
    RekNNDocClutering.CommonValues.SaveSettings();
}

int ScanAndVectorizeTextAndUpdateDatabaseFolder(
    RekNNUtility.RekNNUtility utility, 
    string filePath,
    string vectorPath, 
    int maxDocuments,
    int maxAddDocuments,
    int count)
{
    DirectoryInfo directoryInfoFile = new DirectoryInfo(filePath);
    DirectoryInfo directoryInfoVector = new DirectoryInfo(filePath);

    foreach (DirectoryInfo directoryInfo1 in directoryInfoVector.GetDirectories().OrderBy(d => d.Name))
    {
        string newFilePath = Path.Combine(filePath, directoryInfo1.Name);
        string newVectorPath = Path.Combine(vectorPath, directoryInfo1.Name);

        if (System.IO.Directory.Exists(newVectorPath) == false)
        {
            continue;
        }
        if (System.IO.Directory.Exists(newFilePath) == false)
        {
            continue;
        }

        count = ScanAndVectorizeTextAndUpdateDatabaseFolder(
            utility, newFilePath, newVectorPath, maxDocuments, maxAddDocuments, count);

        if (count >= maxAddDocuments)
        {
            return count;
        }

        if (RekNNDocClutering.CommonValues.Settings.TextFile2IdDictionary.Count >= maxDocuments)
        {
            return count;
        }
    }

    foreach (FileInfo fileInfo in directoryInfoVector.GetFiles().OrderBy(f => f.Name))
    {
        if (fileInfo.Extension.ToLower() == ".txt")
        {
            string chkFilePath = Path.Combine(filePath, fileInfo.Name);
            string chkVectorPath = Path.Combine(vectorPath, fileInfo.Name);

            if (System.IO.File.Exists(chkFilePath) == false)
            {
                continue;
            }
            if (System.IO.File.Exists(chkVectorPath) == false)
            {
                continue;
            }

            if (!RekNNDocClutering.CommonValues.Settings.TextFile2IdDictionary.ContainsKey(chkFilePath))
            {
                Console.WriteLine($"Processing {fileInfo.FullName} ...");

                int id = 0;
                if (RekNNDocClutering.CommonValues.Settings.TextFile2IdDictionary.Count > 0)
                {
                    id = RekNNDocClutering.CommonValues.Settings.TextFile2IdDictionary.Values.Max() + 1;
                }
                RekNNDocClutering.CommonValues.Settings.TextFile2IdDictionary.Add(chkFilePath, id);
                RekNNDocClutering.CommonValues.Settings.Id2TextFileDictionary.Add(id, chkFilePath);

                List<string> lines = System.IO.File.ReadAllLines(chkVectorPath).ToList();

                // lineNo- token & vector
                Dictionary<int, List<(string, float[])>> vectorInfo = new();

                try
                {
                    for (int i = 0; i < lines.Count; i += 3)
                    {
                        //Console.WriteLine($"Line {i}: {lines[i]}");
                        if (int.TryParse(lines[i + 0], out int lineNo))
                        {
                            string token = lines[i + 1];
                            float[] vec = lines[i + 2].Split(',').Select(s => float.Parse(s)).ToArray();

                            if (vec.Length == BERT_VECTOR_SIZE)
                            {
                                // OK である
                                vectorInfo.TryAdd(lineNo, new List<(string, float[])>());
                                vectorInfo[lineNo].Add((token, vec));
                            }
                        }
                    }

                    foreach(int lineNo in vectorInfo.Keys)
                    {
                        string joinedToken = string.Join(" ", vectorInfo[lineNo].Select(t => t.Item1));
                        Console.WriteLine($"Adding Vector {id},{lineNo}: " + joinedToken);

                        float[][] vectors = vectorInfo[lineNo].Select(t => t.Item2).ToArray();

                        utility.AddVectorForCurrentModel(
                            vectors, id, lineNo,
                            RekNNDocClutering.CommonValues.Settings.SearchMaxNumForAddVector,
                            RekNNDocClutering.CommonValues.Settings.SimilarityThreshold);


                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {fileInfo.FullName} : {ex.Message}");
                }

                count += 1;

                Console.WriteLine($"Total Vector : {(utility.GetTotalVectorCount(0) ?? -1).ToString("0")}");

                if (count >= maxAddDocuments)
                {
                    return count;
                }

                if (RekNNDocClutering.CommonValues.Settings.TextFile2IdDictionary.Count >= maxDocuments)
                {
                    return count;
                }
            }
            else
            {
                Console.WriteLine($"Already processed {fileInfo.FullName}, skipping ...");
            }
        }
    }

    return count;
}


List<string> ReadFile(string fullPath)
{
    string fulltext = File.ReadAllText(fullPath);
    List<string> splitTexts = fulltext.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    List<string> lines = new();
    foreach (string text in splitTexts)
    {
        lines.AddRange(FSCLUnicodeLibrary.DivideTextBySentence(text));
    }

    return lines;
}