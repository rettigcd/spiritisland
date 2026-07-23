# Time Passes (cleanup)

## Spirit-Mod
Remove only, no additional cleanup
```
class MyMod : IEndWhenTimePasses {...}
spirit.Mods.Add(new MyMod);
space.Init(new MyMod(),1);
```
With additional cleanup
```
class MyMod : ICleanupSpiritWhenTimePasses { 
	public void Cleanup(Spirit spirit){
		// ... do cleanup
		spirit.Mods.Remove(this); // optional
	}
}
spirit.Mods.Add(new MyMod);
```
## Space-Mod
Remove only, no additional cleanup
```
class MyMod : ISpaceEntity, IEndWhenTimePasses {...}
space.Init(new MyMod(), 1);
```
With cleanup, but does not remove.
```
class MyMod : ISpaceEntity, ICleanupSpaceWhenTimePasses{...}
space.Init(new MyMod(), 1);
```
## Other
```
GameState.Current.AddTimePassesAction( TimePassesAction.Once( gs => self.Forget.ThisCard( card ), TimePassesOrder.Early ) );
```

# Movement
## Push a single token
```
await spaceToken.PushAsync(self);

```

# Run Windows Build

Runs unpackaged (no MSIX). `-t:Run` alone reuses whatever's already in `bin/`
without rebuilding, so pass `Build` too or you'll launch a stale exe.

## Debug build
```
dotnet build SpiritIsland.Maui -f net10.0-windows10.0.19041.0 "-t:Build;Run"
```

## Release build
```
dotnet build SpiritIsland.Maui -f net10.0-windows10.0.19041.0 -c Release "-t:Build;Run"
```

## Notes
- Requires Windows Developer Mode enabled (Settings > Privacy & security > For developers).
- Requires the Windows App Runtime 1.7 "Main" package installed:
  `winget install --id Microsoft.WindowsAppRuntime.1.7 --force`
  (without this, launch fails with `COMException 0x80040154: Class not registered`).
- `WindowsAppSDKSelfContained` is enabled in the csproj for this target - required for
  unpackaged deployment to work at all on this machine.
- Unpackaged Windows apps can't resolve `MauiImage`/`MauiFont` resources via `ms-appx://`
  at runtime (see https://github.com/dotnet/maui/issues/26441) - images/fonts loaded this
  way render as nothing (no exception, no placeholder) even though they lay out normally.
  Spirit/adversary badge images are worked around via embedded assembly resources in
  `NameToBadgeImgConverter` (Windows-only `EmbeddedResource` items in the csproj + a
  `#if WINDOWS` stream-based load). Any other image or font loaded the normal MauiImage/
  MauiFont way will hit the same bug on Windows until packaged (MSIX) or given the same
  embedded-resource treatment.
- `Label`s inside a virtualized `CollectionView`/`BindableLayout` `DataTemplate` render
  no text on Windows unless they have an explicit `BackgroundColor` (even `Transparent`
  works - just needs to be set, not left at the default/unset value). Confirmed via
  hardcoded literal `Text=` still failing to render without it, so it's not a binding
  issue. Likely affects any `CollectionView` template across the app that has a `Label`
  without an explicit `BackgroundColor` - seen so far in NewGamePage.xaml's adversary
  difficulty list; other `CollectionView` usages (SoloGamePage, SpiritPanelView,
  GrowthGroupView, LogView, InvaderBoardSummaryView, ElementDictView, CardsOverlayView,
  CardView, CardSummaryView, SpiritSummaryView) haven't been audited yet.

## Running from Visual Studio
- The default "Windows Machine" launch profile requires MSIX packaging, which this project
  doesn't have configured (no `Package.appxmanifest`). Use the "SpiritIsland.Maui (Unpackaged)"
  profile instead (`Properties/launchSettings.json`), with Platform set to `x64`.
- VS caches the active debug target per-user in `SpiritIsland.Maui.csproj.user`
  (`ActiveDebugFramework` / `ActiveDebugProfile`) - if F5 launches the wrong platform
  (e.g. Android) with no visible way to change it, check/edit that file directly.

# Deploy to Android Device

## List attached devices (to get the `-s <device-id>`)
```
adb devices -l
```

## Debug build
```
dotnet build SpiritIsland.Maui -f net10.0-android -t:Run -p:AdbTarget="-s <device-id>"
```

## Release build
```
dotnet build SpiritIsland.Maui -f net10.0-android -c Release -t:Run -p:AdbTarget="-s <device-id>"
```