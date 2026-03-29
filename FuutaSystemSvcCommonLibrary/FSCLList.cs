using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    /// クローン可能なデータのリスト
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FSCLListC<T> : List<T>, FSCLCloneAs<FSCLListC<T>>
        where T : class, FSCLCloneAs<T>, new()
    {
        public FSCLListC<T> CloneAs()
        {
            FSCLListC<T> ret = new();
            foreach (T elm in this)
            {
                ret.Add(elm.CloneAs());
            }

            return ret;
        }
    }

    /// <summary>
    /// アンマネージドな型のリスト
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FSCLListU<T> : List<T>, FSCLCloneAs<FSCLListU<T>>
        where T : unmanaged
    {
        public FSCLListU<T> CloneAs()
        {
            FSCLListU<T> ret = new();
            foreach (T elm in this)
            {
                ret.Add(elm);
            }

            return ret;
        }
    }
}
