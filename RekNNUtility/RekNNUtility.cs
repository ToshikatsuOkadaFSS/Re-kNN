using FuutaSystemSvcCommonLibrary;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;

namespace RekNNUtility
{
    /// <summary>
    /// 動作モード
    /// </summary>
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



    public class RekNNUtility
    {
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

        [DllImport("DLL\\FuutaSystemSvcVectorLibrary")]
        private static extern void SetDebugMode(int mode);


        /// <summary>
        /// 画像を表示する関数(引数はデータ番号)
        /// </summary>
        public Action<int>? DisplayTrainImage { get; set; } = null;

        /// <summary>
        /// 画像を表示する関数(引数はデータ番号)
        /// </summary>
        public Action<int>? DisplayTestImage { get; set; } = null;

        /// <summary>
        /// ベクトル情報を獲得する
        /// </summary>
        public Func<int, float[][]>? GetTrainVector { get; set; } = null;

        /// <summary>
        /// ベクトル情報を獲得する
        /// </summary>
        public Func<int, int>? GetTrainLabel { get; set; } = null;


        public Func<int>? GetTrainNum { get; set; } = null;

        /// <summary>
        /// ベクトル情報を獲得する
        /// </summary>
        public Func<int, float[][]>? GetTestVector { get; set; } = null;

        /// <summary>
        /// ベクトル情報を獲得する
        /// </summary>
        public Func<int, int>? GetTestLabel { get; set; } = null;


        public Func<int>? GetTestNum { get; set; } = null;

        /// <summary>
        /// ユーティリティの動作モード
        /// </summary>
        public ModeEnum CurrentMode { get; }

        public int dimension { get; }

        /// <summary>
        /// ログを表示する関数
        /// </summary>
        public static Action<string?> DisplayMessage { get; set; } = Console.WriteLine;

        /// <summary>
        /// DLLのリゾルバー。DLLが見つからない場合に呼び出される。ここでDLLの場所を指定する。
        /// </summary>
        /// <param name="libraryName"></param>
        /// <param name="assembly"></param>
        /// <param name="searchPath"></param>
        /// <returns></returns>
        private static IntPtr ResolveNativeLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName == "FuutaSystemSvcVectorLibrary")
            {
                // 実行ファイルの場所を取得
                string baseDir = AppContext.BaseDirectory;

                // OSに応じた拡張子と接頭辞を判定
                string libFileName = libraryName;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    libFileName = $"{libraryName}.dll";
                    //DisplayMessage($"Win:Lib:{libFileName}");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    libFileName = $"{libraryName}.so";
                    //DisplayMessage($"Linux:Lib:{libFileName}");
                }

                // 「実行ファイル/DLL/ライブラリ名」のパスを作成
                string libPath = Path.Combine(baseDir, "DLL", libFileName);

