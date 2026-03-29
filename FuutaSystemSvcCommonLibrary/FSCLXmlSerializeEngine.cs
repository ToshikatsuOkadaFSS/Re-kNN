using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    /// XMLシリアル化エンジン
    /// </summary>
    public class FSCLXmlSerializeEngine : FSCLSerializeEngineBase
    {
        /// <summary>
        /// 逆シリアル化する
        /// </summary>
        /// <param name="ist"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        //protected override object? fromStreamMain(Stream ist, Type dataType)
        //{
        //    object? ret = null;

        //    try
        //    {
        //        DataContractSerializer serializer = new DataContractSerializer(dataType);
        //        XmlReaderSettings settings = new XmlReaderSettings();
        //        settings.CheckCharacters = false;
        //        XmlReader reader = XmlReader.Create(ist, settings);
        //        ret = serializer.ReadObject(reader);
        //        reader.Close();
        //    }
        //    catch (Exception err)
        //    {
        //        DisplayMessage(err.Message);
        //        if (err.StackTrace != null)
        //        {
        //            DisplayMessage(err.StackTrace);
        //        }
        //    }
        //    finally
        //    {
        //        ist.Close();
        //    }

        //    return ret;
        //}

        /// <summary>
        /// 逆シリアル化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ist"></param>
        /// <returns></returns>
        protected override T? fromStreamMain<T>(Stream ist) where T : class
        {
            object? ret = null;

            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.CheckCharacters = false;
                XmlReader reader = XmlReader.Create(ist, settings);
                ret = serializer.ReadObject(reader);
                reader.Close();
            }
            catch (Exception err)
            {
                DisplayMessage(err.Message);
                if (err.StackTrace != null)
                {
                    DisplayMessage(err.StackTrace);
                }
            }
            finally
            {
                ist.Close();
            }

            return ret as T;
        }

        /// <summary>
        /// ストリームに出力する
        /// </summary>
        /// <param name="ost"></param>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <exception cref="NotImplementedException"></exception>
        //protected override void toStreamMain(Stream ost, object data, Type dataType)
        //{
        //    try
        //    {
        //        DataContractSerializer serializer = new DataContractSerializer(dataType);
        //        XmlWriterSettings settings = new XmlWriterSettings();
        //        settings.Encoding = new UTF8Encoding(false);
        //        XmlWriter writer = XmlWriter.Create(ost, settings);
        //        serializer.WriteObject(writer, Convert.ChangeType(data, dataType));
        //        writer.Flush();
        //        writer.Close();
        //    }
        //    catch (Exception err)
        //    {
        //        DisplayMessage(err.Message);
        //        if (err.StackTrace != null)
        //        {
        //            DisplayMessage(err.StackTrace);
        //        }
        //    }
        //    finally
        //    {
        //        ost.Close();
        //    }
        //}

        /// <summary>
        /// シリアル化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ost"></param>
        /// <param name="data"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected override void toStreamMain<T>(Stream ost, T data)
        {
            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = new UTF8Encoding(false);
                XmlWriter writer = XmlWriter.Create(ost, settings);
                serializer.WriteObject(writer, Convert.ChangeType(data, typeof(T)));
                writer.Flush();
                writer.Close();
            }
            catch (Exception err)
            {
                DisplayMessage(err.Message);
                if (err.StackTrace != null)
                {
                    DisplayMessage(err.StackTrace);
                }
            }
            finally
            {
                ost.Close();
            }
        }
    }
}
