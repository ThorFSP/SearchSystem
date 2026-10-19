Version 1.1: 18-08-2026
Postgres is added as an option for database engine.

Version 1: 01-08-2026

This codebase is s PoC seachengine that consist of two programs and a class library.

The two programs are the indexer (also called a crawler) and a search program. Both
are simple console programs.

The indexer will crawl a folder (in depth) and create a reverse index
in a database. It will only index text files with .txt as extension.

The search program (see the ConsoleSearch project) offers a query-based search
in the reverse index.

The class library Shared contains classes that are used by the indexer
and the ConsoleSearch. It contains:

- Paths containing a static path the database (used by both the indexer (write-only), and
the search program (read-only).
- BEDocument (BE for Business Entity) - a class representing a document.

Containerized setup
-------------------
Start the complete search system and observability stack with:

	docker compose up --build

Open the web application at http://localhost:5002 and Grafana at
http://localhost:3000. The Gateway is available at http://localhost:5000.

API, Gateway and WebApp use NLog and send structured JSON logs directly to Loki
while also writing them to stdout. Grafana is provisioned with Loki as its
default datasource. In Grafana Explore, use this query to see the logs:

	{service=~"api-1|api-2|gateway|webapp"}

For errors, use:

	{service=~"api-1|api-2|gateway|webapp"} | json | level="ERROR"

The API runs as two replicas behind the Gateway, with SQLite stored in the
shared Docker volume search-data. The local PowerShell launcher remains
available for running the .NET processes without Docker.



