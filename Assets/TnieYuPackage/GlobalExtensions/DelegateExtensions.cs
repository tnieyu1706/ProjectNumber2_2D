using System;
using System.Linq;

namespace TnieYuPackage.GlobalExtensions
{
    public static class DelegateExtensions
    {
        /// <summary>
        /// Only Apply for delegate, !event
        /// </summary>
        /// <param name="source"></param>
        /// <param name="function"></param>
        /// <typeparam name="TDelegate"></typeparam>
        /// <returns></returns>
        public static TDelegate AddSafe<TDelegate>(this TDelegate source, TDelegate function)
            where TDelegate : Delegate
        {
            if (function == null)
                return source;

            // Nếu delegate hiện tại null ⇒ delegate = function
            if (source == null)
                return function;

            if (!source.Contains(function))
                return (TDelegate)Delegate.Combine(source, function);

            return source;
        }

        public static bool Contains<TDelegate>(this TDelegate source, TDelegate function)
            where TDelegate : Delegate
        {
            if (source == null)
                return false;

            return source.GetInvocationList().Any(d => d.Method == function.Method && d.Target == function.Target);
        }
    }
}