using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Revert.Core.Extensions
{
    public static class Extensions
    {
        public static string ToMilitaryDateString(this DateTime value, bool includeTime = false)
        {
            return value.ToString("dd MMM yyyy") + (includeTime ? $" {value.ToString("hh:mm")}" : "");
        }
    }
}