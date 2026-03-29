using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;


namespace DocuSleuthBertLibrary
{
    public class TextVectorizer
    {
        /// <summary>
        /// トークナイザー
        /// </summary>
        private Tokenizer tokenizer { get; }


        private InferenceSession session { get; }


        private int VectorSize { get; }

        private int MaxTokenSize { get; }

        /// <summary>
        /// onnx 形式のデータで初期化する
        /// </summary>
        public TextVectorizer(string onnxFileName, string vocabFileName, int vectorSize, int maxTokenSize)
        {
            tokenizer = new Tokenizer(FileReader.ReadFile(vocabFileName));
            session = new InferenceSession(onnxFileName);

            VectorSize = vectorSize;
            MaxTokenSize = maxTokenSize;
        }


        public List<(string Token, int VocabularyIndex, long SegmentIndex)> GetTokens(IEnumerable<string> texts)
        {
            List<(string Token, int VocabularyIndex, long SegmentIndex)> tokens
                = tokenizer.Tokenize(texts.ToArray());

            return tokens;
        }


        public float[][] GetVector(IEnumerable<string> texts)
        {
            List<List<float>> buffer = new();

            List<(string Token, int VocabularyIndex, long SegmentIndex)> tokens = GetTokens(texts);

            if ( tokens.Count > MaxTokenSize)
            {
                tokens.RemoveRange(MaxTokenSize, tokens.Count - MaxTokenSize);
            }

            long[] tokenIndexes = tokens.Select(token => (long)token.VocabularyIndex).ToArray();
            long[] segmentIndexes = tokens.Select(token => token.SegmentIndex).ToArray();
            long[] inputMask = tokens.Select(o => 1L).ToArray();

            List<(long input_ids, long TokenTypeIds, long AttentionMask)> encoded
                = new List<(long input_ids, long TokenTypeIds, long AttentionMask)>();

            for (int i = 0; i < tokens.Count; i++)
            {
                encoded.Add((tokenIndexes[i], segmentIndexes[i], inputMask[i]));
            }

            BertInput bertInput = new BertInput()
            {
                InputIds = encoded.Select(t => t.input_ids).ToArray(),
                AttentionMask = encoded.Select(t => t.TokenTypeIds).ToArray(),
                TypeIds = encoded.Select(t => t.AttentionMask).ToArray(),
            };

            using RunOptions runOptions = new RunOptions();

            using OrtValue inputIdsOrtValue = OrtValue.CreateTensorValueFromMemory(bertInput.InputIds,
                  new long[] { 1, bertInput.InputIds.Length });

            using OrtValue attMaskOrtValue = OrtValue.CreateTensorValueFromMemory(bertInput.AttentionMask,
                  new long[] { 1, bertInput.AttentionMask.Length });

            using OrtValue typeIdsOrtValue = OrtValue.CreateTensorValueFromMemory(bertInput.TypeIds,
                  new long[] { 1, bertInput.TypeIds.Length });

            Dictionary<string, OrtValue> inputs = new Dictionary<string, OrtValue>
                {
                    { "input_ids", inputIdsOrtValue },
                    { "attention_mask", attMaskOrtValue },
                    { "token_type_ids", typeIdsOrtValue }
                };

            using IDisposableReadOnlyCollection<OrtValue> output 
                = session.Run(runOptions, inputs, session.OutputNames);

            ReadOnlySpan<float> vectors = output[0].GetTensorDataAsSpan<float>();

            List<float> vals = new();
            foreach (float val in vectors)
            {
                vals.Add(val);

                if (vals.Count >= VectorSize)
                {
                    buffer.Add(vals);
                    vals = new();
                }
            }

            if (vals.Count > 0)
            {
                buffer.Add(vals);
                vals = new();
            }

            float[][] ret = new float[buffer.Count][];
            int count = 0;
            foreach (List<float> item in buffer)
            {
                ret[count] = item.ToArray();
                count += 1;
            }

            return ret;
        }
    }
}



