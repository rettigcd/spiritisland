namespace SpiritIsland.NatureIncarnate; 

public class RoilingBogAndSnaggingThorn {

	public const string Name = "Roiling Bog and Snagging Thorn";

	[MinorCard(Name,0,"moon,fire,water,plant"),Fast,FromSacredSite(1)]
	[Instructions( "1 Fear. Isolate. Defend 2. 1 Dahan does not participate in Ravage." ), Artist( Artists.KatGuevara )]
	static public Task ActAsync(TargetSpaceCtx ctx){

		// 1 Fear.
		ctx.AddFear(1);

		// Isolate.
		ctx.Isolate();

		// Defend 2.
		ctx.Defend(2);

		// 1 Dahan does not participate in Ravage.
		ctx.Tokens.Adjust(new DahanSitOutRavage(ctx.Self,1),1);

		return Task.CompletedTask;
	}

}

public class DahanSitOutRavage : BaseModEntity, IConfigRavagesAsync {

	readonly Spirit _picker;
	readonly int _countToSitOut;
	public DahanSitOutRavage( Spirit picker, int countToSitOut ) {
		_picker = picker;
		_countToSitOut = countToSitOut;
	}

	async Task IConfigRavagesAsync.ConfigAsync( SpaceState space ) {

		var ss = space.SourceSelector.AddGroup(_countToSitOut,Human.Dahan).ConfigOnlySelectEachOnce();

		CountDictionary<HumanToken> counts =  new();

		await foreach(SpaceToken dahan in ss.GetEnumerator(_picker,Prompt.RemainingParts("Sit out Ravage"), Present.Always) )
			++counts[dahan.Token.AsHuman()];

		if(0 < counts.Count)
			SitOutRavage.SitOutThisRavageAction(space, counts);

	}
}
