using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    public interface FSCLCloneAs<T>
        where T : class
    {
        /// <summary>
        /// 型指定での Clone
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CloneAs();
    }
}
