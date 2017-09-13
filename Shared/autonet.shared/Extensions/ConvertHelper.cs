using System;
using System.Collections.Generic;
using System.Text;

namespace autonet.Extensions {
    public static class ConvertHelper {
        /// <summary>
        ///     Checks if <see cref="Convert.ChangeType(object,Type)"/> can be called.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        public static bool CanChangeType(Type value, Type conversionType) {
            return conversionType != null && value != null && typeof(IConvertible).IsAssignableFrom(value) != false;
        }
    }
}