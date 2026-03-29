using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    /// ちょっと利用条件を厳しくした FSCLValuesSet
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class FSCLValuesClassSetUU<T1, T2>:FSCLCloneAs<FSCLValuesClassSetUU<T1, T2>>
        where T1 : unmanaged
        where T2 : unmanaged
    {
        public T1 Value1 { get; set; } = default;

        public T2 Value2 { get; set; } = default;

        /// <summary>
        /// シリアライズ用デフォルトコンストラクタ
        /// </summary>
        public FSCLValuesClassSetUU()
        {
        }


        public FSCLValuesClassSetUU(T1 v1, T2 v2)
        {
            Value1 = v1;
            Value2 = v2;
        }

        public FSCLValuesClassSetUU<T1, T2> CloneAs()
        {
            FSCLValuesClassSetUU<T1, T2> ret = new();
            ret.Value1 = this.Value1;
            ret.Value2 = this.Value2;

            return ret;
        }
    }

    /// <summary>
    /// ちょっと利用条件を厳しくした FSCLValuesSet
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class FSCLValuesClassSetUC<T1, T2>:FSCLCloneAs<FSCLValuesClassSetUC<T1,T2>>
        where T1 : unmanaged
        where T2 : class, FSCLCloneAs<T2>, new()
    {
        public T1 Value1 { get; set; } = default;

        public T2 Value2 { get; set; } = new();

        /// <summary>
        /// シリアライズ用デフォルトコンストラクタ
        /// </summary>
        public FSCLValuesClassSetUC()
        {
        }

        /// <summary>
        /// 初期化付コンストラクタ
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public FSCLValuesClassSetUC(T1 v1, T2 v2)
        {
            Value1 = v1;
            Value2 = v2.CloneAs();
        }

        public FSCLValuesClassSetUC<T1, T2> CloneAs()
        {
            FSCLValuesClassSetUC<T1, T2> ret = new();
            ret.Value1 = this.Value1;
            ret.Value2 = this.Value2.CloneAs();

            return ret;
        }
    }

    /// <summary>
    /// ちょっと利用条件を厳しくした FSCLValuesSet
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class FSCLValuesClassSetCU<T1, T2>
        where T1 : class, FSCLCloneAs<T1>, new()
        where T2 : unmanaged
    {
        public T1 Value1 { get; set; } = new();

        public T2 Value2 { get; set; } = default;

        /// <summary>
        /// シリアライズ用デフォルトコンストラクタ
        /// </summary>
        public FSCLValuesClassSetCU()
        {
        }

        /// <summary>
        /// 初期化付コンストラクタ
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public FSCLValuesClassSetCU(T1 v1, T2 v2)
        {
            Value1 = v1.CloneAs();
            Value2 = v2;
        }
    }

    /// <summary>
    /// ちょっと利用条件を厳しくした FSCLValuesSet
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class FSCLValuesClassSetCC<T1, T2> : FSCLCloneAs<FSCLValuesClassSetCC<T1, T2>>
        where T1 : class, FSCLCloneAs<T1>, new()
        where T2 : class, FSCLCloneAs<T2>, new()
    {
        public T1 Value1 { get; set; } = new();

        public T2 Value2 { get; set; } = new();

        /// <summary>
        /// シリアライズ用デフォルトコンストラクタ
        /// </summary>
        public FSCLValuesClassSetCC()
        {
        }

        /// <summary>
        /// 初期化付コンストラクタ
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public FSCLValuesClassSetCC(T1 v1, T2 v2)
        {
            Value1 = v1.CloneAs();
            Value2 = v2.CloneAs();
        }

        public FSCLValuesClassSetCC<T1, T2> CloneAs()
        {
            FSCLValuesClassSetCC<T1, T2> ret = new();

            ret.Value1 = Value1.CloneAs();
            ret.Value2 = Value2.CloneAs();

            return ret;
        }
    }


}
