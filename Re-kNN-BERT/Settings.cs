// Copyright 2026 Fuuta System Service LLC.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using FuutaSystemSvcCommonLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace RekNNBERT
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Settings))]
    public partial class SettingsSerializerContext : JsonSerializerContext, FSCLJsonTypeInfoProvider<Settings>
    {
        public static JsonTypeInfo<Settings> GetTypeInfo()
        {
            return SettingsSerializerContext.Default.Settings;
        }
    }

    public class Settings :FSCLJsonSerializeBase
    {

        public string OnnxPath{ get; set; } = "your\\path\\model.onnx";

        
        public string VocabPath { get; set; } = "your\\path\\vocab.txt";


        public int MaxTokenSize { get; set; } = 512;


        public string TextFilePath { get; set; } = "your\\textfile\\rootpath";


        public string DatabasePath { get; set; } = "your\\path\\database";

        ///// <summary>
        ///// ファイル情報 / ファイル名単位で、テキストの行ごとに(テキスト,総トークン数)を保持する辞書
        ///// </summary>
        //[JsonIgnore]
        //public Dictionary<string, Dictionary<int, int>> TextFileDictionary { get; set; } = new();

        

        public class IdFileDictInfo
        {
            public int[] ids { get; set; } = Array.Empty<int>();
            public string[] Names { get; set; } = Array.Empty<string>();
        }


        //public List<FSCLValuesSet<string, int, int>> TextFileValues
        //{
        //    get
        //    {
        //        List<FSCLValuesSet<string, int, int>> result = new();
        //        foreach (var kvp in TextFileDictionary)
        //        {
        //            string filename = kvp.Key;
        //            Dictionary<int, int> lineDict = kvp.Value;
        //            foreach (var lineKvp in lineDict)
        //            {
        //                int lineNumber = lineKvp.Key;
        //                int tokenCount = lineKvp.Value;
        //                result.Add(new FSCLValuesSet<string, int, int>(filename, lineNumber, tokenCount));
        //            }
        //        }
        //        return result;
        //    }
        //    set
        //    {
        //        TextFileDictionary.Clear();
        //        foreach(FSCLValuesSet<string, int, int> item in value)
        //        {
        //            string filename = item.Value1 ?? string.Empty;
        //            int lineNumber = item.Value2;
        //            int tokenCount = item.Value3;
        //            TextFileDictionary.TryAdd(filename, new Dictionary<int, int>());
        //            TextFileDictionary[filename].TryAdd(lineNumber, tokenCount);
        //        }
        //    }
        //}

        /// <summary>
        /// ファイル情報 / ファイル名とファイル番号の辞書
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, int> TextFile2IdDictionary { get; set; } = new();


        public IdFileDictInfo TextFile2IdValues
        {
            get
            {
                IdFileDictInfo result = new();
                result.ids = new int[TextFile2IdDictionary.Count];
                result.Names = new string[TextFile2IdDictionary.Count];

                for(int i = 0; i < TextFile2IdDictionary.Count; i++)
                {
                    var kvp = TextFile2IdDictionary.ElementAt(i);
                    result.Names[i] = kvp.Key;
                    result.ids[i] = kvp.Value;
                }

                return result;
            }
            set
            {
                TextFile2IdDictionary.Clear();
                Id2TextFileDictionary.Clear();
                int count = value.ids.Length;
                for(int i = 0; i < count; i++)
                {
                    TextFile2IdDictionary.TryAdd(value.Names[i], value.ids[i]);
                    Id2TextFileDictionary.TryAdd(value.ids[i], value.Names[i]);
                }
            }
        }

        /// <summary>
        /// ファイル情報 / ファイル名とファイル番号の辞書(逆引き)
        /// </summary>
        [JsonIgnore]
        public Dictionary<int, string> Id2TextFileDictionary { get; set; } = new();


        /// <summary>
        /// コサイン類似度の閾値
        /// </summary>
        public double SimilarityThreshold { get; set; } = 0.9;

        /// <summary>
        /// 追加時の候補数
        /// </summary>
        public int SearchMaxNumForAddVector { get; set; } = 5;

        /// <summary>
        /// 最大スレッド数(評価用)
        /// </summary>
        public int MaxThread { get; set; } = 20;
    }
}
