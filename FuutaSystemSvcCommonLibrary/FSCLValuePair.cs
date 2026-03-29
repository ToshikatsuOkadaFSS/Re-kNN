using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    public class FSCLValuePair<T1, T2>
    {
        public T1? Value1 { get; set; } = default;

        public T2? Value2 { get; set; } = default;

        /// <summary>
        /// シリアライズ用デフォルトコンストラクタ
        /// </summary>
        public FSCLValuePair()
        {
        }


        public FSCLValuePair(T1 v1, T2 v2)
        {
            Value1 = v1;
            Value2 = v2;
        }
    }


}
