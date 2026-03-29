using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    public class FSCLJsonTypeResolver : DefaultJsonTypeInfoResolver
    {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            JsonTypeInfo typeInfo = base.GetTypeInfo(type, options);

            //何かあればこんな形で実装
            //if (type == typeof(Person))
            //{
            //    // カスタムプロパティ名に変更
            //    typeInfo.Properties[nameof(Person.Name)].JsonPropertyName = "full_name";
            //    typeInfo.Properties["Age"].JsonPropertyName = "years";
            //}

            return typeInfo;
        }
    }
}
