# SQL Server 2025 Setup

## Overview

This scenario demonstrates the setup and configuration of SQL Server 2025 with native vector search capabilities. The setup includes container configuration, vector feature enablement, and integration with .NET Aspire for seamless orchestration.

## Container Configuration

### SQL Server 2025 Image
The scenario uses the latest SQL Server 2025 preview image with built-in vector support:

```csharp
var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithImageTag("2025-latest")  // SQL Server 2025 preview
    .WithEnvironment("ACCEPT_EULA", "Y");
```

### Key Configuration Parameters
- **Image Tag**: `2025-latest` provides the latest preview with vector capabilities
- **Lifetime**: `ContainerLifetime.Persistent` ensures data persistence across restarts
- **EULA**: Required acceptance for SQL Server container licensing
- **Data Volume**: Persistent storage for database files

## Database Setup

### Database Creation
The database is configured with vector search capabilities:

```csharp
var productsDb = sql
    .WithDataVolume()  // Persistent storage volume
    .AddDatabase("productsDb");
```

### Entity Framework Configuration
Custom configuration enables vector search support:

```csharp
// Custom DbContext configuration for vector search
var productsDbConnectionString = builder.Configuration.GetConnectionString("productsDb");
builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(productsDbConnectionString, o => o.UseVectorSearch()));
```

**Note**: The standard Aspire `AddSqlServerDbContext` method doesn't support vector search configuration, requiring this custom approach.

## Vector Search Features

### Native Vector Data Types
SQL Server 2025 introduces native vector data types:

```sql
-- Vector column definition in table
CREATE TABLE Products (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(255) NOT NULL,
    Description nvarchar(max),
    Price decimal(10,2),
    Brand nvarchar(100),
    Category nvarchar(100),
    EmbeddingVector vector(1536)  -- Native vector type for embeddings
);
```

### Vector Index Creation
Automatic vector index creation for optimized similarity search:

```sql
-- Vector index for cosine similarity search
CREATE VECTOR INDEX IX_Products_EmbeddingVector 
ON Products(EmbeddingVector)
WITH (VECTOR_TYPE = 'COSINE');
```

## Development Environment Setup

### Prerequisites
- **Docker Desktop**: For container runtime
- **SQL Server 2025 Preview**: Container image access
- **.NET 9.0 SDK**: For application development
- **Entity Framework Core**: Vector search extensions

### Local Development Configuration
The setup automatically handles container lifecycle:

```csharp
// Aspire automatically manages:
// - Container download and startup
// - Database creation and initialization
// - Connection string configuration
// - Health checks and monitoring
```

### Connection String Management
Aspire handles connection string generation and injection:

```csharp
// Connection string automatically provided by Aspire
var connectionString = builder.Configuration.GetConnectionString("productsDb");
// Format: "Server=localhost,<port>;Database=productsDb;..."
```

## Container Management

### Lifecycle Management
SQL Server 2025 container is managed through Aspire:

- **Startup**: Automatic container download and initialization
- **Health Checks**: Built-in health monitoring
- **Persistence**: Data survives container restarts
- **Cleanup**: Managed container lifecycle

### Performance Configuration
Container optimized for development and testing:

```csharp
// Aspire applies optimal settings for:
// - Memory allocation
// - CPU limits  
// - Storage performance
// - Network configuration
```

### Volume Management
Persistent data storage configuration:

```csharp
sql.WithDataVolume()  // Creates persistent volume for:
    // - Database files (.mdf, .ldf)
    // - Vector indexes
    // - Transaction logs
    // - Backup files
```

## Vector Search Integration

### Entity Framework Vector Support
The custom configuration enables vector operations:

```csharp
// Vector property in entity model
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Brand { get; set; }
    public string Category { get; set; }
    
    [Column(TypeName = "vector(1536)")]
    public float[] EmbeddingVector { get; set; }
}
```

### Vector Operations
Native SQL Server vector functions:

