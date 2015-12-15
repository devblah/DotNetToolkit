
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevBlah.DotNetToolkit.Linq
{
    // ReSharper disable once InconsistentNaming
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> Traverse<T>(this IEnumerable<T> items,
            Func<T, IEnumerable<T>> childSelector)
        {
            var stack = new Stack<T>(items);
            while (stack.Any())
            {
                var next = stack.Pop();
                yield return next;
                if (childSelector(next) != null)
                {
                    foreach (var child in childSelector(next))
                    {
                        stack.Push(child);
                    }
                }
            }
        }

        public static IEnumerable<TResult> TraverseOfType<TSource, TResult>(this IEnumerable<TSource> items,
            Func<TResult, IEnumerable<TSource>> childSelector)
        {
            var stack = new Stack<TResult>(items.OfType<TResult>());
            while (stack.Any())
            {
                var next = stack.Pop();
                yield return next;
                if (childSelector(next) != null)
                {
                    foreach (var child in childSelector(next).OfType<TResult>())
                    {
                        stack.Push(child);
                    }
                }
            }
        }
    }
}
