using FuutaSystemSvcCommonLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FuutaSystemSvcCommonLibrary
{
    public class FSCLIntForCache<THIS> : IComparable, FSCLCloneAs<THIS>, FSCLTsvHandlerInterface<THIS>
        where THIS : FSCLIntForCache<THIS>, new()
    {
        public int Value { get; set; } = 0;


        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }


        public override bool Equals(object? obj)
        {
            bool ret = false;

            if (obj is FSCLIntForCache<THIS> target)
            {
                if (this.Value == target.Value)
                {
                    ret = true;
                }
            }

            return ret;
        }

        public int CompareTo(object? obj)
        {
            FSCLIntForCache<THIS>? data = obj as FSCLIntForCache<THIS>;

            return Value.CompareTo(data?.Value);
        }


        public THIS CloneAs()
        {
            THIS ret = new();
            ret.Value = this.Value;

            return ret;
        }

        public string ToTsvString()
        {
            return Value.ToString();
        }

        public void FromTsvString(string tsv)
        {
            if ( int.TryParse(tsv, out int val))
            {
                Value = val;
            }
            else
            {
                // bug
                ;
            }
        }
    }
}
