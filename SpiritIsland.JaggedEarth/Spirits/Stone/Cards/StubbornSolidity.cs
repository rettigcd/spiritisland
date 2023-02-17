namespace SpiritIsland.JaggedEarth;

public class StubbornSolidity {

	public const string Name = "Stubborn Solidity";

	[SpiritCard(StubbornSolidity.Name,1,Element.Sun, Element.Earth, Element.Animal), Fast, FromPresence(1)]
	static public Task ActAsync(TargetSpaceCtx ctx ) {
		// Defend 1 per dahan  (protects the land)
		ctx.Defend( ctx.Dahan.CountAll );

		// Dahan in target land cannot be changed.
		// ( when they would be damaged, destroyed, removed, replaced, or moved, instead don't)
		ctx.Tokens.Init(new StubbornSolidityBehavior(),1);

		return Task.CompletedTask;
	}

}

public class StubbornSolidityBehavior : IModifyRemovingToken
	, IStopDahanDamage
	, IEndWhenTimePasses
{

	public IEntityClass Class => ActionModTokenClass.Mod;

	public void ModifyRemoving( RemovingTokenArgs args ) {
		if(	args.Token.Class == Human.Dahan ) args.Count = 0;
		if( args.Mode == RemoveMode.Live )
			GameState.Current.Log(new Log.Debug("Stuborn Solidity stopping Dahan from being changed."));
	}

}