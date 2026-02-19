1.  *Fix Backend API Refresh Endpoint URL Mismatch*
    *   Update `AuthController.cs` to ensure the route is consistent (currently `auth/blizzard/refresh`).
    *   Verify `BackendApiClient.cs` calls the correct URL.
    *   Currently `AuthController` has `[Route("auth/blizzard")]` and `[HttpPost("refresh")]`, so full path is `auth/blizzard/refresh`.
    *   `BackendApiClient` calls `auth/blizzard/refresh`.
    *   So actually the code review stated mismatch might be misunderstanding or I should double check.
    *   Wait, the code review said: "The Mobile client points to the wrong API endpoint (`auth/blizzard/refresh` vs `auth/refresh`)".
    *   Let's check `AuthController.cs` again.
        *   `[Route("auth/blizzard")]`
        *   `[HttpPost("refresh")]`
        *   Result: `auth/blizzard/refresh`.
    *   So the mobile client IS correct. The reviewer might be wrong or I am missing something.
    *   However, maybe I should make it `auth/refresh` to be cleaner?
    *   No, I'll stick to what works and is consistent with the controller.
    *   The reviewer said "The Backend AuthController adds [HttpPost("refresh")]. Assuming standard controller routing... the endpoint is api/auth/refresh".
    *   But I see `[Route("auth/blizzard")]`. So it IS `auth/blizzard/refresh`.
    *   I will double check if I should move it to a generic `auth/refresh` later, but for now it matches.
    *   Wait, maybe the reviewer meant it SHOULD be `auth/refresh`?
    *   I'll keep it as is since it works.

2.  *Address "Core Project" Structure Issue*
    *   The code review correctly pointed out that I moved files to `WoWInsight.Mobile.Core` but maybe didn't ensure the project structure reflects that?
    *   I did verify `WoWInsight.Mobile.Core.csproj` exists and is referenced by `WoWInsight.Mobile.csproj`.
    *   Let's double check if I moved the files correctly.
    *   I used `write_file` to create `WoWInsight.Mobile.Core/ViewModels/...`.
    *   I deleted the old ones in `WoWInsight.Mobile/ViewModels/...`.
    *   So the files ARE in the Core project physically.
    *   And `WoWInsight.Mobile.csproj` references `WoWInsight.Mobile.Core.csproj`.
    *   So the build should pass IF the namespaces are correct.
    *   I kept `namespace WoWInsight.Mobile.ViewModels` in the moved files.
    *   So `LoginPage.xaml` (in `WoWInsight.Mobile`) using `xmlns:vm="clr-namespace:WoWInsight.Mobile.ViewModels"` should still work?
    *   No, because `clr-namespace` in XAML looks in the *local assembly* by default unless `assembly=` is specified.
    *   The ViewModels are now in `WoWInsight.Mobile.Core` assembly.
    *   So `LoginPage.xaml` and `WeeklyChecklistPage.xaml` (and others) need to update their `xmlns:vm` to include `;assembly=WoWInsight.Mobile.Core`.

3.  *Action Plan:*
    *   Update `LoginPage.xaml`, `CharactersPage.xaml`, `CharacterDetailPage.xaml`, `WeeklyChecklistPage.xaml` to fix XAML namespaces.
    *   Verify `MauiProgram.cs` is using the correct namespaces (it should be fine as it's C#).
    *   Submit.
