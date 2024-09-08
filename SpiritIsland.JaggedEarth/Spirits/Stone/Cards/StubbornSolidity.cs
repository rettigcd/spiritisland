namespace SpiritIsland.JaggedEarth;

public class StubbornSolidity {

	public const string Name = "Stubborn Solidity";

	[SpiritCard(StubbornSolidity.Name,1,Element.Sun, Element.Earth, Element.Animal), Fast, FromPresence(1)]
	[Instructions( "Defend 1 per Dahan. Dahan in target land cannot be changed. (When they would be Damaged, Destroyed, Removed, Replaced, or moved, instead don't)" ), Artist( Artists.MoroRogers )]
	static public Task ActAsync(TargetSpaceCtx ctx ) {
		// Defend 1 per dahan  (protects the land)
		ctx.Defend( ctx.Dahan.CountAll );

		// Dahan in target land cannot be changed.
		// ( when they would be damaged, destroyed, removed, replaced, or moved, instead don't)
		new StubbornSolidityBehavior().InitOn(ctx.Space);

		return Task.CompletedTask;
	}

}

public class StubbornSolidityBehavior
	: IModifyAddingToken
	, IModifyRemovingToken
	, IModifyDahanDamage
	, ICleanupSpaceWhenTimePasses
	, IEndWhenTimePasses
{

	public void InitOn( Space space) {
		// make normal Dahan Stubbornly Solid
		var toReplace = space.HumanOfTag(Human.Dahan).ToArray();
		foreach(var original in toReplace ) {
			var stubbornDahan = MakeStubborn(original);
			_solidToNormalMap[stubbornDahan] = original;
			ReplaceAll(space, original, stubbornDahan);
		}
		// add mod to space
		space.Init(this,1);
	}

	void IModifyAddingToken.ModifyAdding(AddingTokenArgs args) {
		if( args.Token is HumanToken healthToken && healthToken.Class == Human.Dahan ) {
			var stubbornDahan = MakeStubborn(healthToken);
			_solidToNormalMap[stubbornDahan] = healthToken;
			args.Token = stubbornDahan;
		}
	}

	static HumanToken MakeStubborn(HumanToken healthToken) => healthToken.ChangeImg(Img.Dahan_Solid);

	void IModifyRemovingToken.ModifyRemoving( RemovingTokenArgs args ) {
		if(	args.Token.Class == Human.Dahan ) args.Count = 0;
		ActionScope.Current.Log(new Log.Debug("Stuborn Solidity stopping Dahan from being changed."));
	}

	void IModifyDahanDamage.Modify( DamagingTokens notification ) => notification.TokenCountToReceiveDamage = 0;

	void ICleanupSpaceWhenTimePasses.EndOfRoundCleanup(Space space) {
		foreach(var (solid,normal) in _solidToNormalMap.Select(p => (p.Key, p.Value)))
			ReplaceAll( space, solid, normal );
	}

	void ReplaceAll( Space space, HumanToken oldType, HumanToken newType) {
		int count = space[oldType];
		if( count == 0 ) return;
		space.Init(oldType, 0);
		space.Adjust(newType, count);
	}

	// allows for multiple Dahan healths that may not heal at end of round.
	Dictionary<HumanToken, HumanToken> _solidToNormalMap = new();

}