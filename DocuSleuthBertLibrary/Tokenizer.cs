using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FuutaSystemSvcCommonLibrary;

namespace DocuSleuthBertLibrary
{
    public class Tokenizer
    {
        private Dictionary<string, FSCLValuesSet<List<string>, int>> _vocabulary { get; } = new();


        public Tokenizer(List<string> vocabulary)
        {
            _vocabulary = new();
            int count = 0;
            foreach (string v in vocabulary)
            {
                string key = FSCLUnicodeLibrary.NormalizeWithoutLowerCase(v);
                List<string> chars = FSCLUnicodeLibrary.DivideCharWithoutNormalize(key);
                _vocabulary.TryAdd(key, new(chars, count));
                count += 1;
            }
        }

        public List<(string Token, int VocabularyIndex, long SegmentIndex)> Tokenize(params string[] texts)
        {
            IEnumerable<string> tokens = new string[] { Tokens.Classification };

            foreach (var text in texts)
            {
                tokens = tokens.Concat(TokenizeSentence(FSCLUnicodeLibrary.NormalizeWithoutLowerCase(text)));
                tokens = tokens.Concat(new string[] { Tokens.Separation });
            }

            // texts を分割する。
            List<(string Token, int VocabularyIndex)> tokenAndIndex = new();
            foreach (string token in tokens)
            {
                tokenAndIndex.AddRange(TokenizeSubwords(token));
            }

            //var tokenAndIndex = tokens
            //    .SelectMany(TokenizeSubwords)
            //    .ToList();

            var segmentIndexes = SegmentIndex(tokenAndIndex);

            return tokenAndIndex.Zip(segmentIndexes, (tokenindex, segmentindex)
                                => (tokenindex.Token, tokenindex.VocabularyIndex, segmentindex)).ToList();
        }

        public List<string> Untokenize(List<string> tokens)
        {
            var currentToken = string.Empty;
            var untokens = new List<string>();
            tokens.Reverse();

            tokens.ForEach(token =>
            {
                if (token.StartsWith("##"))
                {
                    currentToken = token.Replace("##", "") + currentToken;
                }
                else
                {
                    currentToken = token + currentToken;
                    untokens.Add(currentToken);
                    currentToken = string.Empty;
                }
            });

            untokens.Reverse();

            return untokens;
        }

        public IEnumerable<long> SegmentIndex(List<(string token, int index)> tokens)
        {
            var segmentIndex = 0;
            var segmentIndexes = new List<long>();

            foreach (var (token, index) in tokens)
            {
                segmentIndexes.Add(segmentIndex);

                if (token == Tokens.Separation)
                {
                    segmentIndex++;
                }
            }

            return segmentIndexes;
        }


        




        /// <summary>
        /// word を分割する。（既に word は unicode 正規化をしている前提)
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private IEnumerable<(string Token, int VocabularyIndex)> TokenizeSubwords(string word)
        {
            var tokens = new List<(string, int)>();

            if ( _vocabulary.ContainsKey(word))
            {
                // マッチした
                tokens.Add((word, _vocabulary[word].Value2));
            }
            else
            {
                // マッチするものがない→分解していく
                while(word.Length > 0)
                {
                    // まず、先頭単語のマッチング探索
                    List<string> matchl = _vocabulary.Keys.ToList().Where(a => word.StartsWith(a, StringComparison.Ordinal))
                        .OrderByDescending(o => o.Count()).ToList();

                    string? match = _vocabulary.Keys.ToList().Where(a => word.StartsWith(a, StringComparison.Ordinal))
                        .OrderByDescending(o => o.Count())
                        .FirstOrDefault();

                    if (match == null)
                    {
                        // マッチしないぞ→しらん単語だ。unknown とする。
                        tokens.Add((Tokens.Unknown, _vocabulary[Tokens.Unknown].Value2));
                        List<string> div = FSCLUnicodeLibrary.DivideCharWithoutNormalize(word);
                        div.RemoveAt(0);
                        word = string.Join("", div);
                    }
                    else
                    {
                        // マッチしたので、次の単語を調べる
                        tokens.Add((match, _vocabulary[match].Value2));
                        word = word.Substring(match.Length);

                        while (true)
                        {
                            if ( word.Length == 0)
                            {
                                // 文字列が無くなった
                                break;
                            }

                            string checkWord = "##" + word;

                            string? matchSub = _vocabulary.Keys.ToList().Where(a => checkWord.StartsWith(a, StringComparison.Ordinal))
                                .OrderByDescending(o => o.Count())
                                .FirstOrDefault();

                            if ( matchSub == null)
                            {
                                // マッチする文字が無かったので loop 脱出
                                break;
                            }

                            if ( matchSub.Length < 3)
                            {
                                // 2文字目以降ではない単語が引っかかった。これも loop 脱出([## + 何か文字]でないといかん)
                                break;
                            }

                            // 見つけた文字を獲得する
                            string found = matchSub.Substring(2);
                            List<string> foundList = FSCLUnicodeLibrary.DivideCharWithoutNormalize(found);

                            tokens.Add((matchSub, _vocabulary[matchSub].Value2));

                            word = checkWord.Substring(matchSub.Length);
                        }
                    }

                }
            }

            return tokens;
        }

        private IEnumerable<string> TokenizeSentence(string text)
        {
            // remove spaces and split the , . : ; etc..
            return text.Split(new string[] { " ", "   ", "\r\n" }, StringSplitOptions.None)
                .SelectMany(o => o.SplitAndKeep(".,;:\\/?!#$%()=+-*\"'–_`<>&^@{}[]|~'。．".ToArray()))
                .Select(o => o.ToLower());
        }




    }
}
