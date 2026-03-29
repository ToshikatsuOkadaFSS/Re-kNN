using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    public class CommonFunctions
    {
        /// <summary>
        /// ENで指示された enum に対して、string で指示した名前と同じ要素を検索して返す。見つからなかったら default を返す。
        /// </summary>
        /// <typeparam name="EN"></typeparam>
        /// <param name="name"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static EN GetEnumValue<EN>(string name, EN defaultValue)
        {
            EN ret = defaultValue;

            foreach(EN chk in Enum.GetValues(typeof(EN)))
            {
                if (name.Equals(Enum.GetName(typeof(EN), chk)))
                {
                    ret = chk;
                    break;
                }
            }

            return ret;
        }

        /// <summary>
        /// アンマネージ型であるかどうかをチェック
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsUnmanagedType(Type type)
        {
            bool ret = false;

            if ( type.IsGenericType) ret = true;
            if (type.IsValueType) ret = true;

            return ret;
        }
    }
}