```csharp
// Entity Framework vector search
var similarProducts = await context.Products
    .OrderBy(p => EF.Functions.VectorDistance("cosine", p.EmbeddingVector, queryVector))
    .Take(5)
    .ToListAsync();
```

## Database Initialization

### Schema Creation
Automatic database schema generation:

```csharp
public async Task InitializeDatabase(Context context)
{
    // Ensure database exists
    await context.Database.EnsureCreatedAsync();
    
    // Create vector indexes
    await context.Database.ExecuteSqlRawAsync(@"
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_EmbeddingVector')
        CREATE VECTOR INDEX IX_Products_EmbeddingVector 
        ON Products(EmbeddingVector)
        WITH (VECTOR_TYPE = 'COSINE')
    ");
}
```

### Data Seeding
Initial product data and embedding generation:

```csharp
public async Task SeedProducts(Context context, IEmbeddingGenerator embeddingGenerator)
{
    if (!context.Products.Any())
    {
        var products = GetSeedProducts();
        
        foreach (var product in products)
        {
            // Generate embeddings for products
            var text = $"{product.Name} {product.Description} {product.Brand}";
            var embedding = await embeddingGenerator.GenerateEmbeddingAsync(text);
            product.EmbeddingVector = embedding.Vector.ToArray();
            
            context.Products.Add(product);
        }
        
        await context.SaveChangesAsync();
    }
}
```

## Monitoring and Diagnostics

### Aspire Dashboard Integration
SQL Server 2025 integrates with Aspire monitoring:

- **Container Status**: Real-time container health
- **Database Metrics**: Connection counts, query performance
- **Vector Operations**: Index usage and search performance
- **Resource Usage**: Memory, CPU, storage metrics

### SQL Server Diagnostics
Native SQL Server monitoring capabilities:

```sql
-- Vector index statistics
SELECT 
    i.name AS IndexName,
    s.user_seeks,
    s.user_scans,
    s.avg_vector_distance_time_ms
FROM sys.indexes i
JOIN sys.dm_db_index_usage_stats s ON i.object_id = s.object_id
WHERE i.type_desc = 'VECTOR';
```

## Troubleshooting

### Common Issues
1. **Container Download**: Large image size may require time
2. **Vector Support**: Ensure SQL Server 2025 preview image
3. **Memory Requirements**: Vector operations require adequate RAM
4. **Storage Performance**: SSD recommended for vector indexes

### Debug Configuration
Enable detailed logging for troubleshooting:

```csharp
builder.Services.AddDbContext<Context>(options =>
{
    options.UseSqlServer(connectionString, o => o.UseVectorSearch())
           .EnableSensitiveDataLogging()  // Development only
           .LogTo(Console.WriteLine, LogLevel.Information);
});
```

### Performance Tuning
Optimize for vector operations:

```sql
-- Memory allocation for vector operations
EXEC sp_configure 'max server memory (MB)', 4096;
RECONFIGURE;

-- Enable vector optimization
ALTER DATABASE productsDb SET VECTOR_OPTIMIZATION = ON;
```

## Security Considerations

### Container Security
- Use specific image tags rather than 'latest'
- Configure appropriate resource limits
- Secure connection strings and credentials
- Regular security updates for container images

### Database Security
- Enable encryption at rest for sensitive data
- Configure appropriate user permissions
- Use strong passwords for SA account
- Network security for container communications

## Migration from External Vector Stores

### From Chroma DB
```sql
-- Import data from Chroma DB export
BULK INSERT Products_Temp
FROM '/data/chroma_export.csv'
WITH (FORMAT = 'CSV', FIRSTROW = 2);

-- Convert to native vector format
INSERT INTO Products (Name, Description, EmbeddingVector)
SELECT Name, Description, 
       CAST(VectorData AS vector(1536))
FROM Products_Temp;
```

### Performance Benefits
- **Unified Storage**: No data synchronization overhead
- **ACID Compliance**: Full transactional consistency
- **Backup/Recovery**: Standard SQL Server procedures
- **Security**: Integrated SQL Server security model