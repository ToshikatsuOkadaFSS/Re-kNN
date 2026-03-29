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

using DocuSleuthBertLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RekNNBERT
{
    internal class CommonValues
    {

        public static TextVectorizer? TextVectorizer { get; set; } = null;

        /// <summary>
        /// 設定値
        /// </summary>
        public static Settings Settings { get; set; } = new Settings();



        public static void LoadSettings()
        {
            if (System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName is string exeFileFullPath)
            {
                if (System.IO.Path.GetDirectoryName(exeFileFullPath) is string basePath)
                {
                    if (RekNNBERT.Settings.fromFile<Settings, SettingsSerializerContext>(System.IO.Path.Combine(basePath, "settings.json")) is Settings data)
                    {
                        Settings = data;
                    }
                    else
                    {
                        // 読み込み失敗→デフォルト値を保存
                        Settings.toFile<Settings, SettingsSerializerContext>(System.IO.Path.Combine(basePath, "settings.json"));
                    }
                }
            }
        }

        public static void SaveSettings()
        {
            if (System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName is string exeFileFullPath)
            {
                if (System.IO.Path.GetDirectoryName(exeFileFullPath) is string basePath)
                {
                    Settings.toFile<Settings, SettingsSerializerContext>(System.IO.Path.Combine(basePath, "settings.json"));
                }
            }
        }

        public static void SaveSettings(int count)
        {
            if (System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName is string exeFileFullPath)
            {
                if (System.IO.Path.GetDirectoryName(exeFileFullPath) is string basePath)
                {
                    Settings.toFile<Settings, SettingsSerializerContext>(System.IO.Path.Combine(basePath, $"settings-{count}.json"));
                }
            }
        }

    }
}
