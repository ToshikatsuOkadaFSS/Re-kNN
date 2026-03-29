using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    /// 風太システムライブラリ用例外
    /// </summary>
    public class FSCLException:Exception
    {
        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public FSCLException() : base() { }

        /// <summary>
        /// メッセージ付きコンストラクタ
        /// </summary>
        /// <param name="message"></param>
        public FSCLException(string message) : base(message) { }
    }
}
