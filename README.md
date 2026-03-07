# IntraGrabber

IntraGrabber is a self-hosted ASP.NET Core web API that fetches school calendar data, weekly plans, and bulletin board posts from [ForældreIntra / SkoleIntra](https://www.skoleintra.dk/) and exposes them through a simple REST API.

It supports:

- **Calendar** – school schedules as JSON or iCalendar (`.ics`) for subscribing in Apple Calendar, Google Calendar, Outlook, etc.
- **Week plans** – weekly schedules and homework as JSON.
- **Pins / Opslagstavle** – bulletin board posts as JSON.

> **Note:** Only one child is supported at the moment.

---

## Quick start with Docker

A pre-built Docker image is published to the GitHub Container Registry on every push to `master`.

```
ghcr.io/mikkelkaas/intragrabber:latest
```

### Docker run

```bash
docker run -d \
  --name intragrabber \
  -p 8080:8080 \
  -e IntraGrabber__BaseAddress="https://YOURSCHOOL.m.skoleintra.dk/" \
  -e IntraGrabber__LoginUsername="your-username" \
  -e IntraGrabber__LoginPassword="your-password" \
  -e IntraGrabber__ChildName="your-child-name" \
  -e IntraGrabber__ParentId="12345" \
  -e IntraGrabber__ClassName="your-class-name" \
  ghcr.io/mikkelkaas/intragrabber:latest
```

Replace the placeholder values with your actual SkoleIntra credentials and school details.

### Docker Compose

Create a `docker-compose.yml`:

```yaml
services:
  intragrabber:
    image: ghcr.io/mikkelkaas/intragrabber:latest
    container_name: intragrabber
    restart: unless-stopped
    ports:
      - "8080:8080"
    environment:
      - IntraGrabber__BaseAddress=https://YOURSCHOOL.m.skoleintra.dk/
      - IntraGrabber__LoginUsername=your-username
      - IntraGrabber__LoginPassword=your-password
      - IntraGrabber__ChildName=your-child-name
      - IntraGrabber__ParentId=12345
      - IntraGrabber__ClassName=your-class-name
```

Then start the container:

```bash
docker compose up -d
```

---

## Configuration

All settings live under the `IntraGrabber` section. When using environment variables, use double underscores (`__`) as separators (standard ASP.NET Core convention).

| Setting | Environment variable | Description |
|---|---|---|
| `BaseAddress` | `IntraGrabber__BaseAddress` | Your school's SkoleIntra URL, e.g. `https://myschool.m.skoleintra.dk/` |
| `LoginUsername` | `IntraGrabber__LoginUsername` | Your SkoleIntra username |
| `LoginPassword` | `IntraGrabber__LoginPassword` | Your SkoleIntra password |
| `ChildName` | `IntraGrabber__ChildName` | The name/identifier of your child |
| `ParentId` | `IntraGrabber__ParentId` | Your parent account ID (integer) |
| `ClassName` | `IntraGrabber__ClassName` | The school class name |
| `CookieName` | `IntraGrabber__CookieName` | Session cookie name (default: `.AspNet.ApplicationCookie`) |
| `LessonFormatString` | `IntraGrabber__LessonFormatString` | API path template for lessons (has a sensible default) |
| `WeekplanFormatString` | `IntraGrabber__WeekplanFormatString` | API path template for week plans (has a sensible default) |
| `PinFormatString` | `IntraGrabber__PinFormatString` | API path template for pins (has a sensible default) |

The format string settings and `CookieName` have built-in defaults in `appsettings.json` and normally do not need to be overridden.

---

## API endpoints

Once the container is running, Swagger UI is available at the root (`/swagger`).

### Calendar

| Method | Path | Query params | Description |
|---|---|---|---|
| `GET` | `/calendar/json` | `daysAhead` (default `7`) | Returns calendar items as JSON |
| `GET` | `/calendar/ical` | `daysAhead` (default `7`) | Returns an iCalendar `.ics` file |

**Example – subscribe to the calendar in your calendar app:**

```
http://localhost:8080/calendar/ical?daysAhead=14
```

### Week plan

| Method | Path | Query params | Description |
|---|---|---|---|
| `GET` | `/weekplan/json` | `nextWeek` (default `false`) | Returns the full week plan |
| `GET` | `/weekplan/today` | – | Returns a simplified plan for today |

### Pins / Opslagstavle

| Method | Path | Query params | Description |
|---|---|---|---|
| `GET` | `/pin/json` | – | Returns bulletin board posts |

---

## Building from source

Prerequisites: [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

```bash
git clone https://github.com/mikkelkaas/IntraGrabber.git
cd IntraGrabber
dotnet run --project IntraGrabber
```

Or build the Docker image locally:

```bash
docker build -t intragrabber .
```

---

## License

This project is licensed under the [MIT License](LICENSE).
