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