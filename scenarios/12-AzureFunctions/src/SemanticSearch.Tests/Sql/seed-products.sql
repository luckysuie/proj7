-- seed-products.sql
-- Seed script for Semantic Search integration tests.
-- Creates a simple Products table and inserts sample rows.
-- Intended for local/integration tests that run against the orchestration SQL or a test SQL instance.

IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL
    DROP TABLE dbo.Products;

CREATE TABLE dbo.Products
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(4000) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Category NVARCHAR(200) NULL
);

INSERT INTO dbo.Products
    (Title, Description, Category)
VALUES
    ('Wireless Headphones', 'Over-ear wireless headphones with noise cancellation', 'Audio'),
    ('Bluetooth Speaker', 'Portable Bluetooth speaker with 10 hours battery life', 'Audio'),
    ('Coffee Maker', '12-cup programmable coffee maker', 'Home'),
    ('Gaming Mouse', 'Ergonomic gaming mouse with RGB lighting', 'Gaming'),
    ('Smartphone Case', 'Durable silicone case for 6-inch phones', 'Accessories');

-- Optional: create a simple scalar function to emulate semantic scoring for tests
IF OBJECT_ID('dbo.SemanticScoreFunction', 'FN') IS NOT NULL
    DROP FUNCTION dbo.SemanticScoreFunction;

GO

CREATE FUNCTION dbo.SemanticScoreFunction(@title NVARCHAR(MAX), @description NVARCHAR(MAX), @query NVARCHAR(MAX))
RETURNS FLOAT
AS
BEGIN
    -- Very small heuristic score for testing only: count shared words between title+description and query
    DECLARE @combined NVARCHAR(MAX) = LOWER(ISNULL(@title,'') + ' ' + ISNULL(@description,''));
    DECLARE @q NVARCHAR(MAX) = LOWER(ISNULL(@query,''));
    DECLARE @score FLOAT = 0;

    -- naive tokenization by spaces
    DECLARE @pos INT = 1;
    DECLARE @word NVARCHAR(4000);
    WHILE @pos <= LEN(@q)
    BEGIN
        SET @word = LTRIM(RTRIM(SUBSTRING(@q, @pos, CHARINDEX(' ', @q + ' ', @pos) - @pos)));
        IF @word <> '' AND CHARINDEX(@word, @combined) > 0
            SET @score = @score + 1;
        SET @pos = CHARINDEX(' ', @q + ' ', @pos) + 1;
    END

    RETURN @score;
END;

GO

-- Create a view that returns products with a computed score for a given parameter
IF OBJECT_ID('dbo.Vw_ProductsWithScore', 'V') IS NOT NULL
    DROP VIEW dbo.Vw_ProductsWithScore;

GO

CREATE VIEW dbo.Vw_ProductsWithScore
AS
    SELECT p.Id, p.Title, p.Description, p.Category,
        dbo.SemanticScoreFunction(p.Title, p.Description, CONVERT(NVARCHAR(MAX), '')) AS Score
    FROM dbo.Products p;

GO
