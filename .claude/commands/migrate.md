---
description: Add an EF Core migration and apply it to the development database
---

Add a migration named `$1` and apply it:

```bash
dotnet ef migrations add $1 -p src/Ssabba.Infrastructure -s src/Ssabba.Web -o Migrations
dotnet ef database update -p src/Ssabba.Infrastructure -s src/Ssabba.Web
```

Then review the generated files under `src/Ssabba.Infrastructure/Migrations/` and confirm the schema
change matches the entity change that prompted it.
