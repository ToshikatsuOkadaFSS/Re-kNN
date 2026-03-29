using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    public class FSCLDictionaryUU<T1, T2> : Dictionary<T1, T2>, FSCLCloneAs<FSCLDictionaryUU<T1, T2>>
        where T1:unmanaged
        where T2:unmanaged
    {
        public FSCLDictionaryUU<T1, T2> CloneAs()
        {
            FSCLDictionaryUU<T1, T2> ret = new();

            foreach (KeyValuePair<T1, T2> item in this)
            {
                ret.Add(item.Key, item.Value);
            }

            return ret;
        }
    }

    public class FSCLDictionaryUC<T1, T2> : Dictionary<T1, T2>, FSCLCloneAs<FSCLDictionaryUC<T1, T2>>
        where T1 : unmanaged
        where T2 : class, FSCLCloneAs<T2>, new()
    {
        public FSCLDictionaryUC<T1, T2> CloneAs()
        {
            FSCLDictionaryUC<T1, T2> ret = new();

            foreach (KeyValuePair<T1, T2> item in this)
            {
                ret.Add(item.Key, item.Value.CloneAs());
            }

            return ret;
        }
    }

    public class FSCLDictionaryCU<T1, T2> : Dictionary<T1, T2>, FSCLCloneAs<FSCLDictionaryCU<T1, T2>>
    where T1 : class, FSCLCloneAs<T1>, new()
    where T2 : unmanaged
    {
        public FSCLDictionaryCU<T1, T2> CloneAs()
        {
            FSCLDictionaryCU<T1, T2> ret = new();

            foreach (KeyValuePair<T1, T2> item in this)
            {
                ret.Add(item.Key.CloneAs(), item.Value);
            }

            return ret;
        }
    }


    public class FSCLDictionaryCC<T1, T2> : Dictionary<T1, T2>, FSCLCloneAs<FSCLDictionaryCC<T1, T2>>
        where T1 : class, FSCLCloneAs<T1>, new()
        where T2 : class, FSCLCloneAs<T2>, new()
    {
        public FSCLDictionaryCC<T1, T2> CloneAs()
        {
            FSCLDictionaryCC<T1, T2> ret = new();

            foreach (KeyValuePair<T1, T2> item in this)
            {
                ret.Add(item.Key.CloneAs(), item.Value.CloneAs());
            }

            return ret;
        }
    }
}
