namespace EList.Common.Models
{
    public class PagedList<T>
    {
        private int? total;
        
        public int PageIndex { get; }
        public int PageSize { get; }
        public List<T>? Result { get; set; }
        public int Total
        {
            get
            {
                return total ?? Result?.Count() ?? 0;
            }
            set
            {
                total = value;
            }
        }

        public PagedList(int total, List<T> result, int pageIndex, int pageSize)
        { 
            Total = total;
            Result = result;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }
    }
}
