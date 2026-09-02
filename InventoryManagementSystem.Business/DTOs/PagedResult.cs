namespace InventoryManagementSystem.Business.DTOs
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();

        public int Page { get; set; } = 1;

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPrevious => Page > 1;

        public bool HasNext => Page < TotalPages;

        public int FirstRowNumber => ((Page - 1) * PageSize) + 1;
    }
}
