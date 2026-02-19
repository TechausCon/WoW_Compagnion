# WoWInsight

A production-ready Vertical Slice of a multi-platform companion app for World of Warcraft.

## Features
- **Authentication**: Secure Battle.net OAuth2 login with PKCE. Tokens are stored encrypted on the backend. Mobile app receives a backend-specific JWT.
- **Characters**: View your WoW characters (synced from Blizzard API).
- **Mythic+**: View Raider.IO Mythic+ scores and run history.
- **Weekly Checklist**: Track weekly tasks (Vault, Raid, World Boss) locally per character.
- **Offline-First**: All data is cached locally in SQLite on the device.

## Architecture
- **Backend**: .NET 10, ASP.NET Core Web API, Clean Architecture (Domain, Application, Infrastructure, Api).
- **Mobile**: .NET 10 MAUI, MVVM, SQLite-net-pcl.
- **Database**: SQLite (Backend & Mobile).

## Setup

### Prerequisites
- .NET 10 SDK
- Android Emulator or iOS Simulator

### Blizzard Developer Portal
1. Go to https://develop.battle.net/access/clients
2. Create a new Client.
3. Set Redirect URI to: `https://localhost:7123/auth/blizzard/callback` (or your backend URL).
4. Copy `Client ID` and `Client Secret`.
5. Update `WoWInsight/src/backend/WoWInsight.Api/appsettings.json` with these values.

### Backend
1. Navigate to `WoWInsight/src/backend/WoWInsight.Api`.
2. Run `dotnet restore`.
3. Run `dotnet ef database update` (or rely on auto-creation on startup).
4. Run `dotnet run --urls="https://localhost:7123"`.

### Mobile
1. Navigate to `WoWInsight/src/mobile/WoWInsight.Mobile`.
2. Update `BackendApiClient.cs` if your backend URL differs (default `https://10.0.2.2:7123` for Android Emulator).
3. Run on Android Emulator or iOS Simulator.

### First Login Flow
1. Launch App.
2. Click "Login with Battle.net".
3. System browser opens Blizzard Login.
4. After login, browser redirects to Backend `/auth/blizzard/callback`.
5. Backend validates code, exchanges for tokens, creates JWT, and redirects to `wowinsight://auth/callback?token=...`.
6. App intercepts deep link, saves token, and navigates to Characters list.

## Project Structure
- `src/backend`: ASP.NET Core API solution.
- `src/mobile`: MAUI project.
- `docs`: Architecture documentation.
