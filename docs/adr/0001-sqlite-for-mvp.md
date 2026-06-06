# SQLite as the database for the MVP

DocumentHub is a multi-tenant SaaS product, which typically implies a shared Postgres or SQL Server instance. We chose SQLite instead for the MVP because the expected tenant count is small, operational simplicity matters more than scalability at this stage, and SQLite requires zero infrastructure. When tenant volume or concurrent write load justifies it, we will migrate to Postgres — the .NET minimal API data layer will be swapped at that point.
