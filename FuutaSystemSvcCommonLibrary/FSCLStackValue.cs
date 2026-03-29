using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    /// <summary>
    /// 値をStack形式で保存する(Push,Popができる)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FSCLStackValue<T>
    {

        List<T> CurrentValue { get; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="initialValue"></param>
        public FSCLStackValue(T initialValue)
        {
            CurrentValue.Add(initialValue);
        }

        /// <summary>
        /// 最新の値を獲得する(stackの変更はない)
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            lock (this)
            {
                return CurrentValue.Last();
            }
        }

        /// <summary>
        /// Stack に値を一つ追加する
        /// </summary>
        /// <param name="value"></param>
        public void Push(T value)
        {
            lock (this)
            {
                CurrentValue.Add(value);
            }
        }

        /// <summary>
        /// Stack の先頭の値を一つ捨て、その値を返す
        /// </summary>
        public T Pop()
        {
            lock (this)
            {
                if ( CurrentValue.Count >= 1)
                {
                    T ret = CurrentValue.Last();
                    CurrentValue.RemoveAt(CurrentValue.Count-1);
                    return ret;
                }
                else
                {
                    // 空っぽなのに pop しようとしたらエラー
                    throw new FSCLBugException("You can't pop data from empty stack!");
                }
            }
        }

    }
}
