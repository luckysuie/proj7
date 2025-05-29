using DataEntities;
using Microsoft.Extensions.VectorData;

namespace VectorEntities
{
    public class ProductVector
    {
        [VectorStoreKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [VectorStoreData]
        public string? Name { get; set; } = string.Empty;

        [VectorStoreData]
        public string? Description { get; set; } = string.Empty;

        [VectorStoreData]
        public string Price { get; set; } = string.Empty;

        [VectorStoreVector(1536)]
        public ReadOnlyMemory<float> Vector { get; set; }

        [VectorStoreData]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
