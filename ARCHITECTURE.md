# SearchSystem Architecture

The diagrams below describe the current SearchSystem solution as it is started by
`Start-SearchSystem.ps1`.

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
```

## Runtime Ports

| Container | Address | Role |
| --- | --- | --- |
| WebApp | `http://localhost:5002` | Browser-facing search UI |
| Gateway | `http://localhost:5000` | Reverse proxy |
| Database API 1 | `http://localhost:5081` | Search API instance |
| Database API 2 | `http://localhost:5082` | Search API instance |
