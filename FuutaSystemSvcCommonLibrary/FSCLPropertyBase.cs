using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace FuutaSystemSvcCommonLibrary
{
    [DataContract]
    public class FSCLPropertyBase : FSCLJsonSerializeBase
    {
        /// <summary>
        /// プロパティの種別
        /// </summary>
        public enum PropertyTypeEnum
        {
            String,

            Double,

            Int,

            Long,

            Bool,

            DateTime,

            TimeSpan,

            StringList,
        }

        /// <summary>
        /// プロパティの辞書
        /// </summary>
        public Dictionary<string, PropertyTypeEnum> PropertyDict { get; set; } = new();


        /// <summary>
        /// 文字辞書
        /// </summary>
        public Dictionary<string, string> StringDict { get; set; } = new();

        /// <summary>
        /// Double辞書
        /// </summary>
        public Dictionary<string, double> DoubleDict { get; set; } = new();

        /// <summary>
        /// Int辞書
        /// </summary>
        public Dictionary<string, int> IntDict { get; set; } = new();

        /// <summary>
        /// Long辞書
        /// </summary>
        public Dictionary<string, long> LongDict { get; set; } = new();

        /// <summary>
        /// Bool辞書
        /// </summary>
        public Dictionary<string, bool> BoolDict { get; set; } = new();

        /// <summary>
        /// 日付辞書辞書
        /// </summary>
        public Dictionary<string, DateTime> DateTimeDict { get; set; } = new();

        /// <summary>
        /// 時間間隔辞書
        /// </summary>
        public Dictionary<string, TimeSpan> TimeSpanDict { get; set; } = new();

        /// <summary>
        /// 時間間隔辞書
        /// </summary>
        public Dictionary<string, List<string>> StringListDict { get; set; } = new();

        /// <summary>
        /// 値変更イベント(必用に応じて)
        /// </summary>
        public Action<string>? ValueChangedEvent = null;


        /// <summary>
        /// データが指示した型で登録されていれば true
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ptype"></param>
        /// <returns></returns>
        public bool CheckKeyType(string key, PropertyTypeEnum ptype)
        {
            bool ret = false;
            if (CheckKeyExists(key))
            {
                if (PropertyDict[key] == ptype)
                {
                    ret = true;
                }
            }

            return ret;
        }

        /// <summary>
        /// 指示キーが存在していたら true
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool CheckKeyExists(string key)
        {
            bool ret = PropertyDict.ContainsKey(key);

            return ret;
        }


        /// <summary>
        /// 値を設定
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="TSCLBugException"></exception>
        public void SetValue(string key, string value)
        {
            if (PropertyDict.ContainsKey(key))
            {
                if ( PropertyDict[key] != PropertyTypeEnum.String)
                {
                    throw new FSCLBugException("String 以外の情報が既に登録されています。");
                }
            }
            else
            {
                PropertyDict.Add(key, PropertyTypeEnum.String);
            }

            if (StringDict.ContainsKey(key))
            {
                StringDict[key] = value;
            }
            else
            {
                StringDict.Add(key, value);
            }

            ValueChangedEvent?.Invoke(key);
        }


        /// <summary>
        /// 値を獲得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string? GetValueAsString(string key)
        {
            string? ret = null;

            if (StringDict.ContainsKey(key))
            {
                ret = StringDict[key];
            }

            return ret;
        }

        /// <summary>
        /// 値を設定
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="TSCLBugException"></exception>
        public void SetValue(string key, List<string> value)
        {
            if (PropertyDict.ContainsKey(key))
            {
                if (PropertyDict[key] != PropertyTypeEnum.StringList)
                {
                    throw new FSCLBugException("StringList 以外の情報が既に登録されています。");
                }
            }
            else
            {
                PropertyDict.Add(key, PropertyTypeEnum.StringList);
            }

            if (StringListDict.ContainsKey(key))
            {
                StringListDict[key] = new List<string>(value);
            }
            else
            {
                StringListDict.Add(key, new List<string>(value));
            }

            ValueChangedEvent?.Invoke(key);
        }


        /// <summary>
        /// 値を獲得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<string>? GetValueAsStringList(string key)
        {
            List<string>? ret = null;

            if (StringListDict.ContainsKey(key))
            {
                ret = new List<string>(StringListDict[key]);
            }

            return ret;
        }

        /// <summary>
        /// 値を設定
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="TSCLBugException"></exception>
        public void SetValue(string key, bool value)
        {
            if (PropertyDict.ContainsKey(key))
            {
                if (PropertyDict[key] != PropertyTypeEnum.Bool)
                {
                    throw new FSCLBugException("Bool 以外の情報が既に登録されています。");
                }
            }
            else
            {
                PropertyDict.Add(key, PropertyTypeEnum.Bool);
            }

            if (BoolDict.ContainsKey(key))
            {
                BoolDict[key] = value;
            }
            else
            {
                BoolDict.Add(key, value);
            }

            ValueChangedEvent?.Invoke(key);
        }


        /// <summary>
        /// 値を獲得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool? GetValueAsBool(string key)
        {
            bool? ret = null;

            if (BoolDict.ContainsKey(key))
            {
                ret = BoolDict[key];
            }

            return ret;
        }

        /// <summary>
        /// 値を設定
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="TSCLBugException"></exception>
        public void SetValue(string key, int value)
        {
            if (PropertyDict.ContainsKey(key))
            {
                if (PropertyDict[key] != PropertyTypeEnum.Int)
                {
                    throw new FSCLBugException("Bool 以外の情報が既に登録されています。");
                }
            }
            else
            {
                PropertyDict.Add(key, PropertyTypeEnum.Int);
            }

            if (IntDict.ContainsKey(key))
            {
                IntDict[key] = value;
            }
            else
            {
                IntDict.Add(key, value);
            }

            ValueChangedEvent?.Invoke(key);
        }

        /// <summary>
        /// 値を設定
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="TSCLBugException"></exception>
        public void SetValue(string key, long value)
        {
            if (PropertyDict.ContainsKey(key))
            {
                if (PropertyDict[key] != PropertyTypeEnum.Long)
                {
                    throw new FSCLBugException("long 以外の情報が既に登録されています。");
                }
            }
            else
            {
                PropertyDict.Add(key, PropertyTypeEnum.Long);
            }

            if (LongDict.ContainsKey(key))
            {
                LongDict[key] = value;
            }
            else
            {
                LongDict.Add(key, value);
            }

            ValueChangedEvent?.Invoke(key);
        }

        /// <summary>
        /// 値を獲得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int? GetValueAsInt(string key)
        {
            int? ret = null;

            if (IntDict.ContainsKey(key))
            {
                ret = IntDict[key];
            }

            return ret;
        }

        /// <summary>
        /// 値を獲得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long? GetValueAsLong(string key)
        {
            long? ret = null;

            if (LongDict.ContainsKey(key))
            {
                ret = LongDict[key];
            }

            return ret;
        }

        /// <summary>
        /// 値を設定
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="TSCLBugException"></exception>
        public void SetValue(string key, double value)
        {
            if (PropertyDict.ContainsKey(key))
            {
                if (PropertyDict[key] != PropertyTypeEnum.Double)
                {
                    throw new FSCLBugException("Double 以外の情報が既に登録されています。");
                }
            }
            else
            {
                PropertyDict.Add(key, PropertyTypeEnum.Double);
            }

            if (DoubleDict.ContainsKey(key))
            {
                DoubleDict[key] = value;
            }
            else
            {
                DoubleDict.Add(key, value);
            }

            ValueChangedEvent?.Invoke(key);
        }


        /// <summary>
        /// 値を獲得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public double? GetValueAsDouble(string key)
        {
            double? ret = null;

            if (DoubleDict.ContainsKey(key))
            {
                ret = DoubleDict[key];
            }

            return ret;
        }

        /// <summary>
        /// 値を削除
        /// </summary>
        /// <param name="key"></param>
        public void DelValue(string key)
        {
            if (PropertyDict.ContainsKey(key))
            {
                switch (PropertyDict[key])
                {
                    case PropertyTypeEnum.Bool:
                        if (BoolDict.ContainsKey(key))
                        {
                            BoolDict.Remove(key);
                        }
                        break;

                    case PropertyTypeEnum.Double:
                        if (DoubleDict.ContainsKey(key))
                        {
                            DoubleDict.Remove(key);
                        }
                        break;

                    case PropertyTypeEnum.Int:
                        if (IntDict.ContainsKey(key))
                        {
                            IntDict.Remove(key);
                        }
                        break;

                    case PropertyTypeEnum.Long:
                        if (LongDict.ContainsKey(key))
                        {
                            LongDict.Remove(key);
                        }
                        break;

                    case PropertyTypeEnum.String:
                        if (StringDict.ContainsKey(key))
                        {
                            StringDict.Remove(key);
                        }
                        break;

                    case PropertyTypeEnum.DateTime:
                        if (DateTimeDict.ContainsKey(key))
                        {
                            DateTimeDict.Remove(key);
                        }
                        break;

                    case PropertyTypeEnum.TimeSpan:
                        if (TimeSpanDict.ContainsKey(key))
                        {
                            TimeSpanDict.Remove(key);
                        }
                        break;

                    case PropertyTypeEnum.StringList:
                        if (StringListDict.ContainsKey(key))
                        {
                            StringListDict.Remove(key);
                        }
                        break;
                }
                PropertyDict.Remove(key);
            }
        }
    }
}
