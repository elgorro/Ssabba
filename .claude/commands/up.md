---
description: Bring the development stack up and report service health
---

```bash
cd deploy && docker compose up -d --build
docker compose ps --format '{{.Service}} {{.Status}}'
```

Wait until `web` reports healthy, then summarise anything that is not `Up`, quoting the relevant lines
from `docker compose logs <service>`.
