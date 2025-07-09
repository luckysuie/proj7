
# AI-Generated Documentation Prompt

**Note:** The following content was generated with the assistance of AI.

You are a documentation and testing agent responsible for generating detailed documentation and automated UI tests for the following scenarios in the eShopLite .NET Aspire sample:
- 01-SemanticSearch
- 03-RealtimeAudio
- 05-deepseek
- 08-Sql2025

Workspace root: scenarios

For each scenario folder (e.g., scenarios/01-SemanticSearch, scenarios/03-RealtimeAudio, scenarios/05-deepseek, scenarios/08-Sql2025) perform these steps:

1. Analyze the .NET Aspire solution:
   - Open the scenario's `src/eShopAppHost/Program.cs` and `.csproj` file.
   - Enumerate all registered services, AI clients, search providers, and custom modules.
   - For each feature, document:
     • Purpose description
     • Configuration sources (user-secrets, appsettings, environment)
     • External dependencies (e.g., Azure Search, Chroma DB, SQL Server)

2. Create a `docs` folder:
   - At the root of the scenario, add a new folder named `docs/`.
   - All markdown docs and screenshots belong here.

3. Generate documentation:
   - For each feature discovered, create `docs/<feature-name>.md` containing:
     • Title and overview
     • Code snippets for registration and usage
     • Configuration notes
   - Create or update `docs/README.md`:
     • List all feature docs
     • Provide a high-level scenario summary
     • Link to screenshots

4. Use Playwright MCP Server to launch and exercise the running application:
   - Launch the Aspire host with `dotnet run --project src/eShopAppHost/eShopAppHost.csproj`.
   - Wait for the debug console to print a login URL of the form:
     `https://localhost:17104/login?t=<token>`
   - Navigate to that URL in Playwright.
   - Simulate the following user flows:
     a. Log in via the token URL.
     b. Open the Aspire Dashboard page.
     c. Browse the Products listing page.
     d. Use the search UI to perform a semantic search.
   - After each page loads, take a full-page screenshot in JPG format.

5. Save screenshots under docs/images as JPG images (not txt files):
   - Ensure screenshots are saved in JPG format, not as text files.
   - Use descriptive filenames:
     • dashboard.jpg
     • products.jpg
     • search.jpg

6. At the end, update docs/README.md to embed each screenshot under the appropriate section, and update the scenario's main README.md to include a **Want to know more?** section linking to docs/README.md.

Your output in the workspace should be, for each scenario:

```
scenarios/<ID>/
  docs/
    README.md
    images/
      dashboard.jpg
      products.jpg
      search.jpg
    <feature-name>.md (one file per feature)
  README.md (updated with "Want to know more?" link)
```

Proceed end-to-end: code analysis, docs generation, Playwright script, screenshots, README updates, and final documentation.
