using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    /// ログ出力が必要な処理の中断用例外
    /// </summary>
    public class FSCLNeedLogBreakException:FSCLBreakException
    {
        /// <summary>
        /// メッセージ無し
        /// </summary>
        public FSCLNeedLogBreakException() : base() { }

        /// <summary>
        /// メッセージ無し
        /// </summary>
        public FSCLNeedLogBreakException(string message) : base(message) { }
    }
}
