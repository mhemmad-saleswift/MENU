# Saveur — Digital Restaurant Menu Platform

A modern, mobile-first multilingual digital menu with a premium customer experience, a
secure admin dashboard, AR dish previews, and a natural-language AI management assistant.

Built on **.NET 10 / Blazor Web App** with clean architecture.

> This project is unrelated to SaleSwift.

## What's included (vertical foundation)

**Customer menu** (SEO-friendly static SSR)
- Premium responsive UI — hero with parallax, scroll-reveal animations, card layouts, micro-interactions
- Three languages: **English (LTR)**, **Arabic (RTL)**, **Hebrew (RTL)** — switching preserves the current page
- Light / dark mode (cookie-persisted, no flash on load)
- Category browsing, full menu with **live search + filters** (price range, dietary tags, availability, popular)
- Item detail with image gallery, ingredients, allergens, dietary badges
- **AR preview** via `<model-viewer>` (glTF/GLB), graceful fallback to images

**Admin dashboard** (cookie-authenticated, interactive)
- Analytics: most-viewed items & categories, popular searches, language usage
- Category management with **drag-and-drop reordering** and a 3-language editor
- Item management: full editor (pricing, availability, tags, allergens, images, video, 3D model), inline availability toggle, and **bulk actions** (availability, delete)
- **AI Assistant**: type commands in natural language → reviewable plan → confirm destructive actions → execute

**API-first**: read endpoints under `/api/menu/*` return JSON for categories and items.

## Architecture

```
src/
  Restaurant.Domain          Entities + enums (no dependencies)
  Restaurant.Infrastructure  EF Core (SQLite), services, AI agent, seeding
  Restaurant.Web             Blazor Web App — customer + admin UI, endpoints
```

- **Domain** — `Category`, `MenuItem`, translations, `Banner`, `ViewEvent`, `AdminUser`; `Language`/`DietaryTag`/`Allergen` enums.
- **Infrastructure** — `MenuDbContext`, `IMenuService` (read), `IMenuAdminService` (write — also the AI agent's action surface), `IAnalyticsService`, `IRestaurantAgent`. Database is created and seeded on first run.
- **Web** — per-request `LanguageState` from a `lang` cookie sets `<html lang dir>`; customer pages render as static SSR for SEO, admin pages use `InteractiveServer`.

### The AI agent

`RestaurantAgent` turns instructions into a plan of structured actions and executes them
through `IMenuAdminService`. Destructive actions (price changes, availability, deletes)
set `IsDestructive` and require confirmation in the UI.

The parser is a deterministic heuristic so the feature works with **no external API key**.
It sits behind the `IRestaurantAgent` seam — a Claude-backed parser (`claude-opus-4-8`
with tool use) can replace `PlanAsync` without changing callers or the executor.

Understood commands include:
- `Create a Desserts category`
- `Add a new Mojito for 25₪ in the Drinks category`
- `Mark all pizza items as unavailable`
- `Increase all drink prices by 5%`

## Run it

```powershell
cd src/Restaurant.Web
dotnet run
```

Then open the printed URL (e.g. `http://localhost:5221`).

- Customer menu: `/`
- Admin: `/admin` — demo login **admin / admin123**

The SQLite database (`restaurant.db`) is created and seeded automatically on first start.

## Tech

.NET 10 · Blazor Web App (SSR + interactive server) · EF Core 10 (SQLite) ·
cookie authentication · `<model-viewer>` for AR · Google Fonts (Cormorant Garamond,
Inter, Noto Kufi Arabic, Heebo).

## Notable next steps

- Swap the heuristic agent parser for a Claude-backed implementation (description/translation generation).
- Image/video **upload** (currently URL-based) via blob storage.
- Promotional banner management UI (model + seed exist).
- Real-time analytics charts and date-range reports.
"# MENU" 
