using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    /// 共通の実装必要な項目の定義
    /// </summary>
    public interface FSCLCloneable
    {
        /// <summary>
        /// データのクローンを作る
        /// </summary>
        /// <returns></returns>
        public object Clone();

        /// <summary>
        /// source の内容を複写
        /// </summary>
        /// <param name="soruce"></param>
        public void CopyFrom(object source);

        /// <summary>
        /// 型指定での Clone
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T? Clone<T>()
            where T : class;
    }
}
