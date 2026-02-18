namespace CulinaryRecipes.API.Models.Messaging
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Skip { get; set; }
        public int Take { get; set; }
        public long TotalCount { get; set; }
        public bool HasMore => Skip + Items.Count < TotalCount;
    }
}
