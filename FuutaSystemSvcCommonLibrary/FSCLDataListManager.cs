using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography.X509Certificates;


namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    /// 任意データのリストを分割して保存するクラス
    /// </summary>
    public abstract class FSCLDataListManager<T>
    {
        /// <summary>
        /// 保存用データ
        /// </summary>
        public List<T> DataSet { get; set; } = new();


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FSCLDataListManager() { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FSCLDataListManager(IEnumerable<T> data)
        {
            DataSet.AddRange(data);
        }

        /// <summary>
        /// データを追加する
        /// </summary>
        /// <param name="data"></param>
        public void AddData(T data)
        {
            if ( data != null)
            {
                DataSet.Add(data);
            }
        }   

        /// <summary>
        /// データを追加する
        /// </summary>
        /// <param name="datas"></param>
        public void AddDatas(IEnumerable<T> datas)
        {
            foreach(T data in datas)
            {
                if ( data != null)
                {
                    DataSet.Add(data);
                }
            }
        }

        /// <summary>
        /// データを任意サイズで分割する
        /// </summary>
        /// <param name="unitSize"></param>
        /// <returns></returns>
        public virtual List<FSCLDataListManager<T>> Divide(int unitSize, Func<IEnumerable<T>, FSCLDataListManager<T>> newFunction)
        {
            List<FSCLDataListManager<T>> ret = new();

            for ( int i = 0; i < DataSet.Count; i += unitSize)
            {
                List<T> subData = DataSet.GetRange(i, int.Min(DataSet.Count - i, unitSize));
                FSCLDataListManager<T> add = newFunction(subData);
                ret.Add(add);
            }

            return ret;
        }

        /// <summary>
        /// データを任意サイズで分割する
        /// </summary>
        /// <param name="unitSize"></param>
        /// <returns></returns>
        public FSCLDataListManager<T>? GetRange(int pos, int unitSize, Func<IEnumerable<T>, FSCLDataListManager<T>> newFunction)
        {
            if (pos * unitSize >= DataSet.Count)
            {
                return null;
            }

            List<T> subData = DataSet.GetRange(pos * unitSize, int.Min(DataSet.Count - pos * unitSize, unitSize));
            FSCLDataListManager<T> ret = newFunction(subData);

            return ret;
        }


        /// <summary>
        /// 分割してファイルに保存する
        /// </summary>
        /// <param name="path"></param>
        /// <param name="baseName"></param>
        /// <param name="extension"></param>
        /// <param name="length"></param>
        /// <param name="divideFunc"></param>
        public void toFile(string path, string baseName, string extension, int length, Func<int, int, byte[]> divideFunc)
        {
            int count = 0;
            int pos = length * count;

            while (true)
            {
                string name = $"{baseName}-{count.ToString("0000")}{extension}";
                string fname = $"{System.IO.Path.Join(path, name)}";

                byte[] data = divideFunc(count, int.Min(length, DataSet.Count - pos));

                if ( data.Length == 0)
                {
                    // もうデータがなかった
                    if (File.Exists(fname))
                    {
                        File.Delete(fname);
                    }
                    else
                    {
                        // 削除するファイルも無い
                        break;
                    }
                }
                else
                {
                    FileInfo finfo = new(fname);
                    using FileStream st = finfo.Create();
                    st.Write(data);
                }

                pos += length;
                count += 1;
            }

            return;
        }

        /// <summary>
        /// ファイルを読み込む
        /// </summary>
        /// <param name="path"></param>
        /// <param name="baseName"></param>
        /// <param name="extension"></param>
        /// <param name="deserializeFunc"></param>
        /// <returns></returns>
        public static T? fromFile(string path, string baseName, string extension, Func<byte[], T?> deserializeFunc, Func<T?, T?, T?> mergeFunc)
        {
            int count = 0;

            T? ret = default(T);

            while (true)
            {
                string name = $"{baseName}-{count.ToString("0000")}{extension}";
                string fname = $"{System.IO.Path.Join(path, name)}";
                FileInfo finfo = new(fname);

                if (finfo.Exists)
                {
                    Console.WriteLine($"Now loading {fname}");
                    using FileStream fs = finfo.OpenRead();
                    int pos = 0;
                    int len = Convert.ToInt32(finfo.Length);
                    byte[] data = new byte[len];

                    while (pos < len)
                    {
                        pos += fs.Read(data, pos, len - pos);
                    }

                    T? add = deserializeFunc(data);

                    ret = mergeFunc(ret, add);
                }
                else
                {
                    break;
                }

                count += 1;
            }

            return ret;
        }


        /// <summary>
        /// メモリ節約のためにデータをクリアするための処理
        /// </summary>
        public void Clear()
        {
            this.DataSet.Clear();
        }

    }
}
