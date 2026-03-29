using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    /// 暗号化処理付きで text を load/save するもの
    /// </summary>
    public class FSCLTextFileHandler
    {

        /// <summary>
        /// 出力用の stream
        /// </summary>
        private FileStream? OutputStream { get; set; } = null;

        /// <summary>
        /// 暗号化関数
        /// </summary>
        private Func<byte[], byte[]>? CryptFunc { get; set; } = null;

        /// <summary>
        /// 書き込みのユニット行数
        /// </summary>
        private int UnitLines { get; set; } = 0;

        /// <summary>
        /// 書き込み用の行バッファ
        /// </summary>
        private List<string> LineBuffer { get; } = new();

        /// <summary>
        /// 大量のテキストファイルを順次保存するためのハンドラ
        /// </summary>
        /// <param name="unitLines">分割行数(これを超えたらflush)</param>
        public FSCLTextFileHandler(string fullPath, int unitLines, Func<byte[], byte[]>? cryptFunc)
        {
            try
            {
                OutputStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                CryptFunc = cryptFunc;
                UnitLines = unitLines;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FSCLTextFileHandler / Create Error / {ex.Message}");
                OutputStream = null;
                CryptFunc = null;
            }
        }

        /// <summary>
        /// ユニット単位で保存する
        /// </summary>
        /// <param name="texts"></param>
        public void SaveLines(List<string> texts)
        {
            if (OutputStream == null)
            {
                return;
            }

            LineBuffer.AddRange(texts);

            if ( LineBuffer.Count >= UnitLines)
            {
                // ユニット分たまったので保存
                Flush();
            }
        }

        /// <summary>
        /// ユニット単位で保存する
        /// </summary>
        /// <param name="texts"></param>
        public void SaveLine(string text)
        {
            if (OutputStream == null)
            {
                return;
            }

            LineBuffer.Add(text);

            if (LineBuffer.Count >= UnitLines)
            {
                // ユニット分たまったので保存
                Flush();
            }
        }

        /// <summary>
        /// 書き込みバッファに溜まったデータを一括で書き込む
        /// </summary>
        public void Flush()
        {
            if ( OutputStream == null)
            {
                return;
            }

            MemoryStream mst = new();
            using StreamWriter writer = new(mst, Encoding.UTF8);
            foreach (string line in LineBuffer)
            {
                writer.WriteLine(line);
            }
            writer.Flush();
            writer.Close();
            mst.Close();
            byte[] data = mst.ToArray();
            data = CryptFunc?.Invoke(data) ?? data; 

            // サイズヘッダを書き込み
            byte[] sizeBuf = BitConverter.GetBytes(data.Length);

            OutputStream.Write(sizeBuf, 0, sizeBuf.Length);
            // データ本体を書き込み
            OutputStream.Write(data, 0, data.Length);

            // buffer をクリア
            LineBuffer.Clear();
        }

        /// <summary>
        /// 出力streamを閉じる(バッファに残っていたら書き出す)
        /// </summary>
        public void Close()
        {
            if ( OutputStream != null)
            {
                // 残りのデータを書き込み
                if ( LineBuffer.Count > 0)
                {
                    Flush();
                }
                OutputStream.Close();
                OutputStream = null;
            }
        }

        /// <summary>
        /// バッファにたまっている行数が callbackMinLine を超えたらコールバックを呼び出して保存する(ユニットごとに4byteのサイズヘッダを持つ), もうファイルは複数作らない。
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="ext"></param>
        /// <param name="unitLines"></param>
        /// <param name="callbackMinLine"></param>
        /// <param name="loadCallBack">ある程度読めたところで呼び出すコールバック/処理した行は callback 処理側で削除</param>
        public static void LoadFileV2(string fullPath, Action<List<string>> loadCallBack, Func<byte[], byte[]>? decryptFunc)
        {
            FileInfo finfo = new(fullPath);

            if (!finfo.Exists)
            {
                return;
            }

            long fileSize = finfo.Length;
            long readedSize = 0;
            using FileStream curentStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

            List<string> readLines = new();

            while (true)
            {
                byte[] sizeBuf = new byte[4];
                int sizeRead = 0;
                while(sizeRead < 4)
                {
                    int count = curentStream.Read(sizeBuf, sizeRead, 4 - sizeRead);
                    sizeRead += count;
                    readedSize += count;
                    if (readedSize >= fileSize)
                    {
                        if (sizeRead < 4)
                        {
                            // サイズ分読めなかった
                            Debug.WriteLine($"Load Error / uncomplete data");
                        }

                        break;
                    }
                }
                if (readedSize >= fileSize)
                {
                    // 途中で終端に来てしまった
                    Debug.WriteLine($"Load Error / uncomplete data");
                    break;
                }

                int dataSize = BitConverter.ToInt32(sizeBuf, 0);
                byte[] dataBuf = new byte[dataSize];
                int dataRead = 0;
                while(dataRead < dataSize)
                {
                    int count = curentStream.Read(dataBuf, dataRead, dataSize - dataRead);
                    dataRead += count;
                    readedSize += count;
                    if (readedSize >= fileSize)
                    {
                        if ( dataRead < dataSize)
                        {
                            // サイズ分読めなかった
                            Debug.WriteLine($"Load Error / uncomplete data");
                        }

                        break;
                    }
                }

                // 読み込めたのでデコード
                dataBuf = decryptFunc?.Invoke(dataBuf) ?? dataBuf;

                // 文字列listに分割
                using MemoryStream mst = new(dataBuf);
                using StreamReader reader = new StreamReader(mst, Encoding.UTF8);

                while (reader.ReadLine() is string line)
                {
                    readLines.Add(line);
                }

                // コールバック呼び出し
                loadCallBack(readLines);

                // ファイルの終端チェック
                if (readedSize >= fileSize)
                {
                    break;
                }
            }
        }










        /// <summary>
        /// ファイルの書き出し
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="texts"></param>
        /// <param name="cryptFunc"></param>
        public static void SaveFile(string fullPath, List<string> texts, Func<byte[], byte[]>? cryptFunc)
        {
            MemoryStream mst = new();
            using StreamWriter writer = new(mst, Encoding.UTF8);
            foreach (string line in texts)
            {
                writer.WriteLine(line);
            }
            writer.Flush();
            writer.Close();
            mst.Close();

            byte[] data = mst.ToArray();

            if (cryptFunc != null)
            {
                data = cryptFunc(data);
            }

            FileInfo finfo = new FileInfo(fullPath);

            using FileStream fs = finfo.Create();
            fs.Write(data);
            fs.Close();
        }

        /// <summary>
        /// ファイルの読み込み
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="decryptFunc"></param>
        /// <returns></returns>
        public static List<string>? LoadFile(string fullPath, Func<byte[], byte[]>? decryptFunc)
        {
            FileInfo finfo = new FileInfo(fullPath);

            if (finfo.Exists)
            {
                using FileStream st = finfo.OpenRead();
                byte[] data = new byte[finfo.Length];
                int pos = 0;
                while(pos < data.Length)
                {
                    pos += st.Read(data, pos, data.Length - pos);
                }

                if (decryptFunc != null)
                {
                    data = decryptFunc(data);
                }

                using MemoryStream mst = new(data);
                using StreamReader reader = new StreamReader(mst, Encoding.UTF8);

                List<string> ret = new();

                while(reader.ReadLine() is string line)
                {
                    ret.Add(line);
                }

                return ret;
            }

            return null;
        }


    }
}
