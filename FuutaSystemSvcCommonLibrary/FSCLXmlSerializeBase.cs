using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    /// XMLシリアライズ用基底クラス
    /// </summary>
    [DataContract]
    public class FSCLXmlSerializeBase
    {
        /// <summary>
        /// シリアル化エンジン
        /// </summary>
        public static FSCLXmlSerializeEngine Serializer { get; }

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public static object? fromBytes(byte[] data, Type dataType)
        //{
        //    return Serializer.fromBytes(data,dataType);
        //}

        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public static object? fromStream(Stream ist, Type dataType)
        //{
        //    return Serializer.fromStream(ist,dataType);
        //}

        /// <summary>
        /// Fileからインスタンスを生成する
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //public static object? fromFile(string fname, Type dataType)
        //{
        //    return Serializer.fromFile(fname,dataType);
        //}

        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        //public static object? fromFileSupportSub(string fname, Type classType)
        //{
        //    return Serializer.fromFileSupportSub(fname, classType);
        //}

        /// <summary>
        /// 読み込みエラーになったらサブファイルも読む
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        //public object? fromFileSupportHistory(string fname, int history, Type classType)
        //{
        //    return Serializer.fromFileSupportHistory(fname, history, classType);
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
        //public byte[] toBytes(Type dataType)
        //{
        //    return Serializer.toBytes(this, dataType);
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
        //public void toStream(Stream ost, Type dataType)
        //{
        //    Serializer.toStream(ost, this, dataType);
        //}

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
        //public void toFile(string fname, Type dataType)
        //{
        //    Serializer.toFile(fname, this, dataType);
        //}

        /// <summary>
        /// 複数ファイルに出力する
        /// </summary>
        /// <param name="fnameList"></param>
        /// <param name="dataType"></param>
        //public static void toFile(List<string> fnameList, object obj, Type dataType)
        //{
        //    Serializer.toFile(fnameList, obj, dataType);
        //}

        /// <summary>
        /// 複数ファイルに出力する
        /// </summary>
        /// <param name="fnameList"></param>
        /// <param name="dataType"></param>
        //public void toFile(List<string> fnameList, Type dataType)
        //{
        //    Serializer.toFile(fnameList, this, dataType);
        //}

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
        //public void toFileWithHistory(string fname, int history, bool createSubFileFlag, Type dataType)
        //{
        //    Serializer.toFileWithHistory(fname, history, createSubFileFlag, this, dataType);
        //}

        /// <summary>
        /// staticコンストラクタ
        /// </summary>
        static FSCLXmlSerializeBase()
        {
            Serializer = new FSCLXmlSerializeEngine();
        }
    }
}
