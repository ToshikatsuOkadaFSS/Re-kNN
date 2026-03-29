using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    public class FSCLUnicodeLibrary
    {

        /// <summary>
        /// Unicode 正規化＋小文字化を実施・あと tab もスペースにする。改行とかの制御文字もスペースにする。
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Normalize(string text)
        {
            //return text.ToLower().Normalize(NormalizationForm.FormKC).ToLower().Replace("\t", " ");
            return new string(text.Select(c => char.IsControl(c) ? ' ' : c).ToArray()).Normalize(NormalizationForm.FormKC).ToLower();
        }

        /// <summary>
        /// Unicode 正規化。改行とかの制御文字もスペースにする。
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string NormalizeWithoutLowerCase(string text)
        {
            try
            {
                return new string(text.Select(c => char.IsControl(c) ? ' ' : c).ToArray()).Normalize(NormalizationForm.FormKC);
            }
            catch (ArgumentException)
            {
                // unicode がおかしい→そのまま返す
                return text;
            }
        }

        /// <summary>
        /// Unicode 正規化。改行とかの制御文字もスペースにする。
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string NormalizeWithoutLowerCaseWithoutCatchException(string text)
        {
            return new string(text.Select(c => char.IsControl(c) ? ' ' : c).ToArray()).Normalize(NormalizationForm.FormKC);
        }


        /// <summary>
        /// 単語を正規化した後に、サロゲートペアを意識した単語分割を行う
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<string> DivideChar(string text)
        {
            string norm = Normalize(text);

            List<string> ret = DivideCharWithoutNormalize(norm);

            return ret;
        }

        /// <summary>
        /// 単語を正規化せずに、サロゲートペアを意識した単語分割を行う
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<string> DivideCharWithoutNormalize(string text)
        {
            List<string> ret = new();

            StringInfo info = new(text);

            for (int i = 0; i < info.LengthInTextElements; i++)
            {
                ret.Add(info.SubstringByTextElements(i, 1));
            }

            return ret;
        }


        /// <summary>
        /// 句読点等で文章を分割
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<string> DivideTextBySentence(string text)
        {
            // 分割に使用する文字列
            //string[] SplitStrings = new string[] { "。", "．", ". ", ": " };
            string[] SplitStrings = new string[] { "。", ". ", ": " };

            return DivideTextBySentence(text, SplitStrings);
        }

        /// <summary>
        /// 文章を区切る。maxLength を超えたら、更に2nd level の分割をする。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static List<string> DivideTextBySentence(string text, int maxLength)
        {
            List<string> ret = new();

            string[] SubSplitStrings = new string[] { "、", "：", ":", ";", "." };

            foreach (string line in DivideTextBySentence(text))
            {
                if ( line.Length > maxLength)
                {
                    ret.AddRange(DivideTextBySentence(line, SubSplitStrings));
                }
                else
                {
                    ret.Add(line);
                }
            }

            return ret;
        }

        /// <summary>
        /// 文章を分割（分割文字指定）
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<string> DivideTextBySentence(string text, string[] SplitStrings)
        {
            List<string> buffer = new() { text };

            foreach (string splitString in SplitStrings)
            {
                List<string> splits = FSCLUnicodeLibrary.DivideCharWithoutNormalize(splitString);
                List<string> normsplits = new();
                foreach (string s in splits)
                {
                    normsplits.Add(FSCLUnicodeLibrary.Normalize(s));
                }

                List<string> results = new();

                foreach (string buf in buffer)
                {
                    List<string> div = FSCLUnicodeLibrary.DivideCharWithoutNormalize(buf);
                    List<string> normdiv = new();
                    foreach (string d in div)
                    {
                        normdiv.Add(FSCLUnicodeLibrary.Normalize(d));
                    }

                    string current = string.Empty;
                    int pos = 0;
                    while (pos < normdiv.Count)
                    {
                        bool flag = true;
                        for (int i = 0; i < normsplits.Count; i++)
                        {
                            if (pos + i >= normdiv.Count)
                            {
                                flag = false;
                                break;
                            }
                            if (!normdiv[pos + i].Equals(normsplits[i]))
                            {
                                flag = false;
                                break;
                            }
                        }

                        if (flag)
                        {
                            // 終端文字である
                            for (int i = 0; i < normsplits.Count; i++)
                            {
                                current += div[pos + i];
                            }
                            // results に追加
                            results.Add(current);
                            current = string.Empty;
                            pos += normsplits.Count;
                        }
                        else
                        {
                            // 終端文字では無い
                            current += div[pos];
                            pos += 1;
                        }
                    }

                    // この時点で current に残っているのは分割しきれなかった奴
                    if (current.Length > 0)
                    {
                        results.Add(current);
                    }
                }

                // buffer の内容を入れ替える
                buffer.Clear();
                buffer.AddRange(results);
                results.Clear();
            }

            return buffer;
        }
    }
}
