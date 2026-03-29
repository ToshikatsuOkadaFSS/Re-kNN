using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    /// 各種数値演算用ライブラリ
    /// </summary>
    public class FSCLMathLibrary
    {
        /// <summary>
        /// 指示したデータ群からGini係数を計算する(結果は0～1の値を取る。平均的ならば0に近い。)
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static double Gini(double[] datas)
        {
            if (datas.Length == 0)
            {
                return 0;
            }

            double[] tmp = new double[datas.Length];
            Array.Copy(datas, tmp, datas.Length);
            Array.Sort(tmp);

            double sum = 0;
            double total = 0;
            foreach( double val in tmp)
            {
                total += sum + 0.5 * val;
                sum += val;
            }

            double square = 0.5 * sum * datas.Length;

            // diff には、ローレンツ曲線との差分が入る
            double diff = square - total;

            // 0-1に正規化
            double gini = diff / square;

            if (Double.IsNaN(gini))
            {
                gini = 0;
            }

            return gini;
        }

        /// <summary>
        /// ユークリッド距離の計算。計算できない場合は null を返す
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="dat2"></param>
        /// <returns></returns>
        public static double? EuclideanDistance(double[] data1, double[] data2)
        {
            double? ret = null;

            if ( data1.Length == data2.Length)
            {
                double sum = 0;

                for(int i = 0; i < data1.Length; i++)
                {
                    sum += Math.Pow(data1[i] - data2[i], 2);
                }
                ret = Math.Sqrt(sum);
            }

            return ret;
        }

        /// <summary>
        /// コサイン類似度の計算
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static double CosineSimilarity(double[] vec1, double[] vec2)
        {
            double v12 = 0;
            double v11 = 0;
            double v22 = 0;
            for (int i = 0; i < vec1.Length; i++)
            {
                v12 += vec1[i] * vec2[i];
                v11 += vec1[i] * vec1[i];
                v22 += vec2[i] * vec2[i];
            }

            double sim = 1;
            if (v11 * v22 != 0)
            {
                sim = v12 / (Math.Sqrt(v11) * Math.Sqrt(v22));
            }

            return sim;
        }

        /// <summary>
        /// コサイン類似度の計算
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static double CosineSimilarity(float[] vec1, float[] vec2)
        {
            float v12 = 0;
            float v11 = 0;
            float v22 = 0;
            for (int i = 0; i < vec1.Length; i++)
            {
                v12 += vec1[i] * vec2[i];
                v11 += vec1[i] * vec1[i];
                v22 += vec2[i] * vec2[i];
            }

            double sim = 1;
            if (v11 * v22 != 0)
            {
                sim = v12 / (Math.Sqrt(v11) * Math.Sqrt(v22));
            }

            return sim;
        }

        /// <summary>
        /// コサイン類似度の計算
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static unsafe double CosineSimilarity(float* vec1, float* vec2, int dim)
        {
            float v12 = 0;
            float v11 = 0;
            float v22 = 0;
            for (int i = 0; i < dim; i++)
            {
                v12 += vec1[i] * vec2[i];
                v11 += vec1[i] * vec1[i];
                v22 += vec2[i] * vec2[i];
            }

            double sim = 1;
            if (v11 * v22 != 0)
            {
                sim = v12 / (Math.Sqrt(v11) * Math.Sqrt(v22));
            }

            return sim;
        }

        /// <summary>
        /// コサイン類似度の計算
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static unsafe double CosineSimilarity(float* vec1, float[] vec2, int dim)
        {
            float v12 = 0;
            float v11 = 0;
            float v22 = 0;
            for (int i = 0; i < dim; i++)
            {
                v12 += vec1[i] * vec2[i];
                v11 += vec1[i] * vec1[i];
                v22 += vec2[i] * vec2[i];
            }

            double sim = 1;
            if (v11 * v22 != 0)
            {
                sim = v12 / (Math.Sqrt(v11) * Math.Sqrt(v22));
            }

            return sim;
        }

        /// <summary>
        /// 調和平均の計算(値が0のものがあったら0)
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        public static double HarmonicMean(IEnumerable<double> vals)
        {
            int num = 0;
            double sum = 0;

            foreach(double v in vals)
            {
                if ( v != 0)
                {
                    sum += 1 / v;
                    num += 1;
                }
                else
                {
                    // 0 があったら調和平均は減らす
                    return 0;
                }
            }

            if ( sum == 0)
            {
                return 0;
            }

            return num / sum;
        }

        /// <summary>
        /// コサイン距離の計算(距離は0～1とする)
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static double CosineDistance(double[] vec1, double[] vec2)
        {
            double sim = CosineSimilarity(vec1, vec2);
            double dist = (1 - sim) / 2;

            return dist;
        }

        /// <summary>
        /// コサイン距離の計算(距離は0～1とする)
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static double CosineDistance(float[] vec1, float[] vec2)
        {
            double sim = CosineSimilarity(vec1, vec2);
            double dist = (1 - sim) / 2;

            return dist;
        }

        /// <summary>
        /// コサイン距離の計算(距離は0～1とする)
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static unsafe double CosineDistance(float* vec1, float* vec2, int dim)
        {
            double sim = CosineSimilarity(vec1, vec2, dim);
            double dist = (1 - sim) / 2;

            return dist;
        }

        /// <summary>
        /// コサイン距離の計算(距離は0～1とする)
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static unsafe double CosineDistance(float* vec1, float[] vec2, int dim)
        {
            double sim = CosineSimilarity(vec1, vec2, dim);
            double dist = (1 - sim) / 2;

            return dist;
        }



        public static float[] MergeVector(float[] vec1, double weight1, float[] vec2, double weight2)
        {
            if ( vec1.Length != vec2.Length)
            {
                throw new InvalidDataException();
            }

            float[] ret = new float[vec1.Length];

            for(int i = 0; i <  vec1.Length; i++)
            {
                ret[i] = (float)((vec1[i] * weight1 + vec2[i] * weight2) / (weight1 + weight2));
            }

            return ret;
        }

        public unsafe static float[] MergeVector(float* vec1, double weight1, float* vec2, double weight2, int dimensions)
        {
            float[] ret = new float[dimensions];

            for (int i = 0; i < dimensions; i++)
            {
                ret[i] = (float)((vec1[i] * weight1 + vec2[i] * weight2) / (weight1 + weight2));
            }

            return ret;
        }

        public unsafe static float[] MergeVector(float* vec1, double weight1, float[] vec2, double weight2, int dimensions)
        {
            float[] ret = new float[dimensions];

            for (int i = 0; i < dimensions; i++)
            {
                ret[i] = (float)((vec1[i] * weight1 + vec2[i] * weight2) / (weight1 + weight2));
            }

            return ret;
        }
    }
}
