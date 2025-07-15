# 09-AzureAppService Scenario Documentation

## Overview
This scenario demonstrates deployment of .NET Aspire microservices to Azure App Service Environment, with AI-powered search and chat using Azure OpenAI.

- Azure App Service Environment for hosting
- Products API for catalog/search
- Store UI for browsing/searching
- Azure OpenAI for chat and embeddings

## Features
- [Azure App Service Environment](./azure-appservice.md)
- [Products API](./products-api.md)
- [Store UI](./store-ui.md)
- [Azure OpenAI](./azure-openai.md)

## Architecture
```
┌───────────────┐    ┌───────────────┐
│   Store (UI)  │───▶│ Products API  │
└───────────────┘    └───────────────┘
      │                   │
      ▼                   ▼
┌────────────────────┐    ┌────────────────────┐
│ Azure App Service  │    │ Azure OpenAI       │
└────────────────────┘    └────────────────────┘
```

## Screenshots

### Aspire Dashboard
![Aspire Dashboard](./images/dashboard.jpg)

### Products Listing
![Products Listing](./images/products.jpg)

### Semantic Search
![Semantic Search](./images/search.jpg)
