using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    /// Jsonシリアライズ用基底クラス
    /// </summary>
    public class FSCLJsonSerializeBase
    {
        /// <summary>
        /// シリアル化エンジン
        /// </summary>
        public static FSCLJsonSerializeEngine Serializer { get; }

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public static object? fromBytes(byte[] data, Type dataType)
        //{
        //    lock (Serializer)
        //    {
        //        Serializer.currentResolver = null;
        //        return Serializer.fromBytes(data, dataType);
        //    }
        //}

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public static object? fromBytesWithFilter(byte[] data, Type dataType)
        //{
        //    lock (Serializer)
        //    {
        //        Serializer.currentResolver = null;
        //        return Serializer.fromBytesWithFilter(data, dataType);
        //    }
        //}

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static T? fromBytes<T,C>(byte[] data)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            lock (Serializer)
            {
                Serializer.currentResolver = null;
                return Serializer.fromBytes<T,C>(data);
            }
        }

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static T? fromBytesWithFilter<T,C>(byte[] data)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            lock (Serializer)
            {
                Serializer.currentResolver = null;
                return Serializer.fromBytesWithFilter<T,C>(data);
            }
        }

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static T? fromBytes<T,C>(byte[] data, DefaultJsonTypeInfoResolver? resolver)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            lock (Serializer)
            {
                Serializer.currentResolver = resolver;
                return Serializer.fromBytes<T,C>(data);
            }
        }

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public static object? fromBytesWithFilter(byte[] data, Type dataType, DefaultJsonTypeInfoResolver? resolver)
        //{
        //    lock (Serializer)
        //    {
        //        Serializer.currentResolver = resolver;
        //        return Serializer.fromBytesWithFilter(data, dataType);
        //    }
        //}

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public static object? fromStream(Stream ist, Type dataType)
        //{
        //    lock (Serializer)
        //    {
        //        Serializer.currentResolver = null;
        //        return Serializer.fromStream(ist, dataType);
        //    }
        //}

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public static object? fromStreamWithFilter(Stream ist, Type dataType)
        //{
        //    lock (Serializer)
        //    {
        //        Serializer.currentResolver = null;
        //        return Serializer.fromStreamWithFilter(ist, dataType);
        //    }
        //}

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public static object? fromStream(Stream ist, Type dataType, DefaultJsonTypeInfoResolver? resolver)
        //{
        //    lock (Serializer)
        //    {
        //        Serializer.currentResolver = resolver;
        //        return Serializer.fromStream(ist, dataType);
        //    }
        //}

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public static object? fromStreamWithFilter(Stream ist, Type dataType, DefaultJsonTypeInfoResolver? resolver)
        //{
        //    lock (Serializer)
        //    {
        //        Serializer.currentResolver = resolver;
        //        return Serializer.fromStreamWithFilter(ist, dataType);
        //    }
        //}

        /// <summary>
        /// Fileからインスタンスを生成する
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public static object? fromFile(string fname, Type dataType)
        //{
        //    lock (Serializer)
        //    {
        //        Serializer.currentResolver = null;
        //        return Serializer.fromFile(fname, dataType);
        //    }
        //}

        /// <summary>
        /// Fileからインスタンスを生成する
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public static object? fromFileWithFilter(string fname, Type dataType)
        //{
        //    lock (Serializer)
        //    {
        //        Serializer.currentResolver = null;
        //        return Serializer.fromFileWithFilter(fname, dataType);
        //    }
        //}

        /// <summary>
        /// Fileからインスタンスを生成する
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public static object? fromFile(string fname, Type dataType, DefaultJsonTypeInfoResolver? resolver)
        //{
        //    lock (Serializer)
        //    {
        //        Serializer.currentResolver = resolver;
        //        return Serializer.fromFile(fname, dataType);
        //    }
        //}

        /// <summary>
        /// Fileからインスタンスを生成する
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public static object? fromFileWithFilter(string fname, Type dataType, DefaultJsonTypeInfoResolver? resolver)
        //{
        //    lock (Serializer)
        //    {
        //        Serializer.currentResolver = resolver;
        //        return Serializer.fromFileWithFilter(fname, dataType);
        //    }
        //}

        /// <summary>
        /// Fileからインスタンスを生成する
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static T? fromFile<T,C>(string fname)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            lock (Serializer)
            {
                Serializer.currentResolver = null;
                return Serializer.fromFile<T,C>(fname);
            }
        }

        /// <summary>
        /// Fileからインスタンスを生成する
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static T? fromFileWithFilter<T,C>(string fname)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            lock (Serializer)
            {
                Serializer.currentResolver = null;
                return Serializer.fromFileWithFilter<T,C>(fname);
            }
        }

        /// <summary>
        /// Fileからインスタンスを生成する
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static T? fromFile<T,C>(string fname, DefaultJsonTypeInfoResolver? resolver)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            lock (Serializer)
            {
                Serializer.currentResolver = resolver;
                return Serializer.fromFile<T,C>(fname);
            }
        }

        /// <summary>
        /// Fileからインスタンスを生成する
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static T? fromFileWithFilter<T,C>(string fname, DefaultJsonTypeInfoResolver? resolver)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            lock (Serializer)
            {
                Serializer.currentResolver = resolver;
                return Serializer.fromFileWithFilter<T,C>(fname);
            }
        }

        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        //public static object? fromFileSupportSub(string fname, Type dataType)
        //{
        //    lock (Serializer)
        //    {
        //        Serializer.currentResolver = null;
        //        return Serializer.fromFileSupportSub(fname, dataType);
        //    }
        //}

        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        public static T? fromFileSupportSub<T,C>(string fname)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            lock (Serializer)
            {
                Serializer.currentResolver = null;
                return Serializer.fromFileSupportSub<T,C>(fname);
            }
        }

        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        //public static object? fromFileWithFilterSupportSub(string fname, Type dataType)
        //{
        //    lock (Serializer)
        //    {
        //        Serializer.currentResolver = null;

        //        object? ret = null;

        //        try
        //        {
        //            ret = fromFileWithFilter(fname, dataType);

        //            if (ret == null) throw new FSCLBreakException();
        //        }
        //        catch (Exception)
        //        {
        //            string fname1 = Serializer.GetSubFileName(fname);
        //            try
        //            {
        //                ret = fromFileWithFilter(fname1, dataType);
        //            }
        //            catch (Exception)
        //            {
        //                ret = null;
        //            }
        //        }

        //        return ret;
        //    }
        //}

        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        public static T? fromFileWithFilterSupportSub<T,C>(string fname)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            lock (Serializer)
            {
                Serializer.currentResolver = null;

                T? ret = null;

                try
                {
                    ret = fromFileWithFilter<T,C>(fname);

                    if (ret == null) throw new FSCLBreakException();
                }
                catch (Exception)
                {
                    string fname1 = Serializer.GetSubFileName(fname);
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
        }

        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        public static T? fromFileSupportSub<T,C>(string fname, DefaultJsonTypeInfoResolver? resolver)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            lock (Serializer)
            {
                Serializer.currentResolver = resolver;
                return Serializer.fromFileSupportSub<T,C>(fname);
            }
        }

        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        public static T? fromFileWithFilterSupportSub<T,C>(string fname, DefaultJsonTypeInfoResolver? resolver)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            lock (Serializer)
            {
                Serializer.currentResolver = resolver;

                T? ret = null;

                try
                {
                    ret = fromFileWithFilter<T,C>(fname, resolver);

                    if (ret == null) throw new FSCLBreakException();
                }
                catch (Exception)
                {
                    string fname1 = Serializer.GetSubFileName(fname);
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
        }

        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        //public object? fromFileSupportHistory(string fname, int history, Type classType)
        //{
        //    lock (Serializer)
        //    {
        //        Serializer.currentResolver = null;
        //        return Serializer.fromFileSupportHistory(fname, history, classType);
        //    }
        //}

        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        //public object? fromFileWithFilterSupportHistory(string fname, int history, Type classType)
        //{
        //    lock (Serializer)
        //    {
        //        Serializer.currentResolver = null;

        //        object? ret = null;

        //        try
        //        {
        //            ret = fromFileWithFilterSupportSub(fname, classType);
        //        }
        //        catch (Exception)
        //        {
        //            ret = null;
        //        }

        //        if (ret == null)
        //        {
        //            int count = 0;
        //            while (true)
        //            {
        //                if (count >= history) break;

        //                string fname1 = Serializer.GetHistoryFileName(fname, count);

        //                try
        //                {
        //                    ret = fromFileWithFilter(fname1, classType);
        //                }
        //                catch (Exception)
        //                {
        //                    ret = null;
        //                }
        //                count++;

        //                if (ret != null) break;
        //            }
        //        }

        //        return ret;
        //    }
        //}

        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        //public object? fromFileSupportHistory(string fname, int history, Type classType, DefaultJsonTypeInfoResolver? resolver)
        //{
        //    lock (Serializer)
        //    {
        //        Serializer.currentResolver = resolver;
        //        return Serializer.fromFileSupportHistory(fname, history, classType);
        //    }
        //}

        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        //public object? fromFileWithFilterSupportHistory(string fname, int history, Type classType, DefaultJsonTypeInfoResolver? resolver)
        //{
        //    lock (Serializer)
        //    {
        //        Serializer.currentResolver = resolver;

        //        object? ret = null;

        //        try
        //        {
        //            ret = fromFileWithFilterSupportSub(fname, classType, resolver);
        //        }
        //        catch (Exception)
        //        {
        //            ret = null;
        //        }

        //        if (ret == null)
        //        {
        //            int count = 0;
        //            while (true)
        //            {
        //                if (count >= history) break;

        //                string fname1 = Serializer.GetHistoryFileName(fname, count);

        //                try
        //                {
        //                    ret = fromFileWithFilter(fname1, classType, resolver);
        //                }
        //                catch (Exception)
        //                {
        //                    ret = null;
        //                }
        //                count++;

        //                if (ret != null) break;
        //            }
        //        }

        //        return ret;

        //    }
        //}

        /// <summary>
        /// Byte列に変換する
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public static byte[] toBytes(object obj, Type dataType)
        //{
        //    return Serializer.toBytes(obj, dataType);
        //}

        /// <summary>
        /// Byte列に変換する
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static byte[] toBytes<T,C>(T obj)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            return Serializer.toBytes<T,C>(obj);
        }

        /// <summary>
        /// Byte列に変換する
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public static byte[] toBytesWithFilter(object obj, Type dataType)
        //{
        //    return Serializer.toBytesWithFilter(obj, dataType);
        //}

        /// <summary>
        /// Byte列に変換する
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public byte[] toBytes<T,C>()
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            if ( this is T data)
            {
                return Serializer.toBytes<T,C>(data);
            }
            else
            {
                Debug.WriteLine("BAD Class Info!");
            }

            return new byte[0];
        }

        /// <summary>
        /// Byte列に変換する
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public byte[] toBytesWithFilter<T,C>()
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            if ( this is T data)
            {
                return Serializer.toBytesWithFilter<T,C>(data);
            }
            else
            {
                Debug.WriteLine("BAD Class Info!");
            }
            return new byte[0];
        }

        /// <summary>
        /// Byte列に変換する
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public static byte[] toBytesWithFilter(object obj, Type dataType, Func<byte[], byte[]> filterFunc)
        //{
        //    return Serializer.toBytesWithFilter(obj, dataType);
        //}

        /// <summary>
        /// Byte列に変換する
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public byte[] toBytesWithFilter(Type dataType, Func<byte[], byte[]> filterFunc)
        //{
        //    return Serializer.toBytesWithFilter(this, dataType);
        //}

        /// <summary>
        /// Streamに出力する
        /// </summary>
        /// <param name="ost"></param>
        /// <param name="obj"></param>
        /// <param name="dataType"></param>
        //public static void toStream(Stream ost, object obj, Type dataType)
        //{
        //    Serializer.toStream(ost, obj, dataType);
        //}

        /// <summary>
        /// Streamに出力する
        /// </summary>
        /// <param name="ost"></param>
        /// <param name="obj"></param>
        /// <param name="dataType"></param>
        //public static void toStreamWithFilter(Stream ost, object obj, Type dataType)
        //{
        //    Serializer.toStreamWithFilter(ost, obj, dataType);
        //}

        /// <summary>
        /// Streamに出力する
        /// </summary>
        /// <param name="ost"></param>
        /// <param name="obj"></param>
        /// <param name="dataType"></param>
        public static void toStream<T,C>(Stream ost, T obj)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            Serializer.toStream<T,C>(ost, obj);
        }

        /// <summary>
        /// Streamに出力する
        /// </summary>
        /// <param name="ost"></param>
        /// <param name="obj"></param>
        /// <param name="dataType"></param>
        public static void toStreamWithFilter<T,C>(Stream ost, T obj)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            Serializer.toStreamWithFilter<T,C>(ost, obj);
        }

        /// <summary>
        /// Streamに出力する
        /// </summary>
        /// <param name="ost"></param>
        /// <param name="obj"></param>
        /// <param name="dataType"></param>
        //public void toStream(Stream ost, Type dataType)
        //{
        //    Serializer.toStream(ost, this, dataType);
        //}

        /// <summary>
        /// Streamに出力する
        /// </summary>
        /// <param name="ost"></param>
        /// <param name="obj"></param>
        /// <param name="dataType"></param>
        //public void toStreamWithFilter(Stream ost, Type dataType)
        //{
        //    Serializer.toStreamWithFilter(ost, this, dataType);
        //}

        /// <summary>
        /// 文字列で読み込む
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public List<string> toStringList<T,C>()
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            List<string> ret = new();

            if ( this is T data)
            {
                MemoryStream mst = new();
                toStream<T,C>(mst, data);
                byte[] bytes = mst.GetBuffer();
                // nul 文字検索
                int length = 0;
                while (bytes[length] != 0)
                {
                    length += 1;

                    if (length >= bytes.Length) break;
                }
                byte[] conv = new byte[length];
                Array.Copy(bytes, conv, length);
                MemoryStream mst2 = new(conv);
                StreamReader reader = new(mst2);
                while (reader.ReadLine() is string line)
                {
                    ret.Add(line);
                }
            }
            else
            {
                Debug.WriteLine("BAD Class Info!");
            }

            return ret;
        }

        /// <summary>
        /// ファイルに出力する
        /// </summary>
        /// <param name="fname"></param>
        //public static void toFile(string fname, object obj, Type dataType)
        //{
        //    Serializer.toFile(fname, obj, dataType);
        //}

        /// <summary>
        /// ファイルに出力する
        /// </summary>
        /// <param name="fname"></param>
        //public static void toFileWithFilter(string fname, object obj, Type dataType)
        //{
        //    Serializer.toFileWithFilter(fname, obj, dataType);
        //}

        /// <summary>
        /// ファイルに出力する
        /// </summary>
        /// <param name="fname"></param>
        public static void toFile<T,C>(string fname, T obj)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            Serializer.toFile<T,C>(fname, obj);
        }

        /// <summary>
        /// ファイルに出力する
        /// </summary>
        /// <param name="fname"></param>
        public static void toFileWithFilter<T,C>(string fname, T obj)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            Serializer.toFileWithFilter<T,C>(fname, obj);
        }

        /// <summary>
        /// ファイルに出力する
        /// </summary>
        /// <param name="fname"></param>
        public void toFile<T,C>(string fname)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            if ( this is T data)
            {
                Serializer.toFile<T,C>(fname, data);
            }
            else
            {
                Debug.WriteLine("BAD Class Info!");
            }
        }

        /// <summary>
        /// ファイルに出力する
        /// </summary>
        /// <param name="fname"></param>
        public void toFileWithFilter<T,C>(string fname)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            if ( this is T data)
            {
                Serializer.toFileWithFilter<T,C>(fname, data);
            }
            else
            {
                Debug.WriteLine("BAD Class Info!");
            }
        }

        /// <summary>
        /// 複数ファイルに出力する
        /// </summary>
        /// <param name="fnameList"></param>
        /// <param name="dataType"></param>
        public static void toFile<T,C>(List<string> fnameList, T obj)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            Serializer.toFile<T,C>(fnameList, obj);
        }

        /// <summary>
        /// 複数ファイルに出力する
        /// </summary>
        /// <param name="fnameList"></param>
        /// <param name="dataType"></param>
        //public static void toFileWithFilter(List<string> fnameList, object obj, Type dataType)
        //{
        //    Serializer.toFileWithFilter(fnameList, obj, dataType);
        //}

        /// <summary>
        /// 複数ファイルに出力する
        /// </summary>
        /// <param name="fnameList"></param>
        /// <param name="dataType"></param>
        public void toFile<T,C>(List<string> fnameList)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            if ( this is T data)
            {
                Serializer.toFile<T,C>(fnameList, data);
            }
            else
            {
                Debug.WriteLine("BAD Class Info!");
            }
        }

        /// <summary>
        /// ファイルを保存(ヒストリ,サブファイル対応)
        /// </summary>
        /// <param name="fname">ファイル名</param>
        /// <param name="history">履歴ファイル数</param>
        /// <param name="createSubFileFlag">予備ファイルを作るならtrue</param>
        //public static void toFileWithHistory(string fname, int history, bool createSubFileFlag, object obj, Type dataType)
        //{
        //    Serializer.toFileWithHistory(fname, history, createSubFileFlag, obj, dataType);
        //}

        /// <summary>
        /// ファイルを保存(ヒストリ,サブファイル対応)
        /// </summary>
        /// <param name="fname">ファイル名</param>
        /// <param name="history">履歴ファイル数</param>
        /// <param name="createSubFileFlag">予備ファイルを作るならtrue</param>
        //public static void toFileWithFilterWithHistory(string fname, int history, bool createSubFileFlag, object obj, Type dataType)
        //{
        //    Serializer.toFileWithFilterWithHistory(fname, history, createSubFileFlag, obj, dataType);
        //}

        /// <summary>
        /// ファイルを保存(ヒストリ,サブファイル対応)
        /// </summary>
        /// <param name="fname">ファイル名</param>
        /// <param name="history">履歴ファイル数</param>
        /// <param name="createSubFileFlag">予備ファイルを作るならtrue</param>
        public void toFileWithHistory<T,C>(string fname, int history, bool createSubFileFlag)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            if ( this is T data)
            {
                Serializer.toFileWithHistory<T,C>(fname, history, createSubFileFlag, data);
            }
            else
            {
                Debug.WriteLine("BAD Class Info!");
            }
        }

        /// <summary>
        /// ファイルを保存(ヒストリ,サブファイル対応)
        /// </summary>
        /// <param name="fname">ファイル名</param>
        /// <param name="history">履歴ファイル数</param>
        /// <param name="createSubFileFlag">予備ファイルを作るならtrue</param>
        public void toFileWithFilterWithHistory<T,C>(string fname, int history, bool createSubFileFlag)
            where T : FSCLJsonSerializeBase
            where C : FSCLJsonTypeInfoProvider<T>
        {
            if (this is T data)
            {
                Serializer.toFileWithFilterWithHistory<T,C>(fname, history, createSubFileFlag, data);
            }
            else
            {
                Debug.WriteLine("BAD Class Info!");
            }
        }

        /// <summary>
        /// staticコンストラクタ
        /// </summary>
        static FSCLJsonSerializeBase()
        {
            Serializer = new FSCLJsonSerializeEngine();
        }
    }
}
