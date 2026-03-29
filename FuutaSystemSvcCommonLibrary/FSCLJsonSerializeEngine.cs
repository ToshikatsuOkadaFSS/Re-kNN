using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    /// Jsonシリアル化エンジン
    /// </summary>
    public class FSCLJsonSerializeEngine : FSCLSerializeEngineBaseForJson
    {
        /// <summary>
        /// Jsonの型Resolverを使うときはここに設定(Thread処理で影響が出ないよう注意)
        /// </summary>
        public DefaultJsonTypeInfoResolver? currentResolver { get; set; } = null;

        /// <summary>
        /// streamを逆シリアル化する(終了時closeする, 暗号化対応できないので非公開)
        /// </summary>
        /// <param name="ist"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //protected override object? fromStreamMain(Stream ist, Type dataType)
        //{
        //    object? result = null;

        //    try
        //    {
        //        JsonSerializerOptions option = new();
        //        option.IgnoreReadOnlyFields = true;
        //        option.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        //        option.WriteIndented = true;
        //        if (currentResolver != null)
        //        {
        //            option.TypeInfoResolver = currentResolver;
        //        }
        //        result = JsonSerializer.Deserialize(ist, dataType, option);
        //    }
        //    catch (Exception err)
        //    {
        //        DisplayMessage(err.Message);
        //        if (err.StackTrace != null)
        //        {
        //            DisplayMessage(err.StackTrace);
        //        }
        //        result = null;
        //    }
        //    finally
        //    {
        //        ist.Close();
        //    }

        //    return result;
        //}

        /// <summary>
        /// streamを逆シリアル化する(終了時closeする)
        /// </summary>
        /// <param name="ist"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //protected override T? fromStreamMain<T, C>(Stream ist, C contextProvider)
        //    where T : FSCLJsonSerializeBase
        //    where C : FSCLJsonTypeInfoProvider<T>
        protected override T? fromStreamMain<T, C>(Stream ist)
            where T : class
        {
            T? result = null;

            try
            {
                //JsonSerializerOptions option = new();
                //option.IgnoreReadOnlyFields = true;
                //option.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                //option.WriteIndented = true;
                //if (currentResolver != null)
                //{
                //    option.TypeInfoResolver = currentResolver;
                //}
                //JsonTypeInfo<T> info = JsonTypeInfo<T>.CreateJsonTypeInfo<T>(option);
                //result = JsonSerializer.Deserialize<T>(ist, info);

                //JsonSerializerOptions options = JsonSerializerOptions.Default;
                //options.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                //JsonTypeInfo<T> info = JsonTypeInfo<T>.CreateJsonTypeInfo<T>(options);

                result = JsonSerializer.Deserialize<T>(ist, C.GetTypeInfo());
            }
            catch (Exception err)
            {
                DisplayMessage(err.Message);
                if (err.StackTrace != null)
                {
                    DisplayMessage(err.StackTrace);
                }
                result = default(T);
            }
            finally
            {
                ist.Close();
            }

            return result;
        }


        /// <summary>
        /// シリアル化してストリームに出力する(終了時closeする)
        /// </summary>
        /// <param name="ost"></param>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        //protected override void toStreamMain(Stream ost, object data, Type dataType)
        //{
        //    try
        //    {
        //        JsonSerializerOptions option = new();
        //        option.IgnoreReadOnlyFields = true;
        //        option.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        //        option.WriteIndented = true;

        //        JsonSerializer.Serialize(ost, data, dataType, option);
        //    }
        //    finally
        //    {
        //        ost.Close();
        //    }
        //}


        /// <summary>
        /// シリアル化してストリームに出力する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ost"></param>
        /// <param name="data"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected override void toStreamMain<T, C>(Stream ost, T data)
        {
            try
            {
                //JsonSerializerOptions option = JsonSerializerOptions.Default;
                //option.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                //option.IgnoreReadOnlyFields = true;
                //option.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                //option.WriteIndented = true;
                //JsonTypeInfo<T> info = JsonTypeInfo<T>.CreateJsonTypeInfo<T>(option);
                //JsonSerializer.Serialize<T>(ost, data, info);

                //JsonSerializerOptions options = JsonSerializerOptions.Default;
                //options.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                //JsonTypeInfo<T> info = JsonTypeInfo<T>.CreateJsonTypeInfo<T>(options);

                JsonSerializer.Serialize(ost, data, C.GetTypeInfo());
            }
            catch(Exception err)
            {
                Trace.WriteLine($"Exception at toStreamMain<{typeof(T).Name}>");
                Trace.WriteLine(err.Message);
                Trace.WriteLine(err.StackTrace);
            }
        }
    }
}
