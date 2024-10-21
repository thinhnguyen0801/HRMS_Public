
namespace HNOne.Common
{
    /// <summary>
    /// Phương thức mở rộng cho collection
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// kiểm tra một danh sách null hoặc rỗng
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<TSource>(this IEnumerable<TSource>? source) where TSource : class
         => source == null || !source.Any();

        /// <summary>
        /// kiểm tra một danh sách null hoặc rỗng có điều kiện
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<TSource>(this IEnumerable<TSource>? source, Func<TSource, bool> where) where TSource : class
         => source == null || !source.Any(where);

        /// <summary>
        /// cập nhật dữ liệu. tất cả các phần tử
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        public static IEnumerable<TSource>? Update<TSource>(this IEnumerable<TSource>? source, Action<TSource> update) where TSource : class
        {
            if (source == null || !source.Any()) return default;
            foreach (var item in source)
            {
                update(item);
            }
            return source;
        }

        /// <summary>
        /// cập nhật dữ liệu theo điều kiện
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="update"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static IEnumerable<TSource>? Update<TSource>(this IEnumerable<TSource>? source, Action<TSource> update, Func<TSource, bool> where) where TSource : class
        {
            if (source == null || !source.Any()) return default;
            foreach (var item in source)
            {
                if (where(item))
                {
                    update(item);
                }
            }
            return source;
        }
    }
}
