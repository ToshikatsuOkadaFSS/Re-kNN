using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    /// デバッグログの出力用
    /// </summary>
    public class FSCLDebugLogger
    {
        /// <summary>
        /// ログのフルパス
        /// </summary>
        public string FilePath { get; }


        public FSCLDebugLogger(string filePath)
        {
            FilePath = filePath;
        }


        public void WriteLine(string line)
        {
            using FileStream st = new(FilePath, FileMode.Append, FileAccess.Write);
            using StreamWriter streamWriter = new(st, Encoding.UTF8);

            streamWriter.WriteLine($"{DateTime.Now.ToString("yy/MM/dd HH:mm:ss")}:{line}");
        }

        public void WriteLine(List<string> lines)
        {
            using FileStream st = new(FilePath, FileMode.Append, FileAccess.Write);
            using StreamWriter streamWriter = new(st, Encoding.UTF8);

            foreach (string line in lines)
            {
                streamWriter.WriteLine(line);

                streamWriter.WriteLine($"{DateTime.Now.ToString("yy/MM/dd HH:mm:ss")}:{line}");
            }
        }
    }
}
