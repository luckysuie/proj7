# SQL seed scripts for Semantic Search tests

This folder contains SQL scripts to seed a test database for the Semantic Search integration tests.

How to use:

- If you run the orchestration (`src/eShopAppHost`), you can execute this script against the `productsDb` database inside the container once it's ready.
- Example using sqlcmd against a local SQL server:

  sqlcmd -S localhost,1433 -U sa -P "Your_password123" -i seed-products.sql

- The script creates a `Products` table, inserts sample rows, and adds a simple scalar function `dbo.SemanticScoreFunction` used for basic scoring in tests.

Notes:

- The `SemanticScoreFunction` provided is a naive heuristic for tests only and should not be used in production.
- If your integration tests use a different DB name or schema, adapt the script accordingly.
