using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    public class FSCLValuesSet<T1, T2>
    {
        public T1? Value1 { get; set; } = default;

        public T2? Value2 { get; set; } = default;

        /// <summary>
        /// シリアライズ用デフォルトコンストラクタ
        /// </summary>
        public FSCLValuesSet()
        {
        }


        public FSCLValuesSet(T1 v1, T2 v2)
        {
            Value1 = v1;
            Value2 = v2;
        }
    }

    public class FSCLValuesSet<T1, T2, T3>:FSCLJsonSerializeBase
    {
        public T1? Value1 { get; set; } = default;

        public T2? Value2 { get; set; } = default;

        public T3? Value3 { get; set; } = default;

        /// <summary>
        /// シリアライズ用デフォルトコンストラクタ
        /// </summary>
        public FSCLValuesSet()
        {
        }


        public FSCLValuesSet(T1 v1, T2 v2, T3 v3)
        {
            Value1 = v1;
            Value2 = v2;
            Value3 = v3;
        }
    }

    public class FSCLValuesSet<T1, T2, T3, T4>
    {
        public T1? Value1 { get; set; } = default;

        public T2? Value2 { get; set; } = default;

        public T3? Value3 { get; set; } = default;

        public T4? Value4 { get; set; } = default;

        /// <summary>
        /// シリアライズ用デフォルトコンストラクタ
        /// </summary>
        public FSCLValuesSet()
        {
        }


        public FSCLValuesSet(T1 v1, T2 v2, T3 v3, T4 v4)
        {
            Value1 = v1;
            Value2 = v2;
            Value3 = v3;
            Value4 = v4;
        }
    }

    public class FSCLValuesSet<T1, T2, T3, T4, T5>
    {
        public T1? Value1 { get; set; } = default;

        public T2? Value2 { get; set; } = default;

        public T3? Value3 { get; set; } = default;

        public T4? Value4 { get; set; } = default;

        public T5? Value5 { get; set; } = default;

        /// <summary>
        /// シリアライズ用デフォルトコンストラクタ
        /// </summary>
        public FSCLValuesSet()
        {
        }


        public FSCLValuesSet(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5)
        {
            Value1 = v1;
            Value2 = v2;
            Value3 = v3;
            Value4 = v4;
            Value5 = v5;
        }
    }

    public class FSCLValuesSet<T1, T2, T3, T4, T5, T6>
    {
        public T1? Value1 { get; set; } = default;

        public T2? Value2 { get; set; } = default;

        public T3? Value3 { get; set; } = default;

        public T4? Value4 { get; set; } = default;

        public T5? Value5 { get; set; } = default;

        public T6? Value6 { get; set; } = default;

        /// <summary>
        /// シリアライズ用デフォルトコンストラクタ
        /// </summary>
        public FSCLValuesSet()
        {
        }


        public FSCLValuesSet(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6)
        {
            Value1 = v1;
            Value2 = v2;
            Value3 = v3;
            Value4 = v4;
            Value5 = v5;
            Value6 = v6;
        }
    }

    public class FSCLValuesSet<T1, T2, T3, T4, T5, T6, T7>
    {
        public T1? Value1 { get; set; } = default;

        public T2? Value2 { get; set; } = default;

        public T3? Value3 { get; set; } = default;

        public T4? Value4 { get; set; } = default;

        public T5? Value5 { get; set; } = default;

        public T6? Value6 { get; set; } = default;

        public T7? Value7 { get; set; } = default;

        /// <summary>
        /// シリアライズ用デフォルトコンストラクタ
        /// </summary>
        public FSCLValuesSet()
        {
        }


        public FSCLValuesSet(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7)
        {
            Value1 = v1;
            Value2 = v2;
            Value3 = v3;
            Value4 = v4;
            Value5 = v5;
            Value6 = v6;
            Value7 = v7;
        }
    }
}