                // ライブラリをロード
                if (NativeLibrary.TryLoad(libPath, out IntPtr handle))
                {
                    return handle;
                }
            }

            // 見つからない場合は IntPtr.Zero を返すと、標準の探索ルールにフォールバックされる
            return IntPtr.Zero;
        }

        public RekNNUtility(ModeEnum mode, int instanceNum)
        {
            // リゾルバーを登録する
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), ResolveNativeLibrary);


            Initialize(mode, instanceNum);

            this.CurrentMode = mode;

            switch (mode)
            {
                case ModeEnum.MNIST:
                    this.dimension = 28 * 28;
                    break;
                case ModeEnum.CIFAR10:
                    this.dimension = 32 * 32 * 3;
                    break;
                case ModeEnum.BERT:
                    this.dimension = 768;
                    break;
                default:
                    throw new FSCLBugException("Unsupported mode");
            }
        }


        public void LoadModel(int instanceNo, string basePath)
        {
            // 1. C#のstring(UTF-16)を、UTF-8のバイト配列に変換
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(basePath);

            unsafe
            {
                fixed (byte* pText = utf8Bytes)
                {
                    if (Load(instanceNo, pText, utf8Bytes.Length))
                    {
                        StatusDetailEnum status = GetStatusDetail();
                        DisplayMessage($"Load:true:{status.ToString()}");
                    }
                    else
                    {
                        StatusDetailEnum status = GetStatusDetail();
                        DisplayMessage($"Load:falase:{status.ToString()}");
                    }
                }
            }

        }



        public void Test_by_Pattern(int searchMax, string logPath, int maxThread, IEnumerable<int> kValues, IEnumerable<double> detectThresholds, IEnumerable<int> allLabels, IEnumerable<int> dropLabels, IEnumerable<double> addVectorThresholds, IEnumerable<double?> refineThresholds, bool[] refine)
        {
            //FuutaSystemSvcVectorLibrary.CommonValues.Initialize(RekNNMNIST, )

            // data は threshold, dropLabel, targetLabels, allLabels
            List<(double, int, IEnumerable<int>, IEnumerable<int>, double?, bool)> testParams = new();

            // パラメタ生成
            foreach (bool refineFlag in refine)
            {
                foreach (double? refineThreshold in refineThresholds)
                {
                    foreach (double threshold in addVectorThresholds)
                    {
                        foreach (byte dropLabel in dropLabels)
                        {
                            List<int> targetLabels = new();
                            foreach (int chk in allLabels)
                            {
                                if (chk != dropLabel)
                                {
                                    targetLabels.Add(chk);
                                }
                            }

                            testParams.Add((threshold, dropLabel, targetLabels, allLabels, refineThreshold, refineFlag));
                        }
                    }
                }
            }

            if (Directory.Exists(logPath) == false)
            {
                Directory.CreateDirectory(logPath);
            }

            Semaphore semaphore = new(maxThread, maxThread);

            Initialize(CurrentMode, testParams.Count);

            Parallel.For(0, testParams.Count, (modelNo) =>
            {
                (double, int, IEnumerable<int>, IEnumerable<int>, double?, bool) arg = testParams[modelNo];

                string logFileName = System.IO.Path.Combine(logPath, $"result-{arg.Item1.ToString("0.00")}-{arg.Item2}-{string.Join("-", arg.Item3.Select(a => a.ToString()))}-{arg.Item5?.ToString("0.00") ?? "none"}-{arg.Item6}.csv");
                FileInfo finfo = new(logFileName);
                using StreamWriter writer = new(finfo.Open(FileMode.Create, FileAccess.Write, FileShare.Read));

                string logFileName2 = System.IO.Path.Combine(logPath, $"result2-{arg.Item1.ToString("0.00")}-{arg.Item2}-{string.Join("-", arg.Item3.Select(a => a.ToString()))}-{arg.Item5?.ToString("0.00") ?? "none"}-{arg.Item6}.csv");
                FileInfo finfo2 = new(logFileName2);
                using StreamWriter writer2 = new(finfo2.Open(FileMode.Create, FileAccess.Write, FileShare.Read));

                string logFileName3 = System.IO.Path.Combine(logPath, $"result3-{arg.Item1.ToString("0.00")}-{arg.Item2}-{string.Join("-", arg.Item3.Select(a => a.ToString()))}-{arg.Item5?.ToString("0.00") ?? "none"}-{arg.Item6}.csv");
                FileInfo finfo3 = new(logFileName3);

                using StreamWriter writer3 = new(finfo3.Open(FileMode.Create, FileAccess.Write, FileShare.Read)); try
                {
                    semaphore.WaitOne();

                    writer2.WriteLine($"refine, refineflag, k, dbThreshold, detectThreshold, totalTime, predictTime, skipLabel, targetLabels, predictLabels, {string.Join(",", allLabels.Select(a => a.ToString()))}, Unknown");
                    writer3.WriteLine($"refine, refineflag, k, dbThreshold, detectThreshold, skipLabel, targetLabels, predictLabels, testLabel, testNo, voteLabel, voteNo, voteScore");

                    TestSub(
                        modelNo, writer, writer2, writer3, arg.Item2, arg.Item3, arg.Item4, searchMax, arg.Item1, arg.Item5, arg.Item6,
                        kValues, detectThresholds);
                }
                catch (Exception err)
                {
                    DisplayMessage($"Error: {err.ToString()}");
                    DisplayMessage(err.Message);
                    DisplayMessage(err.StackTrace);

                    writer.WriteLine($"Error: {err.ToString()}");
                    writer.WriteLine(err.Message);
                    writer.WriteLine(err.StackTrace);
                    writer2.WriteLine($"Error: {err.ToString()}");
                    writer2.WriteLine(err.Message);
                    writer2.WriteLine(err.StackTrace);
                    writer3.WriteLine($"Error: {err.ToString()}");
                    writer3.WriteLine(err.Message);
                    writer3.WriteLine(err.StackTrace);
                }
                finally
                {
                    writer.Close();
                    writer2.Close();
                    writer3.Close();

                    semaphore.Release();
                }
            });
        }




        private unsafe void TestSub(
            int modelNo, 
            StreamWriter writer, 
            StreamWriter writer2, 
            StreamWriter writer3, 
            int skipLabel, 
            IEnumerable<int> targetLabels, 
            IEnumerable<int> allLabels, 
            int searchMax, 
            double addVectorThreshold, 
            double? refineThreshold,
            bool refineFlag,
            IEnumerable<int> kValues,
            IEnumerable<double> detectThresholds)
        {
            if (GetTrainVector == null)
            {
                throw new FSCLBugException("GetTrainVector is not set");
            }
            if (GetTrainLabel == null)
            {
                throw new FSCLBugException("GetTrainLabel is not set");
            }
            if (GetTrainNum == null)
            {
                throw new FSCLBugException("GetTrainNum is not set");
            }
            if (GetTestVector == null)
            {
                throw new FSCLBugException("GetTrainVector is not set");
            }
            if (GetTestLabel == null)
            {
                throw new FSCLBugException("GetTrainLabel is not set");
            }
            if (GetTestNum == null)
            {
                throw new FSCLBugException("GetTrainNum is not set");
            }

            // 学習
            LearnLimitedLabels(modelNo, targetLabels.ToArray(), searchMax, addVectorThreshold);



            if (refineThreshold != null)
            {
                RefineDatabase(modelNo, searchMax, targetLabels, addVectorThreshold, refineThreshold.Value);
            }

            if (refineFlag)
            {
                RefineAll(modelNo, searchMax);
            }

            // 評価
            foreach (int kValue in kValues)
            {
                foreach (double detectThreshold in detectThresholds)
                {
                    DateTime startTime = DateTime.Now;

                    (Dictionary<int, Dictionary<int, int>>, int, List<(int?, int, int, int, int, double)>) reslt = TestMain(modelNo, kValue, detectThreshold, targetLabels);

                    Dictionary<int, Dictionary<int, int>> evaluateReslt = reslt.Item1;

                    int count2 = reslt.Item2;

                    double timeSec = (DateTime.Now - startTime).TotalMilliseconds;

                    int correct = 0;
                    int labelErrorCount = 0;
                    int unknownCount = 0;

                    DisplayMessage($"refine, refineflag, k, dbThreshold, detectThreshold, totalTime, predictTime, skipLabel, targetLabels");
                    writer.WriteLine($"refine, refineflag, k, dbThreshold, detectThreshold, totalTime, predictTime, skipLabel, targetLabels");

                    DisplayMessage($"{refineThreshold?.ToString("0.00") ?? "none"}. {refineFlag}, {kValue},{addVectorThreshold.ToString("0.00")}, {detectThreshold},{timeSec.ToString("0.00")},{(timeSec / count2).ToString("0.00")},{skipLabel}," + string.Join("-", targetLabels.Select(a => a.ToString()).ToArray()));
                    writer.WriteLine($"{refineThreshold?.ToString("0.00") ?? "none"}. {refineFlag}, {kValue},{addVectorThreshold.ToString("0.00")}, {detectThreshold},{timeSec.ToString("0.00")},{(timeSec / count2).ToString("0.00")},{skipLabel}," + string.Join("-", targetLabels.Select(a => a.ToString()).ToArray()));

                    string line2 = $"{refineThreshold?.ToString("0.00") ?? "none"},{refineFlag},{kValue},{addVectorThreshold.ToString("0.00")}, {detectThreshold},{timeSec.ToString("0.00")},{(timeSec / count2).ToString("0.00")},{skipLabel}," + string.Join("-", targetLabels.Select(a => a.ToString()).ToArray());

                    DisplayMessage("label," + string.Join(",", allLabels.Select(a => a.ToString())) + ",unknown");
                    writer.WriteLine("label," + string.Join(",", allLabels.Select(a => a.ToString())) + ",unknown");

                    foreach ((int? predictedLabel, int chklabel, int chkpos, int mainId, int subId, double score) in reslt.Item3)
                    {
                        writer3.WriteLine($"{refineThreshold?.ToString("0.00") ?? "none"},{refineFlag},{kValue},{addVectorThreshold.ToString("0.00")}, {detectThreshold.ToString("0.00")}, {skipLabel}, {string.Join("-", targetLabels.Select(a => a.ToString()))}, {predictedLabel ?? -1}, {chklabel}, {chkpos}, {mainId}, {subId}, {score.ToString("0.0000")}");
                    }

                    foreach (byte label in allLabels)
                    {
                        string line = string.Empty;

                        line += $"Label_{label}";
                        Console.Write($"Label {label}");

                        foreach (int i in allLabels)
                        {
                            if (evaluateReslt[label].ContainsKey(i))
                            {
                                Console.Write($" : {evaluateReslt[label][i]}");
                                line += $",{evaluateReslt[label][i]}";
                                if (i == label)
                                {
                                    correct += evaluateReslt[label][i];
                                }
                                else
                                {
                                    if (targetLabels.Contains(label))
                                    {
                                        labelErrorCount += evaluateReslt[label][i];
                                    }
                                }
                            }
                            else
                            {
                                Console.Write($" : 0");
                                line += $",{0}";
                            }
                        }
                        if (evaluateReslt[label].ContainsKey(-1))
                        {
                            DisplayMessage($" : {evaluateReslt[label][-1]}");
                            line += $",{evaluateReslt[label][-1]}";
                            if (targetLabels.Contains(label))
                            {
                                unknownCount += evaluateReslt[label][-1];
                            }
                        }
                        else
                        {
                            DisplayMessage($" : {0}");
                            line += $",{0}";
                        }

                        writer.WriteLine(line);
                        writer2.WriteLine(line2 + "," + line);
                    }

                    DisplayMessage($"Accuracy, {(double)correct / (GetTestNum())}, {correct} / {GetTestNum()}");
                    writer.WriteLine($"Accuracy, {(double)correct / (GetTestNum())}, {correct} / {GetTestNum()}");

                    DisplayMessage($"Accuracy2, {(double)correct / (count2)}, {correct} / {count2}");
                    writer.WriteLine($"Accuracy2, {(double)correct / (count2)}, {correct} / {count2}");

                    DisplayMessage($"Label Error({string.Join("-", targetLabels.Select(a => a.ToString()))}), {labelErrorCount}");
                    writer.WriteLine($"Label Error({string.Join("-", targetLabels.Select(a => a.ToString()))}), {labelErrorCount}");

                    DisplayMessage($"Unknown Data Count ({string.Join("-", targetLabels.Select(a => a.ToString()))}), {unknownCount}");
                    writer.WriteLine($"Unknown Data Count ({string.Join("-", targetLabels.Select(a => a.ToString()))}), {unknownCount}");

                    if ( allLabels.Contains(skipLabel))
                    {
                        // skipLabel が評価対象に含まれている場合は、skipLabel の正解率も出力する
                        DisplayMessage($"Error({skipLabel}), ErrorCount({skipLabel}), Total({skipLabel})");
                        writer.WriteLine($"Error({skipLabel}), ErrorCount({skipLabel}), Total({skipLabel})");
                        if (evaluateReslt[skipLabel].ContainsKey(-1))
                        {
                            DisplayMessage($"{(double)evaluateReslt[skipLabel][-1] / evaluateReslt[skipLabel].Values.Sum()}, {evaluateReslt[skipLabel][-1]}, {evaluateReslt[skipLabel].Values.Sum()}");
                            writer.WriteLine($"{(double)evaluateReslt[skipLabel][-1] / evaluateReslt[skipLabel].Values.Sum()}, {evaluateReslt[skipLabel][-1]}, {evaluateReslt[skipLabel].Values.Sum()}");
                        }
                        else
                        {
                            DisplayMessage($"{(double)0 / evaluateReslt[skipLabel].Values.Sum()}, {0}, {evaluateReslt[skipLabel].Values.Sum()}");
                            writer.WriteLine($"{(double)0 / evaluateReslt[skipLabel].Values.Sum()}, {0}, {evaluateReslt[skipLabel].Values.Sum()}");
                        }
                    }

                    DisplayMessage("");
                    writer.WriteLine("");
                    writer.Flush();
                    writer2.Flush();
                    writer3.Flush();
                }
            }
        }

        private unsafe void LearnLimitedLabels(int modelNo, IEnumerable<int> targetLabels, int searchMax, double threshold)
        {
            if (GetTrainVector == null)
            {
                throw new FSCLBugException("GetTrainVector is not set");
            }
            if (GetTrainLabel == null)
            {
                throw new FSCLBugException("GetTrainLabel is not set");
            }
            if (GetTrainNum == null)
            {
                throw new FSCLBugException("GetTrainNum is not set");
            }

            DateTime nextTime = DateTime.Now.AddSeconds(-1);

            for (int i = 0; i < GetTrainNum(); i++)
            {
                if (targetLabels.Contains(GetTrainLabel(i)) == false)
                {
                    continue;
                }

                float[][] vector = GetTrainVector(i);

                float* pVector = null;

                try
                {
                    pVector = ConvertVector(vector);

                    Add(modelNo, pVector, vector.GetLength(0), GetTrainLabel(i), i, searchMax, threshold);
                }
                finally
                {
                    if (pVector != null)
                    {
                        NativeMemory.Free(pVector);
                    }
                }

                if (DateTime.Now > nextTime)
                {
                    DisplayMessage($"Learned {i + 1} / {GetTrainNum()} images");
                    nextTime = DateTime.Now.AddSeconds(10);
                }
            }
        }

        public unsafe void LearnLimitedLabelsForCurrentModel(IEnumerable<int> targetLabels, int searchMax, double threshold)
        {
            LearnLimitedLabels(0, targetLabels, searchMax, threshold);
        }



        /// データベースの洗練
        private unsafe void RefineDatabase(int modelNo, int kValue, IEnumerable<int> targetLabels, double addVectorThreshold, double targetThreshold)
        {
            if (GetTrainVector == null)
            {
                throw new FSCLBugException("GetTrainVector is not set");
            }
            if (GetTrainLabel == null)
            {
                throw new FSCLBugException("GetTrainLabel is not set");
            }
            if (GetTrainNum == null)
            {
                throw new FSCLBugException("GetTrainNum is not set");
            }

            int tryCount = 0;

            while (true)
            {
                int count = 0;

                DisplayMessage($"Refine Try Count : {tryCount}");

                DateTime nextTime = DateTime.Now.AddSeconds(1);

                for (int i = 0; i < GetTrainNum(); i++)
                {
                    if (!targetLabels.Contains(GetTrainLabel(i)))
                    {
                        // 学習対象外
                        continue;
                    }

                    float[][] vector = GetTrainVector(i);
                    float* pVector = null;

                    try
                    {
                        pVector = ConvertVector(vector);

                        if (Refine(modelNo, pVector, vector.GetLength(0), kValue, GetTrainLabel(i), i, addVectorThreshold))
                        {
                            // 再学習した
                            if (DateTime.Now > nextTime)
                            {
                                DisplayMessage($"Refined {count} images in this try({tryCount})");
                                nextTime = DateTime.Now.AddSeconds(10);
                            }

                            count++;
                        }
                    }
                    finally
                    {
                        if (pVector != null)
                        {
                            NativeMemory.Free(pVector);
                        }

                    }
                }

                if (count < GetTrainNum() * targetThreshold)
                {
                    // 再学習の数が少なくなったので終了
                    break;
                }

                tryCount += 1;
            }

            DisplayMessage($"Refine Finished. Total {tryCount} trys.");
        }


        public void RefineDatabaseForCurrentModel(int kValue, IEnumerable<int> targetLabels, double addVectorThreshold, double targetThreshold)
        {
            RefineDatabase(0, kValue, targetLabels, addVectorThreshold, targetThreshold);
        }

        private unsafe bool RefineBERTDatabase(int modelNo, float[][] vector, int kValue, int docNo, int sentenceNo, double addVectorThreshold)
        {
            float* pVector = null;

            try
            {
                pVector = ConvertVector(vector);

                return Refine(modelNo, pVector, vector.GetLength(0), kValue, docNo, sentenceNo, addVectorThreshold);
            }
            finally
            {
                if (pVector != null)
                {
                    NativeMemory.Free(pVector);
                }
            }
        }

        public bool RefineBERTDatabaseForCurrentModel(float[][] vector, int kValue, int docNo, int sentenceNo, double addVectorThreshold)
        {
            return RefineBERTDatabase(0, vector, kValue, docNo, sentenceNo, addVectorThreshold);
        }



        private unsafe (Dictionary<int, Dictionary<int, int>>, int, List<(int?, int, int, int, int, double)>) TestMain(int modelNo, int kValue, double detectThreshold, IEnumerable<int> targetLabels)
        {
            if (GetTrainVector == null)
            {
                throw new FSCLBugException("GetTrainVector is not set");
            }
            if (GetTrainLabel == null)
            {
                throw new FSCLBugException("GetTrainLabel is not set");
            }
            if (GetTrainNum == null)
            {
                throw new FSCLBugException("GetTrainNum is not set");
            }
            if (GetTestVector == null)
            {
                throw new FSCLBugException("GetTrainVector is not set");
            }
            if (GetTestLabel == null)
            {
                throw new FSCLBugException("GetTrainLabel is not set");
            }
            if (GetTestNum == null)
            {
                throw new FSCLBugException("GetTrainNum is not set");
            }

            Dictionary<int, Dictionary<int, int>> evaluateReslt = new();

            List<(int?, int, int, int, int, double)> vote = new();

            DateTime startTime = DateTime.Now;

            int count2 = 0;
            for (int i = 0; i < GetTestNum(); i++)
            {
                if (i % 500 == 0)
                {
                    DisplayMessage($"Evaluated {i} / {GetTestNum()} images");
                }

                float[][] vector = GetTestVector(i);

                float* pVector = null;
                PredictResult* result = null;

                try
                {
                    pVector = ConvertVector(vector);

                    result = Predict(modelNo, pVector, vector.GetLength(0), kValue, detectThreshold);

                    if (result == null)
                    {
                        DisplayMessage($"Prediction failed. pos={i}, label={GetTestLabel(i)}");
                        continue;
                    }

                    evaluateReslt.TryAdd(GetTestLabel(i), new Dictionary<int, int>());
                    evaluateReslt[GetTestLabel(i)].TryAdd(result->PredictedLabel, 0);
                    evaluateReslt[GetTestLabel(i)][result->PredictedLabel]++;

                    for (int pos = 0; pos < result->ScoreListMainAndSubNum; pos++)
                    {
                        ResultItemMainAndSub detail = result->ScoreListMainAndSub[pos];
                        vote.Add((result->PredictedLabel, GetTestLabel(i), i, detail.MainId, detail.SubId, detail.Score));
                    }

                    if (targetLabels.Contains(GetTestLabel(i)))
                    {
                        count2++;
                    }
                }
                finally
                {
                    if (pVector != null)
                    {
                        NativeMemory.Free(pVector);
                    }
                    if (result != null)
                    {
                        NativeMemory.Free(result);
                    }
                }
            }

            return (evaluateReslt, count2, vote);
        }


        /// 学習
        private unsafe void LearnAllLabels(int modelNo, int searchMax, double addVectorThreshold)
        {
            if (GetTrainVector == null)
            {
                throw new FSCLBugException("GetTrainVector is not set");
            }
            if (GetTrainLabel == null)
            {
                throw new FSCLBugException("GetTrainLabel is not set");
            }
            if (GetTrainNum == null)
            {
                throw new FSCLBugException("GetTrainNum is not set");
            }

            DateTime nextTime = DateTime.Now.AddSeconds(-1);

            for (int i = 0; i < GetTrainNum(); i++)
            {
                float[][] vector = GetTrainVector(i);

                float* pVector = null;

                try
                {
                    pVector = ConvertVector(vector);

                    Add(modelNo, pVector, vector.GetLength(0), GetTrainLabel(i), i, searchMax, addVectorThreshold);
                }
                finally
                {
                    if ( pVector != null)
                    {
                        NativeMemory.Free(pVector);
                    }
                }

                if (DateTime.Now > nextTime)
                {
                    DisplayMessage($"Learned {i + 1} / {GetTrainNum()} images");
                    nextTime = DateTime.Now.AddSeconds(10);
                }
            }

            DisplayMessage($"Finished!");
        }

        public void LearnAllLabelsForCurrentModel(int searchMax, double addVectorThreshold)
        {
            LearnAllLabels(0, searchMax, addVectorThreshold);
        }


        /// ランダム予測
        private unsafe void RandomPredict(int modelNo, int searchRange, double predictThreshold)
        {
            if (GetTrainVector == null)
            {
                throw new FSCLBugException("GetTrainVector is not set");
            }
            if (GetTrainLabel == null)
            {
                throw new FSCLBugException("GetTrainLabel is not set");
            }
            if (GetTrainNum == null)
            {
                throw new FSCLBugException("GetTrainNum is not set");
            }

            if (GetTestVector == null)
            {
                throw new FSCLBugException("GetTrainVector is not set");
            }
            if (GetTestLabel == null)
            {
                throw new FSCLBugException("GetTrainLabel is not set");
            }
            if (GetTestNum == null)
            {
                throw new FSCLBugException("GetTrainNum is not set");
            }

            float* pVector = null;
            PredictResult* result = null;

            try
            {
                int pos = new Random().Next(0, GetTestNum());

                float[][] vector = GetTestVector(pos);

                pVector = ConvertVector(vector);

                result = Predict(modelNo, pVector, vector.GetLength(0), searchRange, predictThreshold);

                if (result == null)
                {
                    DisplayMessage("Prediction failed");
                    return;
                }

                DisplayMessage($"Search Class : {GetTestLabel(pos)}");
                DisplayMessage($"Predicted Class : {result->PredictedLabel.ToString() ?? "unknown"}");

                List<ResultItemMain> sortedResult = new List<ResultItemMain>();
                for (int i = 0; i < result->ScoreListMainNum; i++)
                {
                    ResultItemMain add = new ResultItemMain();
                    add = result->ScoreListMain[i];
                    sortedResult.Add(add);
                }

                sortedResult = sortedResult.OrderByDescending(x => x.Score).ToList();
                for (int i = 0; i < sortedResult.Count; i++)
                {
                    DisplayMessage($"No.{i + 1} : Class {sortedResult[i].MainId}, Score {sortedResult[i].Score}");
                }
                DisplayMessage($"Key Image(pos={pos}, label={GetTestLabel(pos)})");
                DisplayTestImage?.Invoke(pos);

                List<ResultItemMainAndSub> sortedResult2 = new List<ResultItemMainAndSub>();
                for (int i = 0; i < result->ScoreListMainAndSubNum; i++)
                {
                    ResultItemMainAndSub add = new ResultItemMainAndSub();
                    add = result->ScoreListMainAndSub[i];
                    sortedResult2.Add(add);
                }
                ResultItemMainAndSub top = sortedResult2.OrderByDescending(x => x.Score).ToList().FirstOrDefault();

                DisplayMessage($"Predicted Image(pos ={top.SubId}, label={GetTrainLabel(top.SubId)}.{top.MainId}) : {top.Score.ToString("0.0000")}");
                DisplayTrainImage?.Invoke(top.SubId);
            }
            finally
            {
                if (pVector != null)
                {
                    NativeMemory.Free(pVector);
                }
                if (result != null)
                {
                    NativeMemory.Free(result);
                }
            }
        }

        public void RandomPredictForCurrentModel(int searchRange, double predictThreshold)
        {
            RandomPredict(0, searchRange, predictThreshold);
        }


        /// ランダム予測
        private unsafe void RandomPredictLimitedLabels(int modelNo, int searchRange, double predictThreshold, IEnumerable<int> targetLabels)
        {
            if (GetTrainVector == null)
            {
                throw new FSCLBugException("GetTrainVector is not set");
            }
            if (GetTrainLabel == null)
            {
                throw new FSCLBugException("GetTrainLabel is not set");
            }
            if (GetTrainNum == null)
            {
                throw new FSCLBugException("GetTrainNum is not set");
            }

            if (GetTestVector == null)
            {
                throw new FSCLBugException("GetTrainVector is not set");
            }
            if (GetTestLabel == null)
            {
                throw new FSCLBugException("GetTrainLabel is not set");
            }
            if (GetTestNum == null)
            {
                throw new FSCLBugException("GetTrainNum is not set");
            }

            float* pVector = null;
            PredictResult* result = null;

            try
            {
                List<int> targetPosList = new();
                for (int i = 0; i < GetTestNum(); i++)
                {
                    if (targetLabels.Contains(GetTestLabel(i)))
                    {
                        targetPosList.Add(i);
                    }
                }

                int pos = targetPosList[new Random().Next(0, targetPosList.Count)];

                //pos = 4;

                //DisplayImage(img, pos);

                float[][] vector = GetTestVector(pos);

                pVector = ConvertVector(vector);

                result = Predict(modelNo, pVector, vector.GetLength(0), searchRange, predictThreshold);

                if (result == null)
                {
                    DisplayMessage("Prediction failed");
                    return;
                }

                DisplayMessage($"Search Class : {GetTestLabel(pos)}");
                DisplayMessage($"Predicted Class : {result->PredictedLabel.ToString() ?? "unknown"}");

                List<ResultItemMain> sortedResult = new List<ResultItemMain>();
                for (int i = 0; i < result->ScoreListMainNum; i++)
                {
                    ResultItemMain add = new ResultItemMain();
                    add = result->ScoreListMain[i];
                    sortedResult.Add(add);
                }

                for (int i = 0; i < sortedResult.Count; i++)
                {
                    DisplayMessage($"No.{i + 1} : Class {sortedResult[i].MainId}, Score {sortedResult[i].Score}");
                }
                DisplayMessage($"Key Image(pos={pos}, label={GetTestLabel(pos)})");
                DisplayTestImage?.Invoke(pos);

                List<ResultItemMainAndSub> sortedResult2 = new List<ResultItemMainAndSub>();
                for (int i = 0; i < result->ScoreListMainAndSubNum; i++)
                {
                    ResultItemMainAndSub add = new ResultItemMainAndSub();
                    add = result->ScoreListMainAndSub[i];
                    sortedResult2.Add(add);
                }
                ResultItemMainAndSub top = sortedResult2.OrderByDescending(x => x.Score).ToList().FirstOrDefault();

                DisplayMessage($"Predicted Image(pos ={top.SubId}, label={GetTrainLabel(top.SubId)}.{top.MainId}) : {top.Score.ToString("0.0000")}");
                DisplayTrainImage?.Invoke(top.SubId);
            }
            finally
            {
                if (pVector != null)
                {
                    NativeMemory.Free(pVector);
                }
                if (result != null)
                {
                    NativeMemory.Free(result);
                }
            }
        }


        public void RandomPredictLimitedLabelsForCurrentModel(
            int searchRange, double predictThreshold, IEnumerable<int> targetLabels)
        {
            RandomPredictLimitedLabels(0, searchRange, predictThreshold, targetLabels);
        }


        private unsafe void EvaluateAllData(int modelNo, int kValue, double predictThreshold)
        {
            if (GetTrainVector == null)
            {
                throw new FSCLBugException("GetTrainVector is not set");
            }
            if (GetTrainLabel == null)
            {
                throw new FSCLBugException("GetTrainLabel is not set");
            }
            if (GetTrainNum == null)
            {
                throw new FSCLBugException("GetTrainNum is not set");
            }

            if (GetTestVector == null)
            {
                throw new FSCLBugException("GetTrainVector is not set");
            }
            if (GetTestLabel == null)
            {
                throw new FSCLBugException("GetTrainLabel is not set");
            }
            if (GetTestNum == null)
            {
                throw new FSCLBugException("GetTrainNum is not set");
            }

            Dictionary<int, Dictionary<int, int>> evaluateReslt = new();

            int[] targets = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };


            int count2 = 0;
            for (int i = 0; i < GetTestNum(); i++)
            {
                if (i % 500 == 0)
                {
                    DisplayMessage($"Evaluated {i} / {GetTestNum()} images");
                }

                float[][] vector = GetTestVector(i);

                float* pVector = null;
                PredictResult* result = null;

                try
                {
                    pVector = ConvertVector(vector);

                    result = Predict(modelNo, pVector, vector.GetLength(0), kValue, predictThreshold);

                    if (result == null)
                    {
                        DisplayMessage($"Prediction failed. pos={i}, label={GetTestLabel(i)}");
                        continue;
                    }
                    evaluateReslt.TryAdd(GetTestLabel(i), new Dictionary<int, int>());
                    evaluateReslt[GetTestLabel(i)].TryAdd(result->PredictedLabel, 0);
                    evaluateReslt[GetTestLabel(i)][result->PredictedLabel]++;

                    if (targets.Contains(GetTestLabel(i)))
                    {
                        count2++;
                    }
                }
                finally
                {
                    if (pVector != null)
                    {
                        NativeMemory.Free(pVector);
                    }
                    if (result != null)
                    {
                        NativeMemory.Free(result);
                    }
                }
            }

            int correct = 0;
            int labelErrorCount = 0;
            int unknownCount = 0;
            foreach (byte label in new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })
            {
                Console.Write($"Label {label}");
                for (int i = 0; i < 10; i++)
                {
                    if (evaluateReslt[label].ContainsKey(i))
                    {
                        Console.Write($" : {evaluateReslt[label][i]}");
                        if (i == label)
                        {
                            correct += evaluateReslt[label][i];
                        }
                        else
                        {
                            if (targets.Contains(label))
                            {
                                labelErrorCount += evaluateReslt[label][i];
                            }
                        }
                    }
                    else
                    {
                        Console.Write($" : 0");
                    }
                }
                if (evaluateReslt[label].ContainsKey(-1))
                {
                    DisplayMessage($" : {evaluateReslt[label][-1]}");
                    if (targets.Contains(label))
                    {
                        unknownCount += evaluateReslt[label][-1];
                    }
                }
                else
                {
                    DisplayMessage($" : {0}");
                }
            }

            DisplayMessage($"Accuracy : {(double)correct / (GetTestNum())} : {correct} / {GetTestNum()}");
            DisplayMessage($"Accuracy2 : {(double)correct / (count2)} : {correct} / {count2}");
            DisplayMessage($"Label Error(0-8) : {labelErrorCount}");
            DisplayMessage($"Unknown Data Count (0-8) : {unknownCount}");
            if (evaluateReslt.ContainsKey(9) && evaluateReslt[9].ContainsKey(-1))
            {
                DisplayMessage($"Error : {(double)evaluateReslt[9][-1] / evaluateReslt[9].Values.Sum()} : {evaluateReslt[9][-1]}/{evaluateReslt[9].Values.Sum()}");
            }
        }

        public void EvaluateAllDataForCurrentModel(int kValue, double predictThreshold)
        {
            EvaluateAllData(0, kValue, predictThreshold);
        }


        /// データベースの洗練
        unsafe void CheckScore(int modelNo, int kValue, double detectThreshold, int skipLabel, IEnumerable<int> targetLabels, IEnumerable<int> allLabels)
        {
            if (GetTrainVector == null)
            {
                throw new FSCLBugException("GetTrainVector is not set");
            }
            if (GetTrainLabel == null)
            {
                throw new FSCLBugException("GetTrainLabel is not set");
            }
            if (GetTrainNum == null)
            {
                throw new FSCLBugException("GetTrainNum is not set");
            }

            if (GetTestVector == null)
            {
                throw new FSCLBugException("GetTrainVector is not set");
            }
            if (GetTestLabel == null)
            {
                throw new FSCLBugException("GetTrainLabel is not set");
            }
            if (GetTestNum == null)
            {
                throw new FSCLBugException("GetTrainNum is not set");
            }

            DateTime startTime = DateTime.Now;

            (Dictionary<int, Dictionary<int, int>>, int, List<(int?, int, int, int, int, double)>) reslt = TestMain(modelNo, kValue, detectThreshold, targetLabels);

            Dictionary<int, Dictionary<int, int>> evaluateReslt = reslt.Item1;

            int count2 = reslt.Item2;

            double timeSec = (DateTime.Now - startTime).TotalMilliseconds;


            int correct = 0;
            int labelErrorCount = 0;
            int unknownCount = 0;

            DisplayMessage($"k, dbThreshold, detectThreshold, totalTime, predictTime, skipLabel, targetLabels");
            DisplayMessage($"{kValue},{detectThreshold.ToString("0.00")}, {detectThreshold},{timeSec.ToString("0.00")},{(timeSec / count2).ToString("0.00")},{skipLabel}," + string.Join("-", targetLabels.Select(a => a.ToString()).ToArray()));

            string line2 = $"{kValue},{detectThreshold.ToString("0.00")}, {detectThreshold},{timeSec.ToString("0.00")},{(timeSec / count2).ToString("0.00")},{skipLabel}," + string.Join("-", targetLabels.Select(a => a.ToString()).ToArray());

            DisplayMessage("label," + string.Join(",", allLabels.Select(a => a.ToString())) + ",unknown");

            foreach (byte label in allLabels)
            {
                string line = string.Empty;

                line += $"Label_{label}";
                Console.Write($"Label {label}");

                foreach (int i in allLabels)
                {
                    if (evaluateReslt[label].ContainsKey(i))
                    {
                        Console.Write($" : {evaluateReslt[label][i]}");
                        line += $",{evaluateReslt[label][i]}";
                        if (i == label)
                        {
                            correct += evaluateReslt[label][i];
                        }
                        else
                        {
                            if (targetLabels.Contains(label))
                            {
                                labelErrorCount += evaluateReslt[label][i];
                            }
                        }
                    }
                    else
                    {
                        Console.Write($" : 0");
                        line += $",{0}";
                    }
                }
                if (evaluateReslt[label].ContainsKey(-1))
                {
                    DisplayMessage($" : {evaluateReslt[label][-1]}");
                    line += $",{evaluateReslt[label][-1]}";
                    if (targetLabels.Contains(label))
                    {
                        unknownCount += evaluateReslt[label][-1];
                    }
                }
                else
                {
                    DisplayMessage($" : {0}");
                    line += $",{0}";
                }
            }

            DisplayMessage($"Accuracy, {(double)correct / (GetTestNum())} : {correct} / {GetTestNum()}");

            DisplayMessage($"Accuracy2, {(double)correct / (count2)} : {correct} / {count2}");

            DisplayMessage($"Label Error({string.Join("-", targetLabels.Select(a => a.ToString()))}), {labelErrorCount}");

            DisplayMessage($"Unknown Data Count ({string.Join("-", targetLabels.Select(a => a.ToString()))}), {unknownCount}");

            if (evaluateReslt.ContainsKey(skipLabel) && evaluateReslt[skipLabel].ContainsKey(-1))
            {
                DisplayMessage($"Error({skipLabel}), ErrorCount({skipLabel}), Total({skipLabel})");
                DisplayMessage($"{(double)evaluateReslt[skipLabel][-1] / evaluateReslt[skipLabel].Values.Sum()}, {evaluateReslt[skipLabel][-1]}, {evaluateReslt[skipLabel].Values.Sum()}");
            }

            DisplayMessage("");
        }



        public void CheckScoreForCurrentModel(int kValue, double detectThreshold, int skipLabel, IEnumerable<int> targetLabels, IEnumerable<int> allLabels)
        {
            CheckScore(0, kValue, detectThreshold, skipLabel, targetLabels, allLabels);
        }


        public void Save(int modelNo, string savePath, int? count = null)
        {
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(savePath);

            unsafe
            {
                fixed (byte* pText = utf8Bytes)
                {
                    if (Save(modelNo, pText, utf8Bytes.Length))
                    {
                        StatusDetailEnum status = GetStatusDetail();
                        DisplayMessage($"Save:true:{status}");
                    }
                    else
                    {
                        StatusDetailEnum status = GetStatusDetail();
                        DisplayMessage($"Save:falase:{status}");
                    }
                }
            }

        }

        private unsafe void AddVector(int modelNo, float[][] vector, int label, int docId, int searchMax, double addVectorThreshold)
        {
            float* pVector = null;

            try
            {
                pVector = ConvertVector(vector);

                Add(modelNo, pVector, vector.GetLength(0), label, docId, searchMax, addVectorThreshold);
            }
            finally
            {
                if (pVector != null)
                {
                    NativeMemory.Free(pVector);
                }
            }
        }

        public void AddVectorForCurrentModel(float[][] vector, int label, int docId, int searchMax, double addVectorThreshold)
        {
            AddVector(0, vector, label, docId, searchMax, addVectorThreshold);
        }


        public unsafe SearchResultForManaged? Search(int modelNo, float[][] vector, int kValue)
        {
            float* pVector = null;
            SearchResult* result = null;

            try
            {
                pVector = ConvertVector(vector);

                result = Search(modelNo, pVector, vector.GetLength(0), kValue);

                if (result == null)
                {
                    return null;
                }

                SearchResultForManaged resultForManaged = new SearchResultForManaged();

                for(int i = 0; i < result->ResultItemMainNum; i++)
                {
                    ResultItemMain add = result->ResultItemMains[i];
                    resultForManaged.ResultItemMains.Add(add);
                }

                for (int i = 0; i < result->ResultItemMainAndSubNum; i++)
                {
                    ResultItemMainAndSub add = result->ResultItemMainAndSubs[i];
                    resultForManaged.ResultItemMainAndSubs.Add(add);
                }

                for (int i = 0; i < result->ResultItemMainDetailNum; i++)
                {
                    resultForManaged.ResultItemMainDetails.Add(result->ResultItemMainDetails[i].key, new());
                    for (int j = 0; j < result->ResultItemMainDetails[i].valueNum; j++)
                    {
                        ResultItemMain add2 = result->ResultItemMainDetails[i].values[j];
                        resultForManaged.ResultItemMainDetails[result->ResultItemMainDetails[i].key].Add(add2);
                    }
                }

                for (int i = 0; i < result->ResultItemMainAndSubDetailNum; i++)
                {
                    resultForManaged.ResultItemMainAndSubDetails.Add(result->ResultItemMainAndSubDetails[i].key, new());
                    for (int j = 0; j < result->ResultItemMainAndSubDetails[i].valueNum; j++)
                    {
                        ResultItemMainAndSub add2 = result->ResultItemMainAndSubDetails[i].values[j];
                        resultForManaged.ResultItemMainAndSubDetails[result->ResultItemMainAndSubDetails[i].key].Add(add2);
                    }
                }

                return resultForManaged;
            }
            finally
            {
                if (pVector != null)
                {
                    NativeMemory.Free(pVector);
                }
                if ( result != null)
                {
                    NativeMemory.Free(result);
                }
            }
        }

        public unsafe bool IsNeedRefine(int modelNo, float[][] vector, int searchMax, int docId, int subDocId, double threshold)
        {
            float* pVector = null;

            try
            {
                pVector = ConvertVector(vector);

                return IsNeedRefine(modelNo, pVector, vector.GetLength(0), searchMax, docId, subDocId);
            }
            finally
            {
                NativeMemory.Free(pVector);
            }



        }


        public double? GetTotalVectorCount(int modelNo)
        {
            return GetTotalVector(modelNo);
        }


        public unsafe float* ConvertVector(float[][] vector)
        {
            nuint vsize = (nuint)(vector.GetLength(0) * vector[0].GetLength(0) * sizeof(float));
            float* pVector = (float*)NativeMemory.Alloc(vsize);
            for (int v1 = 0; v1 < vector.GetLength(0); v1++)
            {
                for (int v2 = 0; v2 < vector[v1].GetLength(0); v2++)
                {
                    pVector[v1 * vector[v1].GetLength(0) + v2] = vector[v1][v2];
                }
            }

            return pVector;
        }


        public unsafe Dictionary<int, List<ResultItemMainAndSub>> Clustering(int modelNo)
        {
            SearchResult* result = SimpleClustering(modelNo);

            if ( result == null)
            {
                return new Dictionary<int, List<ResultItemMainAndSub>>();
            }

            Dictionary<int, List<ResultItemMainAndSub>> ret = new();
            for (int i = 0; i < result->ResultItemMainAndSubDetailNum; i++)
            {
                ret.Add(result->ResultItemMainAndSubDetails[i].key, new List<ResultItemMainAndSub>());
                for(int j = 0; j < result->ResultItemMainAndSubDetails[i].valueNum; j++)
                {
                    ResultItemMainAndSub add2 = result->ResultItemMainAndSubDetails[i].values[j];
                    ret[result->ResultItemMainAndSubDetails[i].key].Add(add2);
                }
            }

            return ret;
        }

        public bool RefineDatabaseForCurrentModel(int limit)
        {
            return RefineDatabase(0, limit);
        }




        public bool RefineDatabase(int instanceNo, int limit)
        {
            return RekNNUtility.RefineAll(instanceNo, limit);
        }


        /// <summary>
        /// 推論の実行
        /// </summary>
        /// <param name="modelNo"></param>
        /// <param name="searchRange"></param>
        /// <param name="predictThreshold"></param>
        /// <param name="data"></param>
        public unsafe ((int, double)?, List<ResultItemMainAndSub>) Predict(int modelNo, int searchRange, double predictThreshold, float[][] vector)
        {
            float* pVector = null;
            PredictResult* result = null;

            try
            {
                pVector = ConvertVector(vector);

                result = Predict(modelNo, pVector, vector.GetLength(0), searchRange, predictThreshold);

                if (result == null)
                {
                    DisplayMessage("Prediction failed");
                    return (null, new List<ResultItemMainAndSub>());
                }

                DisplayMessage($"Predicted Class : {result->PredictedLabel.ToString() ?? "unknown"}");

                List<ResultItemMain> sortedResult = new List<ResultItemMain>();
                for (int i = 0; i < result->ScoreListMainNum; i++)
                {
                    ResultItemMain add = new ResultItemMain();
                    add = result->ScoreListMain[i];
                    sortedResult.Add(add);
                }
                sortedResult = sortedResult.OrderByDescending(x => x.Score).ToList();

                List<ResultItemMainAndSub> sortedResult2 = new List<ResultItemMainAndSub>();
                for (int i = 0; i < result->ScoreListMainAndSubNum; i++)
                {
                    ResultItemMainAndSub add = new ResultItemMainAndSub();
                    add = result->ScoreListMainAndSub[i];
                    sortedResult2.Add(add);
                }

                Dictionary<int, double> classScore = new Dictionary<int, double>();
                foreach (ResultItemMainAndSub elm in sortedResult2)
                {
                    classScore.TryAdd(elm.MainId, 0);
                    classScore[elm.MainId] += elm.Score;
                }

                (int, double)? pred = null;
                if ( classScore.Count> 0)
                {
                    KeyValuePair<int, double> max = classScore.OrderByDescending(x => x.Value).FirstOrDefault();
                    if (max.Value >= predictThreshold)
                    {
                        pred = (max.Key, max.Value);
                    }
                }

                DisplayMessage($"Predicted Image(label={pred?.Item1.ToString() ?? "Unknown"}) : Score = {pred?.Item2.ToString("0.0000") ?? "null"})");

                return (pred, sortedResult2.OrderByDescending(x=>x.Score).ToList());
            }
            finally
            {
                if (pVector != null)
                {
                    NativeMemory.Free(pVector);
                }
                if (result != null)
                {
                    NativeMemory.Free(result);
                }
            }
        }

        /// <summary>
        /// メッセージ表示モードの変更
        /// </summary>
        /// <param name="mode"></param>
        public void SetDebugMode(DebugModeEnum mode)
        {
            int m = (int)mode;
            SetDebugMode(m);
        }
    }
}

