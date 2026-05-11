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

namespace RekNN_MNIST_Sample
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
    }
}
