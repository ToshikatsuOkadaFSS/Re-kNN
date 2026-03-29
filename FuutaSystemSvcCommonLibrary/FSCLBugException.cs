using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    /// バグが発生したときの例外
    /// </summary>
    public class FSCLBugException:FSCLException
    {
        /// <summary>
        /// メッセージ無し版
        /// </summary>
        public FSCLBugException() : base() { }

        /// <summary>
        /// メッセージ有り版
        /// </summary>
        public FSCLBugException(string message) : base(message) { }
    }
}
