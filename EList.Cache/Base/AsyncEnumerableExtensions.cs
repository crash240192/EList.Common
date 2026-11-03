using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EList.Cache.Base
{
    public static class AsyncEnumerableExtensions
    {
        public static async Task<T[]> AsyncEnumerableToArrayAsync<T>(this IAsyncEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            var results = new List<T>();

            var enumerator = items.GetAsyncEnumerator(cancellationToken);

            try
            {
                while (await enumerator.MoveNextAsync())
                    results.Add(enumerator.Current);
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            return results.ToArray();
        }
    }
}