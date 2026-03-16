# DT-website

## Lokale database (team setup)

De projectdatabase heet: **dt_website**.

Belangrijk:
- Deze database is **lokaal per ontwikkelaar**.
- Iedereen in het team kan dezelfde setup draaien op eigen computer.
- Het is dus niet gelimiteerd tot één persoon, maar ook niet automatisch één gedeelde cloud-database.

### Vereisten
- PostgreSQL server lokaal geïnstalleerd en draaiend
- `psql` beschikbaar in PATH

### Snelle setup (Windows / PowerShell)

Voer uit vanuit de project root:

```powershell
.\database\setup-local.ps1
```

Optioneel met custom instellingen:

```powershell
.\database\setup-local.ps1 -DatabaseName dt_website -PostgresUser postgres -DbHost localhost -Port 5432
```

Als er een wachtwoord nodig is:

```powershell
.\database\setup-local.ps1 -Password "JOUW_POSTGRES_WACHTWOORD"
```

### Snelle setup (macOS / Linux)

```bash
chmod +x ./database/setup-local.sh
./database/setup-local.sh
```

Of met eigen databasenaam:

```bash
./database/setup-local.sh dt_website
```

### Wat deze setup doet
- Maakt database `dt_website` aan als die nog niet bestaat
- Voert `database/01-schema.sql` uit
- Voert `database/02-data.sql` alleen uit als het bestand niet leeg is

### Controleren

```bash
psql -U postgres -d dt_website -c "\dt"
```