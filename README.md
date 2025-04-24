<!-- filepath: d:\azure-samples\eShopLite\README.md -->
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](/LICENSE)
[![Twitter: elbruno](https://img.shields.io/twitter/follow/elbruno.svg?style=social)](https://twitter.com/elbruno)
![GitHub: elbruno](https://img.shields.io/github/followers/elbruno?style=social)

[![GitHub license](https://img.shields.io/github/license/azure-samples/eshoplite.svg)](https://github.com/azure-samples/eshoplite/blob/main/LICENSE)
[![GitHub contributors](https://img.shields.io/github/contributors/azure-samples/eshoplite.svg)](https://github.com/azure-samples/eshoplite/graphs/contributors/)
[![GitHub issues](https://img.shields.io/github/issues/azure-samples/eshoplite.svg)](https://github.com/azure-samples/eshoplite/issues/)
[![GitHub pull-requests](https://img.shields.io/github/issues-pr/azure-samples/eshoplite.svg)](https://github.com/azure-samples/eshoplite/pulls/)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](http://makeapullrequest.com)

[![GitHub watchers](https://img.shields.io/github/watchers/azure-samples/eshoplite.svg?style=social&label=Watch)](https://github.com/azure-samples/eshoplite/watchers/)
[![GitHub forks](https://img.shields.io/github/forks/azure-samples/eshoplite.svg?style=social&label=Fork)](https://github.com/azure-samples/eshoplite/network/)
[![GitHub stars](https://img.shields.io/github/stars/azure-samples/eshoplite.svg?style=social&label=Star)](https://github.com/azure-samples/eshoplite/stargazers/)

[![Azure AI Community Discord](https://dcbadge.vercel.app/api/server/ByRwuEEgH4)](https://discord.com/invite/ByRwuEEgH4)

# eShopLite

**eShopLite** is a set of reference .NET applications implementing an eCommerce site with features like Semantic Search, MCP, Reasoning models and more.

## Features

This project framework provides the following features:

* Modern .NET application architecture with .NET Aspire
* Various search capabilities (keyword search, semantic search)
* Integration with multiple AI models (GPT-4o, DeepSeek-R1, etc.)
* Vector database implementations (In Memory, Azure AI Search, Chroma DB and others)
* Real-time audio capabilities
* Model Context Protocol (MCP) server and client implementation

## eShopLite Scenarios

The project includes several scenarios demonstrating different capabilities:

| Scenario | Description | Key Technologies |
|----------|-------------|-----------------|
| [01 - Semantic Search](./scenarios/01-SemanticSearch/) | A reference .NET application implementing an eCommerce site with Search features using Keyword Search and Semantic Search. | .NET Aspire, OpenAI GPT-4o-mini, In-memory Vector DB |
| [02 - Azure AI Search](./scenarios/02-AzureAISearch/) | Implements an eCommerce site with Keyword Search using SQL queries and Semantic Search with Vector Database and Azure AI Search. | Azure AI Search, OpenAI Embeddings, SQL Server |
| [03 - Realtime Audio](./scenarios/03-RealtimeAudio/) | Extends the eCommerce site with advanced search features and real-time audio capabilities powered by the GPT-4o Realtime Audio API. | GPT-4o Realtime Audio API, Audio in Blazor, .NET Aspire |
| [04 - Chroma DB](./scenarios/04-chromadb/) | Implements semantic search functionality using Chroma DB, an open-source database designed for AI applications. | Chroma DB, OpenAI Embeddings, .NET SDK |
| [05 - DeepSeek-R1](./scenarios/05-deepseek/) | Demonstrates integration of the DeepSeek-R1 model for enhanced semantic understanding and search capabilities. | DeepSeek-R1, .NET Aspire, Vector Embeddings |
| [06 - Model Context Protocol (MCP)](./scenarios/06-mcp/) | Implements the Model Context Protocol (MCP) for advanced AI interactions with MCP Servers and MCP Clients. | Model Context Protocol, Function Calling, SSE Events |

## Getting Started

### Prerequisites

- [.NET 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or [Podman](https://podman.io/)
- [Azure Developer CLI (azd)](https://aka.ms/install-azd) (for Azure deployment)
- [Git](https://git-scm.com/downloads)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/Azure-Samples/eShopLite.git
   ```

1. Navigate to the scenario directory of interest:
   ```bash
   cd eShopLite/scenarios/[scenario-folder]
   ```

1. Login to Azure:

    ```shell
    azd auth login
    ```

1. Provision and deploy all the resources:

    ```shell
    azd up
    ```

    It will prompt you to provide an `azd` environment name (like "eShopLite"), select a subscription from your Azure account, and select a [location where the necessary models, like gpt-4o-mini and ADA-002 are available](https://azure.microsoft.com/explore/global-infrastructure/products-by-region/?products=cognitive-services&regions=all), a sample region can be "eastus2".

### Quickstart

1. Navigate to a specific scenario folder (e.g., `scenarios/01-SemanticSearch/`)
2. Follow the README instructions in that scenario folder
3. Run the solution using `dotnet run` in the appropriate host project folder

## Demo

To run the demo, follow these steps:

1. Navigate to the specific scenario folder
2. Follow the "Run the solution" instructions in that scenario's README
3. Access the application via the URLs provided in the console output

## Sample Application

This is the eShopLite Aplication running, performing a **Keyword Search**:

![eShopLite Aplication running doing search using keyworkd search](./images/05eShopLite-SearchKeyWord.gif)

This is the eShopLite Aplication running, performing a **Semantic Search**:

![eShopLite Aplication running doing search using keyworkd search](./images/06eShopLite-SearchSemantic.gif)

This is the eShopLite Application running the **Realtime Audio** feature:

![eShopLite Application running the Realtime Audio feature](./images/07eShopLite-RealtimeAudio.gif)

This is the eShopLite Application using the **DeepSeek-R1 Reasoning Model**:

![eShopLite Application using the DeepSeek-R1 Reasoning Model](./images/07ReasoningSearch.png)

The Aspire Dashboard to check the running services:

![Aspire Dashboard to check the running services](./images/10AzureResources.png)

The Azure Resource Group with all the deployed services:

![Azure Resource Group with all the deployed services](./images/15AspireDashboard.png)


## Resources

- [Generative AI for Beginners .NET](https://aka.ms/genainnet)

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)

- [Azure OpenAI Service Documentation](https://learn.microsoft.com/azure/ai-services/openai/)
