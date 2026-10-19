# SearchSystem Architecture

This document describes the containerized SearchSystem solution started with
`docker compose up --build`.

## System Overview

```mermaid
flowchart LR
    user[Search user] --> web[WebApp\nBlazor]
    web --> gateway[Gateway\nYARP reverse proxy]
    gateway --> api1[API 1\nASP.NET Core]
    gateway --> api2[API 2\nASP.NET Core]
    api1 --> db[(Shared SQLite volume)]
    api2 --> db
    indexer[Indexer\n.NET console] --> db
    indexer --> files[Document folder\n.txt files]
```

The WebApp is the browser-facing application. It sends search requests to the
Gateway, which distributes `/api/*` requests between the two API containers.
Both API containers read the same SQLite database from the `search-data`
Docker volume. The Indexer builds the reverse index from text documents.

## C4 Context

```mermaid
C4Context
title SearchSystem - System Context

Person(searchUser, "Search user", "Searches indexed documents through the web UI")
System(searchSystem, "SearchSystem", "Indexes text documents and provides ranked document search")
System_Ext(fileStore, "Document folder", "Folder containing source .txt files")
System_Ext(searchIndex, "Search index database", "SQLite database containing the reverse index and document metadata")

Rel(searchUser, searchSystem, "Submits searches and views results", "HTTPS")
Rel(searchSystem, fileStore, "Reads .txt files during indexing", "File system")
Rel(searchSystem, searchIndex, "Writes and queries the search index", "SQLite")
```

## C4 Containers

```mermaid
C4Container
title SearchSystem - Container Diagram

Person(searchUser, "Search user", "Uses the browser-based search interface")

System_Ext(documentFolder, "Document folder", "The `medium` folder containing .txt files")
SystemDb(searchDatabase, "Search index database", "SQLite reverse index and document metadata")

System_Boundary(searchSystem, "SearchSystem") {
    Container(webApp, "WebApp", "ASP.NET Core Blazor", "Interactive web UI and search client")
    Container(gateway, "Gateway", "ASP.NET Core + YARP", "Reverse proxy and round-robin load balancer for `/api/*`")
    Container(api1, "Database API 1", "ASP.NET Core Web API", "Search endpoint and database-backed search logic")
    Container(api2, "Database API 2", "ASP.NET Core Web API", "Second API instance for availability and load distribution")
    Container(indexer, "Indexer", ".NET console application", "Crawls .txt files and builds the reverse index")
    ContainerDb(shared, "Shared model library", ".NET class library", "Shared contracts, models, paths, and database interfaces")
    ContainerDb(infrastructure, "Infrastructure library", ".NET class library", "SQLite and PostgreSQL database implementations")
    Container(loki, "Loki", "Grafana Loki", "Stores and indexes application logs")
    Container(grafana, "Grafana", "Grafana", "Queries and visualizes logs from Loki")
}

Rel(searchUser, webApp, "Searches documents", "HTTPS")
Rel(webApp, gateway, "Sends search requests to `/api/search`", "HTTP")
Rel(gateway, api1, "Routes requests (round robin)", "HTTP :5081")
Rel(gateway, api2, "Routes requests (round robin)", "HTTP :5082")
Rel(api1, shared, "Uses shared contracts and interfaces")
Rel(api2, shared, "Uses shared contracts and interfaces")
Rel(api1, infrastructure, "Uses database implementations")
Rel(api2, infrastructure, "Uses database implementations")
Rel(indexer, shared, "Uses indexing contracts and models")
Rel(indexer, infrastructure, "Uses SQLite implementation")
Rel(indexer, documentFolder, "Crawls .txt files", "File system")
Rel(indexer, searchDatabase, "Creates and updates the index", "SQLite")
Rel(api1, searchDatabase, "Reads search data", "SQLite")
Rel(api2, searchDatabase, "Reads search data", "SQLite")
Rel(webApp, loki, "Sends logs through NLog", "HTTP")
Rel(gateway, loki, "Sends logs through NLog", "HTTP")
Rel(api1, loki, "Sends logs through NLog", "HTTP")
Rel(api2, loki, "Sends logs through NLog", "HTTP")
Rel(grafana, loki, "Queries logs", "HTTP")
```

## Request Flow

```mermaid
sequenceDiagram
    participant User as Browser
    participant Web as WebApp
    participant Gateway
    participant API as API instance
    participant DB as SQLite volume

    User->>Web: Search query
    Web->>Gateway: GET /api/search
    Gateway->>API: Forward request
    API->>DB: Read indexed documents
    DB-->>API: Matching documents
    API-->>Gateway: Search results
    Gateway-->>Web: Search results
    Web-->>User: Render results
```

## Logging Flow

```mermaid
flowchart LR
    app[WebApp / Gateway / API] --> nlog[NLog]
    nlog --> console[Container stdout]
    nlog --> loki[Loki HTTP Push API]
    grafana[Grafana] --> loki
    grafana --> user[Logs in Explore]
```

Each ASP.NET Core application uses `NLog.Targets.Loki`. NLog sends structured
JSON logs directly to Loki and also writes them to stdout. The `SERVICE_NAME`
environment variable is written as the Loki `service` label, which makes it
possible to filter logs in Grafana:

```logql
{service=~"api-1|api-2|gateway|webapp"}
```

Promtail is not used. The applications send logs directly to Loki through the
Docker-internal address `http://loki:3100`.

## Docker Services and Ports

| Service | Internal port | Host port | Responsibility |
| --- | ---: | ---: | --- |
| `webapp` | `8080` | `5002` | Browser-facing Blazor UI |
| `gateway` | `8080` | `5000` | Reverse proxy and API load balancing |
| `api-1` | `8080` | - | Search API instance |
| `api-2` | `8080` | - | Search API instance |
| `loki` | `3100` | `3100` | Log storage and query API |
| `grafana` | `3000` | `3000` | Log visualization |

## Persistence

| Volume | Used by | Purpose |
| --- | --- | --- |
| `search-data` | `api-1`, `api-2` | Shared SQLite search database |
| `loki-data` | `loki` | Persistent log data |
| `grafana-data` | `grafana` | Grafana state and configuration |

The two API containers share the same SQLite volume. This is suitable for the
current proof of concept. A production deployment would typically use a
database designed for concurrent multi-instance access, such as PostgreSQL.

## Runtime Ports

| Container | Address | Role |
| --- | --- | --- |
| WebApp | `http://localhost:5002` | Browser-facing search UI |
| Gateway | `http://localhost:5000` | Reverse proxy |
| Database API 1 | Internal only | Search API instance |
| Database API 2 | Internal only | Search API instance |
| Loki | `http://localhost:3100` | Log ingestion and query API |
| Grafana | `http://localhost:3000` | Log dashboards and Explore |

## Startup

```powershell
docker compose up --build
```

Open the WebApp at `http://localhost:5002` and Grafana at
`http://localhost:3000`. Grafana is provisioned with Loki as its datasource.
