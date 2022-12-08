namespace SpiritIsland.PromoPack1;

class CompoundActionFactory : GrowthActionFactory {

	readonly GrowthActionFactory[] parts;

	public CompoundActionFactory(params GrowthActionFactory[] parts) {
		this.parts = parts;
	}

	public override string Name => string.Join(":",parts.Select(x=>x.Name));

	public override async Task ActivateAsync( SelfCtx ctx ) {
		foreach(var part in parts )
			await part.ActivateAsync( ctx );
	}
}
