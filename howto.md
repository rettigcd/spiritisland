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
With additional cleanup

```
class MyMod : ISpaceEntity, ICleanupSpaceWhenTimePasses{...}
space.Init(new MyMod(), 1);
```
## Other
```
GameState.Current.AddTimePassesAction( TimePassesAction.Once( gs => self.Forget.ThisCard( card ), TimePassesOrder.Early ) );
```

