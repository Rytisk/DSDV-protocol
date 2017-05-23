using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSDV_protocol
{
    static class StringExtension
    {
        public static int GetSequenceCount(this string _sequenceNumber)
        {
            string[] data = _sequenceNumber.Split('-');
            return int.Parse(data[1]);
        }
    }
}
