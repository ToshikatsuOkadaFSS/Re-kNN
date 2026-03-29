using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    public interface FSCLJsonTypeInfoProvider<T>
        where T : FSCLJsonSerializeBase
    {
        public static abstract JsonTypeInfo<T> GetTypeInfo();
    }
}
