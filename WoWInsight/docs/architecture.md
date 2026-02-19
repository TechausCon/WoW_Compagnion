# WoWInsight Architecture

## Overview
WoWInsight is a multi-platform companion app for World of Warcraft, built with .NET 10. It follows Clean Architecture principles to separate concerns and ensure maintainability.

## Backend
- **Clean Architecture**:
  - `Domain`: Core entities (UserAccount, OAuthToken), interfaces.
  - `Application`: Use cases, DTOs, logic (AuthService, CharacterService).
  - `Infrastructure`: Implementation details (Repositories, BlizzardService, SQLite Context).
  - `Api`: REST API Controllers, Dependency Injection wiring.
- **Authentication**:
  - OAuth2 Authorization Code flow with PKCE.
  - Backend acts as Token Broker.
  - Blizzard Tokens stored encrypted in `OAuthToken` entity.
  - Backend issues short-lived HS256 JWT for Mobile API access.
- **Persistence**:
  - EF Core + SQLite.
  - Data Protection API for token encryption.
- **Resilience**:
  - Polly policies for external API calls (Retry, Circuit Breaker - optional).
  - Caching (MemoryCache) for Blizzard and Raider.IO calls.

## Mobile
- **MVVM Pattern**: ViewModels handle business logic and data binding.
- **UI**: MAUI Pages (XAML + Code-behind).
- **Offline-First**:
  - `LocalDbService` (sqlite-net-pcl) caches data.
  - `SyncService` synchronizes data in background.
  - App displays local data immediately.
- **Deep Linking**:
  - Custom URL scheme `wowinsight://` for login callback.

## Data Flow
1. **User Login**: Mobile initiates login -> Backend -> Blizzard Auth -> Backend Callback -> Mobile Deep Link (with JWT).
2. **Sync**: Mobile requests `/me/characters` (Backend JWT Auth) -> Backend calls Blizzard (Blizzard Access Token) -> Returns DTOs -> Mobile saves to SQLite.
3. **Mythic+**: Mobile requests `/characters/{key}/mythicplus` -> Backend calls Raider.IO (Public API) -> Returns Summary -> Mobile saves to SQLite.
