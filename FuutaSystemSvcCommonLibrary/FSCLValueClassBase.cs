using System;
using System.Collections.Generic;

namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    ///WPFアプリケーション用プロパティ基底
    /// </summary>
    public abstract class FSCLValueClassBase: FSCLJsonSerializeBase
    {
        /// <summary>
        /// 値保存用辞書(基本的には全てStringで値を保存)
        /// </summary>
        public Dictionary<String, String> dict = new Dictionary<string, string>();

        /// <summary>
        /// 値を設定したときのイベント(設定した値の名前が引数)
        /// </summary>
        public event Action<String>? OnSetValueEvent = null;

        /// <summary>
        /// bool値の文字列変換用辞書
        /// </summary>
        private Dictionary<bool, String> boolStringDict = new Dictionary<bool, String>
        {
            { true, "true" },
            {false, "false" }
        };

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public FSCLValueClassBase()
        {
            // 特に何もしない
        }

        /// <summary>
        /// 値を設定する(既にあれば更新)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void setValue(String key, String value, String? peopertyName)
        {
            lock (this)
            {
                if (dict.ContainsKey(key))
                {
                    // 既にあったので更新
                    dict[key] = value;
                }
                else
                {
                    dict.Add(key, value);
                }
            }

            if (!(peopertyName is null))
            {
                // プロパティ値が指示されているので、イベント発行
                OnSetValueEvent?.Invoke(peopertyName);
            }
        }

        /// <summary>
        /// Stringとして値を獲得する
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public String? getValueAsString(String key)
        {
            String? ret = null;

            lock (this)
            {
                if (dict.ContainsKey(key))
                {
                    ret = dict[key];
                }
            }

            return ret;
        }


        /// <summary>
        /// 値を設定する(既にあれば更新)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void setValue(String key, bool value, String peopertyName)
        {
            String convValue = boolStringDict[value];

            setValue(key, convValue, peopertyName);
        }

        /// <summary>
        /// bool として値を獲得する(ない値を指示したら常にfalse)
        /// </summary>
        /// <param name="key"></param>
        public bool getValueAsBool(String key)
        {
            bool ret = false;

            String? val = null;

            lock (this)
            {
                val = getValueAsString(key);
            }

            if (val != null)
            {
                foreach (bool k in boolStringDict.Keys)
                {
                    if (boolStringDict[k].CompareTo(val) == 0)
                    {
                        ret = k;
                        break;
                    }
                }
            }

            return ret;
        }

    }
}
