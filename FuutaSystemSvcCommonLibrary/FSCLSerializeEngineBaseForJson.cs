using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    public abstract class FSCLSerializeEngineBaseForJson
    {
        /// <summary>
        /// メッセージの表示関数
        /// </summary>
        public Action<string>? DisplayMessageSingleLine { get; set; } = null;

        /// <summary>
        /// メッセージの表示関数
        /// </summary>
        public Action<List<string>>? DisplayMessageMultiLine { get; set; } = null;

        /// <summary>
        /// フィルター関数(暗号化用)
        /// </summary>
        public Func<byte[], byte[]>? FilterFunc { get; set; } = null;

        /// <summary>
        /// フィルター関数(復号化用)
        /// </summary>
        public Func<byte[], byte[]>? RevFilterFunc { get; set; } = null;

        /// <summary>
        /// メッセージの表示関数
        /// </summary>
        /// <param name="message"></param>
        public void DisplayMessage(string message)
        {
            DisplayMessageSingleLine?.Invoke(message);
        }

        /// <summary>
        /// メッセージの表示関数
        /// </summary>
        /// <param name="message"></param>
        public void DisplayMessage(List<string> messageList)
        {
            DisplayMessageMultiLine?.Invoke(messageList);
        }

        /// <summary>
        /// シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public byte[] toBytes(object data, Type dataType)
        //{
        //    using MemoryStream mst = new();

        //    Trace.WriteLine($"mst start");

        //    toStreamMain(mst, data, dataType);

        //    Trace.WriteLine($"mst size = {mst.Length}");

        //    return mst.ToArray();
        //}

        /// <summary>
        /// シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public byte[] toBytesWithFilter(object data, Type dataType)
        //{
        //    MemoryStream mst = new();
        //    try
        //    {
        //        toStreamMain(mst, data, dataType);
        //    }
        //    finally
        //    {
        //        mst.Close();
        //    }

        //    return FilterFunc?.Invoke(mst.ToArray()) ?? mst.ToArray();
        //}

        /// <summary>
        /// シリアル化する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] toBytes<T,C>(T data)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            using MemoryStream mst = new();

            toStreamMain<T,C>(mst, data);

            return mst.ToArray();
        }

        /// <summary>
        /// シリアル化する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] toBytesWithFilter<T,C>(T data)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            using MemoryStream mst = new();

            toStreamMain<T, C>(mst, data);

            return FilterFunc?.Invoke(mst.ToArray()) ?? mst.ToArray();
        }

        /// <summary>
        /// シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public void toStream(Stream ost, object data, Type dataType)
        //{
        //    // 暗号化のため、一旦 byte に変換する(暗号化もここでやる)
        //    byte[] dataArray = toBytes(data, dataType);

        //    ost.Write(dataArray, 0, dataArray.Length);
        //}

        /// <summary>
        /// シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public void toStreamWithFilter(Stream ost, object data, Type dataType)
        //{
        //    // 暗号化のため、一旦 byte に変換する(暗号化もここでやる)
        //    try
        //    {
        //        byte[] dataArray = toBytesWithFilter(data, dataType);

        //        ost.Write(dataArray, 0, dataArray.Length);
        //    }
        //    finally
        //    {
        //        ost.Close();
        //    }
        //}

        /// <summary>
        /// シリアル化する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ost"></param>
        /// <param name="data"></param>
        public void toStream<T,C>(Stream ost, T data)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            // さしあたり option は特に指示せずデフォルトのまま
            try
            {
                byte[] dataArray = toBytes<T,C>(data);

                ost.Write(dataArray, 0, dataArray.Length);
            }
            finally
            {
                ost.Close();
            }
        }

        /// <summary>
        /// シリアル化する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ost"></param>
        /// <param name="data"></param>
        public void toStreamWithFilter<T, C>(Stream ost, T data)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            // さしあたり option は特に指示せずデフォルトのまま
            try
            {
                byte[] dataArray = toBytesWithFilter<T, C>(data);

                ost.Write(dataArray, 0, dataArray.Length);
            }
            finally
            {
                ost.Close();
            }
        }


        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public object? fromBytes(byte[] data, Type dataType)
        //{
        //    object? ret = null;
        //    // フィルタ関数があるならそれを経由
        //    MemoryStream mst = new(data);
        //    try
        //    {
        //        lock (this)
        //        {
        //            ret = fromStreamMain(mst, dataType);
        //        }
        //    }
        //    finally
        //    {
        //        mst.Close();
        //    }

        //    return ret;
        //}

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public object? fromBytesWithFilter(byte[] data, Type dataType)
        //{
        //    object? ret = null;
        //    // フィルタ関数があるならそれを経由
        //    MemoryStream mst = new(RevFilterFunc?.Invoke(data) ?? data);
        //    try
        //    {
        //        lock (this)
        //        {
        //            ret = fromStreamMain(mst, dataType);
        //        }
        //    }
        //    finally
        //    {
        //        mst.Close();
        //    }

        //    return ret;
        //}

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public T? fromBytes<T, C>(byte[] data)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            T? ret;
            MemoryStream mst = new(data);
            try
            {
                ret = fromStreamMain<T, C>(mst);
            }
            finally
            {
                mst.Close();
            }

            return ret;
        }

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public T? fromBytesWithFilter<T, C>(byte[] data)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            T? ret;
            //{
            //    MemoryStream mstA = new(RevFilterFunc?.Invoke(data) ?? data);
            //    StreamReader reader = new(mstA, Encoding.UTF8);
            //    while(reader.ReadLine() is string line)
            //    {
            //        Debug.WriteLine(line);
            //    }
            //}

            MemoryStream mst = new(RevFilterFunc?.Invoke(data) ?? data);
            try
            {
                ret = fromStreamMain<T, C>(mst);
            }
            finally
            {
                mst.Close();
            }

            return ret;
        }

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public object? fromStream(Stream ist, Type dataType)
        //{
        //    object? result = null;

        //    try
        //    {
        //        // まず、ist を byte 列に変換する
        //        byte[] data = new byte[ist.Length];
        //        int pos = 0;
        //        int length = 0;
        //        try
        //        {
        //            length = Convert.ToInt32(ist.Length);
        //        }
        //        catch
        //        {
        //            length = 0;
        //        }

        //        while (pos < ist.Length)
        //        {
        //            pos += ist.Read(data, pos, length - pos);
        //        }

        //        result = fromBytes(data, dataType);
        //    }
        //    catch (Exception err)
        //    {
        //        DisplayMessage(err.Message);
        //        if (err.StackTrace != null)
        //        {
        //            DisplayMessage(err.StackTrace);
        //        }
        //        result = null;
        //    }
        //    finally
        //    {
        //        ist.Close();
        //    }


        //    return result;
        //}

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public object? fromStreamWithFilter(Stream ist, Type dataType)
        //{
        //    object? result = null;

        //    try
        //    {
        //        // まず、ist を byte 列に変換する
        //        byte[] data = new byte[ist.Length];
        //        int pos = 0;
        //        int length = 0;
        //        try
        //        {
        //            length = Convert.ToInt32(ist.Length);
        //        }
        //        catch
        //        {
        //            length = 0;
        //        }

        //        while (pos < ist.Length)
        //        {
        //            pos += ist.Read(data, pos, length - pos);
        //        }

        //        result = fromBytesWithFilter(data, dataType);
        //    }
        //    catch (Exception err)
        //    {
        //        DisplayMessage(err.Message);
        //        if (err.StackTrace != null)
        //        {
        //            DisplayMessage(err.StackTrace);
        //        }
        //        result = null;
        //    }
        //    finally
        //    {
        //        ist.Close();
        //    }


        //    return result;
        //}

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ist"></param>
        /// <returns></returns>
        public T? fromStream<T,C>(Stream ist)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            T? result;

            try
            {
                // まず、ist を byte 列に変換する
                byte[] data = new byte[ist.Length];
                int pos = 0;
                int length = 0;
                try
                {
                    length = Convert.ToInt32(ist.Length);
                }
                catch
                {
                    length = 0;
                }

                while (pos < ist.Length)
                {
                    pos += ist.Read(data, pos, length - pos);
                }

                result = fromBytes<T, C>(data);
            }
            catch (Exception err)
            {
                DisplayMessage(err.Message);
                if (err.StackTrace != null)
                {
                    DisplayMessage(err.StackTrace);
                }
                result = default(T);
            }
            finally
            {
                ist.Close();
            }


            return result;
        }

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ist"></param>
        /// <returns></returns>
        public T? fromStreamWithFilter<T,C>(Stream ist)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            T? result;

            try
            {
                // まず、ist を byte 列に変換する
                byte[] data = new byte[ist.Length];
                int pos = 0;
                int length = 0;
                try
                {
                    length = Convert.ToInt32(ist.Length);
                }
                catch
                {
                    length = 0;
                }

                while (pos < ist.Length)
                {
                    pos += ist.Read(data, pos, length - pos);
                }

                result = fromBytesWithFilter<T, C>(data);
            }
            catch (Exception err)
            {
                DisplayMessage(err.Message);
                if (err.StackTrace != null)
                {
                    DisplayMessage(err.StackTrace);
                }
                result = default(T);
            }
            finally
            {
                ist.Close();
            }


            return result;
        }


        /// <summary>
        /// ファイルに出力する
        /// </summary>
        /// <param name="fname"></param>
        //public void toFile(string fname, object obj, Type dataType)
        //{
        //    string? dirname = Path.GetDirectoryName(fname);

        //    if (dirname != null)
        //    {
        //        DirectoryInfo dinfo = new DirectoryInfo(dirname);
        //        if (!dinfo.Exists)
        //        {
        //            Trace.WriteLine($"Create Folder : {dirname}");
        //            dinfo.Create();
        //        }
        //        else
        //        {
        //            Trace.WriteLine($"Folder Exists : {dirname}");
        //        }
        //    }

        //    // たまに書き込みに失敗するのでリトライ処理を追加
        //    for ( int i = 0; i < 10; i++)
        //    {
        //        try
        //        {
        //            FileInfo finfo = new FileInfo(fname);
        //            using FileStream stream = finfo.Open(FileMode.Create, FileAccess.Write, FileShare.None);
        //            toStream(stream, obj, dataType);

        //            //Console.WriteLine($"{this.GetType().Name}:toFile Finished!");

        //            break;
        //        }
        //        catch (Exception err)
        //        {
        //            Trace.WriteLine($"{this.GetType().Name}:toFile Error / Retry (fname={fname}, type={dataType.ToString()})");
        //            Trace.WriteLine($"{err.Message}");
        //            Trace.WriteLine($"{err.StackTrace}");

        //            // 怖いのでちょっと待つ
        //            Thread.Sleep(10);
        //        }
        //    }
        //}

        /// <summary>
        /// ファイルに出力する
        /// </summary>
        /// <param name="fname"></param>
        //public void toFileWithFilter(string fname, object obj, Type dataType)
        //{
        //    string? dirname = Path.GetDirectoryName(fname);

        //    if (dirname != null)
        //    {
        //        DirectoryInfo dinfo = new DirectoryInfo(dirname);
        //        if (!dinfo.Exists)
        //        {
        //            //Console.WriteLine($"Create Folder : {dirname}");
        //            dinfo.Create();
        //        }
        //        else
        //        {
        //            //Console.WriteLine($"Folder Exists : {dirname}");
        //        }
        //    }

        //    FileStream? stream = null;

        //    // たまに書き込みに失敗するのでリトライ処理を追加
        //    for (int i = 0; i < 10; i++)
        //    {
        //        try
        //        {
        //            FileInfo finfo = new FileInfo(fname);
        //            stream = finfo.Create();
        //            toStreamWithFilter(stream, obj, dataType);

        //            //Debug.WriteLine($"{this.GetType().Name}:toFileWithFilter Save end");

        //            break;
        //        }
        //        catch (Exception err)
        //        {
        //            Console.Write($"{this.GetType().Name}:toFileWithFilter Error / Retry (fname={fname}, type={dataType.ToString()})");
        //            Console.Write($"{err.Message}");
        //            Console.Write($"{err.StackTrace}");

        //            // 怖いのでちょっと待つ
        //            Thread.Sleep(10);
        //        }
        //        finally
        //        {
        //            stream?.Close();
        //        }
        //    }
        //}

        /// <summary>
        /// 複数ファイルに出力する
        /// </summary>
        /// <param name="fnameList"></param>
        /// <param name="dataType"></param>
        public void toFile<T, C>(List<string> fnameList, T obj)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            foreach (string fname in fnameList)
            {
                toFile<T,C>(fname, obj);
            }
        }

        /// <summary>
        /// 複数ファイルに出力する
        /// </summary>
        /// <param name="fnameList"></param>
        /// <param name="dataType"></param>
        //public void toFileWithFilter(List<string> fnameList, object obj, Type dataType)
        //{
        //    foreach (string fname in fnameList)
        //    {
        //        toFileWithFilter(fname, obj, dataType);
        //    }
        //}

        /// <summary>
        /// ファイルを保存(ヒストリ,サブファイル対応)
        /// </summary>
        /// <param name="fname">ファイル名</param>
        /// <param name="history">履歴ファイル数</param>
        /// <param name="createSubFileFlag">予備ファイルを作るならtrue</param>
        public void toFileWithHistory<T, C>(string fname, int history, bool createSubFileFlag, T obj)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            // ヒストリファイルの更新
            if (history > 0)
            {
                // まず、最終ファイルを削除する
                try
                {
                    string fname1 = $"{fname}.{(history - 1).ToString("00")}";
                    if (File.Exists(fname1))
                    {
                        File.Delete(fname1);
                    }
                }
                catch (Exception err)
                {
                    DisplayMessage(err.Message);
                    if (err.StackTrace != null)
                    {
                        DisplayMessage(err.StackTrace);
                    }
                }

                // ヒストリを更新する
                if (history >= 2)
                {
                    for (int i = history - 2; i >= 0; i--)
                    {
                        string fname1 = $"{fname}.{i.ToString("00")}";
                        string fname2 = $"{fname}.{(i + 1).ToString("00")}";


                        if (File.Exists(fname1))
                        {
                            //if (File.Exists(fname2))
                            //{
                            //    File.Delete(fname2);
                            //}

                            int count = 0;
                            while (true)
                            {
                                try
                                {
                                    File.Move(fname1, fname2, true);
                                    break;
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine($"move error : {fname1} → {fname2}");
                                    count++;
                                    if (count < 10)
                                    {
                                        System.Threading.Thread.Sleep(1000);
                                        continue;
                                    }
                                    else
                                    {
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                }

                // 最新ファイルを保存
                {
                    string fname1 = $"{fname}.00";

                    if (File.Exists(fname))
                    {
                        //if (File.Exists(fname1))
                        //{
                        //    File.Delete(fname1);
                        //}

                        int count = 0;
                        while (true)
                        {
                            try
                            {
                                File.Move(fname, fname1, true);
                                break;
                            }
                            catch (Exception)
                            {
                                //Console.WriteLine($"move error : {fname} → {fname1}");
                                count++;
                                if (count < 10)
                                {
                                    System.Threading.Thread.Sleep(1000);
                                    continue;
                                }
                                else
                                {
                                    throw;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // 古いファイルを消す(ヒストリ無いからね…)
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        string fname1 = $"{fname}.{i.ToString("00")}";
                        if (File.Exists(fname1))
                        {
                            File.Delete(fname1);
                        }
                    }
                    catch (Exception err)
                    {
                        DisplayMessage(err.Message);
                        if (err.StackTrace != null)
                        {
                            DisplayMessage(err.StackTrace);
                        }
                    }
                }
            }

            // ファイルの保存
            toFile<T,C>(fname, obj);

            // サブファイル対応
            if (createSubFileFlag)
            {
                string fname1 = GetSubFileName(fname);
                toFile<T, C>(fname1, obj);
            }
        }

        /// <summary>
        /// ファイルを保存(ヒストリ,サブファイル対応)
        /// </summary>
        /// <param name="fname">ファイル名</param>
        /// <param name="history">履歴ファイル数</param>
        /// <param name="createSubFileFlag">予備ファイルを作るならtrue</param>
        public void toFileWithFilterWithHistory<T, C>(string fname, int history, bool createSubFileFlag, T obj)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            // ヒストリファイルの更新
            if (history > 0)
            {
                // まず、最終ファイルを削除する
                try
                {
                    string fname1 = $"{fname}.{(history - 1).ToString("00")}";
                    if (File.Exists(fname1))
                    {
                        File.Delete(fname1);
                    }
                }
                catch (Exception err)
                {
                    DisplayMessage(err.Message);
                    if (err.StackTrace != null)
                    {
                        DisplayMessage(err.StackTrace);
                    }
                }

                // ヒストリを更新する
                if (history >= 2)
                {
                    for (int i = history - 2; i >= 0; i--)
                    {
                        string fname1 = $"{fname}.{i.ToString("00")}";
                        string fname2 = $"{fname}.{(i + 1).ToString("00")}";


                        if (File.Exists(fname1))
                        {
                            //if (File.Exists(fname2))
                            //{
                            //    File.Delete(fname2);
                            //}

                            int count = 0;
                            while (true)
                            {
                                try
                                {
                                    File.Move(fname1, fname2, true);
                                    break;
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine($"move error : {fname1} → {fname2}");
                                    count++;
                                    if (count < 10)
                                    {
                                        System.Threading.Thread.Sleep(1000);
                                        continue;
                                    }
                                    else
                                    {
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                }

                // 最新ファイルを保存
                {
                    string fname1 = $"{fname}.00";

                    if (File.Exists(fname))
                    {
                        //if (File.Exists(fname1))
                        //{
                        //    File.Delete(fname1);
                        //}

                        int count = 0;
                        while (true)
                        {
                            try
                            {
                                File.Move(fname, fname1, true);
                                break;
                            }
                            catch (Exception)
                            {
                                //Console.WriteLine($"move error : {fname} → {fname1}");
                                count++;
                                if (count < 10)
                                {
                                    System.Threading.Thread.Sleep(1000);
                                    continue;
                                }
                                else
                                {
                                    throw;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // 古いファイルを消す(ヒストリ無いからね…)
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        string fname1 = $"{fname}.{i.ToString("00")}";
                        if (File.Exists(fname1))
                        {
                            File.Delete(fname1);
                        }
                    }
                    catch (Exception err)
                    {
                        DisplayMessage(err.Message);
                        if (err.StackTrace != null)
                        {
                            DisplayMessage(err.StackTrace);
                        }
                    }
                }
            }

            // ファイルの保存
            toFileWithFilter<T, C>(fname, obj);

            // サブファイル対応
            if (createSubFileFlag)
            {
                string fname1 = GetSubFileName(fname);
                toFileWithFilter<T, C>(fname1, obj);
            }
        }

        /// <summary>
        /// ファイルに出力
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fname"></param>
        /// <param name="obj"></param>
        public void toFile<T, C>(string fname, T obj)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            string? dirname = Path.GetDirectoryName(fname);

            if (dirname != null)
            {
                DirectoryInfo dinfo = new DirectoryInfo(dirname);
                if (!dinfo.Exists)
                {
                    dinfo.Create();
                }
            }

            FileStream? stream = null;

            try
            {
                byte[] bytes = toBytes<T,C>(obj);

                FileInfo finfo = new FileInfo(fname);
                stream = finfo.Create();
                stream.Write(bytes, 0, bytes.Length);
            }
            finally
            {
                stream?.Close();
            }
        }

        /// <summary>
        /// ファイルに出力する
        /// </summary>
        /// <param name="fname"></param>
        public void toFileWithFilter<T, C>(string fname, T obj)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            string? dirname = Path.GetDirectoryName(fname);

            if (dirname != null)
            {
                DirectoryInfo dinfo = new DirectoryInfo(dirname);
                if (!dinfo.Exists)
                {
                    dinfo.Create();
                }
            }

            FileStream? stream = null;

            try
            {
                byte[] bytes = toBytesWithFilter<T, C>(obj);

                FileInfo finfo = new FileInfo(fname);
                stream = finfo.Create();
                stream.Write(bytes, 0, bytes.Length);
            }
            finally
            {
                stream?.Close();
            }
        }


        /// <summary>
        /// Fileからインスタンスを生成する
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public object? fromFile(string fname, Type dataType)
        //{
        //    object? ret = null;
        //    FileStream? stream = null;

        //    try
        //    {
        //        FileInfo finfo = new FileInfo(fname);
        //        if (finfo.Exists)
        //        {
        //            byte[] data = new byte[finfo.Length];
        //            stream = finfo.OpenRead();
        //            int pos = 0;
        //            while (pos < data.Length)
        //            {
        //                pos += stream.Read(data, pos, data.Length - pos);
        //            }

        //            ret = fromBytes(data, dataType);
        //        }
        //    }
        //    finally
        //    {
        //        stream?.Close();
        //    }

        //    return ret;
        //}

        /// <summary>
        /// Fileからインスタンスを生成する
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public T? fromFile<T,C>(string fname)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            T? ret = default(T);
            FileStream? stream = null;

            try
            {
                FileInfo finfo = new FileInfo(fname);
                if (finfo.Exists)
                {
                    byte[] data = new byte[finfo.Length];
                    stream = finfo.OpenRead();
                    int pos = 0;
                    while (pos < data.Length)
                    {
                        pos += stream.Read(data, pos, data.Length - pos);
                    }

                    ret = fromBytes<T, C>(data);
                }
            }
            finally
            {
                stream?.Close();
            }

            return ret;
        }

        /// <summary>
        /// Fileからインスタンスを生成する
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public object? fromFileWithFilter(string fname, Type dataType)
        //{
        //    object? ret = null;
        //    FileStream? stream = null;

        //    try
        //    {
        //        FileInfo finfo = new FileInfo(fname);
        //        if (finfo.Exists)
        //        {
        //            byte[] data = new byte[finfo.Length];
        //            stream = finfo.OpenRead();
        //            int pos = 0;
        //            while (pos < data.Length)
        //            {
        //                pos += stream.Read(data, pos, data.Length - pos);
        //            }

        //            ret = fromBytesWithFilter(data, dataType);
        //        }
        //    }
        //    finally
        //    {
        //        stream?.Close();
        //    }

        //    return ret;
        //}

        /// <summary>
        /// Fileからインスタンスを生成する
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public T? fromFileWithFilter<T,C>(string fname)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            T? ret = default(T);
            FileStream? stream = null;

            try
            {
                FileInfo finfo = new FileInfo(fname);
                if (finfo.Exists)
                {
                    byte[] data = new byte[finfo.Length];
                    stream = finfo.OpenRead();
                    int pos = 0;
                    while (pos < data.Length)
                    {
                        pos += stream.Read(data, pos, data.Length - pos);
                    }

                    ret = fromBytesWithFilter<T,C>(data);
                }
            }
            finally
            {
                stream?.Close();
            }

            return ret;
        }


        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        //public object? fromFileSupportSub(string fname, Type classType)
        //{
        //    object? ret = null;

        //    try
        //    {
        //        ret = fromFile(fname, classType);

        //        if (ret == null) throw new FSCLBreakException();
        //    }
        //    catch (Exception)
        //    {
        //        string fname1 = GetSubFileName(fname);
        //        try
        //        {
        //            ret = fromFile(fname1, classType);
        //        }
        //        catch (Exception)
        //        {
        //            ret = null;
        //        }
        //    }

        //    return ret;
        //}

        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        public T? fromFileSupportSub<T, C>(string fname)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            T? ret = null;

            try
            {
                ret = fromFile<T, C>(fname);

                if (ret == null) throw new FSCLBreakException();
            }
            catch (Exception)
            {
                string fname1 = GetSubFileName(fname);
                try
                {
                    ret = fromFile<T,C>(fname1);
                }
                catch (Exception)
                {
                    ret = null;
                }
            }

            return ret;
        }

        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        //public object? fromFileWithFilterSupportSub(string fname, Type classType)
        //{
        //    object? ret = null;

        //    try
        //    {
        //        ret = fromFileWithFilter(fname, classType);

        //        if (ret == null) throw new FSCLBreakException();
        //    }
        //    catch (Exception)
        //    {
        //        string fname1 = GetSubFileName(fname);
        //        try
        //        {
        //            ret = fromFileWithFilter(fname1, classType);
        //        }
        //        catch (Exception)
        //        {
        //            ret = null;
        //        }
        //    }

        //    return ret;
        //}

        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        public T? fromFileWithFilterSupportSub<T,C>(string fname)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            T? ret = null;

            try
            {
                ret = fromFileWithFilter<T,C>(fname);

                if (ret == null) throw new FSCLBreakException();
            }
            catch (Exception)
            {
                string fname1 = GetSubFileName(fname);
                try
                {
                    ret = fromFileWithFilter<T,C>(fname1);
                }
                catch (Exception)
                {
                    ret = null;
                }
            }

            return ret;
        }

        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        //public object? fromFileSupportHistory(string fname, int history, Type classType)
        //{
        //    object? ret = null;

        //    try
        //    {
        //        ret = fromFileSupportSub(fname, classType);
        //    }
        //    catch (Exception)
        //    {
        //        ret = null;
        //    }

        //    if (ret == null)
        //    {
        //        int count = 0;
        //        while (true)
        //        {
        //            if (count >= history) break;

        //            string fname1 = GetHistoryFileName(fname, count);

        //            try
        //            {
        //                ret = fromFile(fname1, classType);
        //            }
        //            catch (Exception)
        //            {
        //                ret = null;
        //            }
        //            count++;

        //            if (ret != null) break;
        //        }
        //    }

        //    return ret;
        //}

        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        //public object? fromFileWithFilterSupportHistory(string fname, int history, Type classType)
        //{
        //    object? ret = null;

        //    try
        //    {
        //        ret = fromFileWithFilterSupportSub(fname, classType);
        //    }
        //    catch (Exception)
        //    {
        //        ret = null;
        //    }

        //    if (ret == null)
        //    {
        //        int count = 0;
        //        while (true)
        //        {
        //            if (count >= history) break;

        //            string fname1 = GetHistoryFileName(fname, count);

        //            try
        //            {
        //                ret = fromFile(fname1, classType);
        //            }
        //            catch (Exception)
        //            {
        //                ret = null;
        //            }
        //            count++;

        //            if (ret != null) break;
        //        }
        //    }

        //    return ret;
        //}

        /// <summary>
        /// サブファイルの名前を獲得する
        /// </summary>
        /// <param name="fname"></param>
        /// <returns></returns>
        public string GetSubFileName(string fname)
        {
            string dirName = Path.GetDirectoryName(fname) ?? "";

            string fname1 = Path.Combine(
                dirName,
                $"{Path.GetFileNameWithoutExtension(fname)}-sub{Path.GetExtension(fname)}"
                );

            return fname1;
        }

        /// <summary>
        /// サブファイルの名前を獲得する
        /// </summary>
        /// <param name="fname"></param>
        /// <returns></returns>
        public string GetHistoryFileName(string fname, int history)
        {
            string fname1 = $"{fname}.{history.ToString("00")}";

            return fname1;
        }

        /// <summary>
        /// 実際にシリアル化するところは下位で実装
        /// </summary>
        /// <param name="ist"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //protected abstract object? fromStreamMain(Stream ist, Type dataType);

        /// <summary>
        /// 実際にシリアル化するところは下位で実装
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ist"></param>
        /// <returns></returns>
        protected abstract T? fromStreamMain<T, C>(Stream ist)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>;


        /// <summary>
        /// シリアル化するところは下位で実装
        /// </summary>
        /// <param name="ost"></param>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        //protected abstract void toStreamMain(Stream ost, object data, Type dataType);

        /// <summary>
        /// シリアル化するところは下位で実装
        /// </summary>
        /// <param name="ost"></param>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        protected abstract void toStreamMain<T, C>(Stream ost, T data)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>;
    }
}
