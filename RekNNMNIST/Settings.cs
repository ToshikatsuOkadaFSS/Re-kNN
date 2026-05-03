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

namespace RekNNMNIST
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
        public string TrainImageFilePath { get; set; } = "your\\train-images.idx3-ubyte.idx3-ubyte";

        public string TrainLabelFilePath { get; set; } = "your\\train-labels.idx1-ubyte";

        public string TestImageFilePath { get; set; } = "your\\t10k-images.idx3-ubyte";

        public string TestLabelFilePath { get; set; } = "your\\t10k-labels.idx1-ubyte";

        public string DatabasePath { get; set; } = "your\\path\\database";

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

        /// <summary>
        /// k値
        /// </summary>
        public int[] KValues { get; set; } = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        /// <summary>
        /// 正答判定閾値
        /// </summary>
        public double[] DetectThresholds { get; set; } = new double[] { 0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.55, 0.6, 0.62, 0.64, 0.66, 0.68, 0.7, 0.72, 0.74, 0.76, 0.78, 0.8, 0.82, 0.84, 0.86, 0.88, 0.9, 0.92, 0.94, 0.96 };

        /// <summary>
        /// 評価対象ラベル
        /// </summary>
        public int[] AllLabels { get; set; } = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        /// <summary>
        /// 除外対象ラベル
        /// </summary>
        public int[] DropLabels { get; set; } = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 99 };

        /// <summary>
        /// ベクトル追加時の一致判定閾値
        /// </summary>
        public double[] AddVectorThresholds { get; set; } = new double[] { 0.9, 0.92, 0.94, 0.95, 0.96, 0.97, 0.98, 0.99 };

        /// <summary>
        /// Refine用パラメタ
        /// </summary>
        public double?[] RefineThresholds { get; set; } = new double?[] { null, 0.01 };

        /// <summary>
        /// Refine用パラメタ
        /// </summary>
        public bool[] Refine { get; set; } = new bool[] { false, true };
    }
}
