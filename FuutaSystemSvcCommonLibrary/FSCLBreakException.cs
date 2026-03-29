using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    /// 処理の中断用例外
    /// </summary>
    public class FSCLBreakException:FSCLException
    {

        /// <summary>
        /// メッセージ無し例外
        /// </summary>
        public FSCLBreakException() : base() { }

        /// <summary>
        /// メッセージあり例外
        /// </summary>
        public FSCLBreakException(string message) : base(message) { }
    }
}
