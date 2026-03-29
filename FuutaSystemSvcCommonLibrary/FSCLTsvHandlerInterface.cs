using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    /// TSV変換用ハンドラーインターフェース
    /// </summary>
    public interface FSCLTsvHandlerInterface<T>
    {
        /// <summary>
        /// TSV文字列に変換
        /// </summary>
        /// <returns></returns>
        public string ToTsvString();

        /// <summary>
        /// TSV文字列から復元して自身に設定
        /// </summary>
        /// <param name="tsv"></param>
        /// <returns></returns>
        public void FromTsvString(string tsv);

    }
}
