namespace SpiritIsland.JaggedEarth;

public class VoiceOfCommand {

	[MajorCard("Voice of Command",3,Element.Sun,Element.Air), Fast, FromSacredSite(1,Filter.Dahan)]
	[Instructions( "1 Damage per Dahan / Explorer, to Town / City only. Defend 2. During Ravage actions, Explorer fight alongside Dahan. (Deal / take damage at the same time, and to / from the same sources.) -If you have- 3 Sun, 2 Air: First, Gather up to 2 Explorer / Town / Dahan" ), Artist( Artists.LucasDurham )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {

		// if you have 3 sun 2 air:
		if( await ctx.YouHave("3 sun,2 air"))
			// first gather up to 2 explorer/town/dahan
			await ctx.GatherUpTo(2, Human.Explorer_Town.Plus(Human.Dahan) );

		// 1 damage per dahan/explorer, to towns/cities only
		await ctx.DamageInvaders( 
			ctx.Dahan.CountAll + ctx.Tokens.Sum( Human.Explorer ),
			Human.Town_City
		);

		// Defend 2.
		ctx.Defend(2);

		// During Ravage Actions, explorer fight alongside dahan. (Deal/take Damage at the same time, and to/from the same source.)
//		var cfg = ctx.Tokens.RavageBehavior;
//		cfg.IsAttacker = (token) => token.Class.IsOneOf( Human.Town_City );
//		cfg.IsDefender = (token) => token.Class.IsOneOf( Human.Explorer, Human.Dahan);
		ctx.Tokens.Init(new RavageConfigToken(
			space => {
				foreach(HumanToken orig in space.HumanOfTag( Human.Explorer ).ToArray()) {
					AdjustRavageSide( space, orig, RavageSide.Defender, RavageOrder.DahanTurn );
				}
			},
			space => {
				foreach(HumanToken orig in space.HumanOfAnyTag( Human.Explorer ).ToArray())
					AdjustRavageSide( space, orig, RavageSide.Attacker, RavageOrder.InvaderTurn );
			}
		),1);

	}
	static void AdjustRavageSide( SpaceState space, HumanToken orig, RavageSide side, RavageOrder order ) {
		space.Init( orig.SetRavageSide( side ).SetRavageOrder( order ), space[orig] );
		space.Init( orig, 0 );
	}


}

public class RavageConfigToken( Action<SpaceState> _setup, Action<SpaceState> _teardown ) 
	: BaseModEntity, 
	IConfigRavages, 
	IEndWhenTimePasses
{
	void IConfigRavages.Config( SpaceState space ) {
		// Token Reduces Attack of invaders by 1
		_setup( space );

		// At end of Action, invaders are are restored to original.
		ActionScope.Current.AtEndOfThisAction( _ => _teardown( space ) );

	}
}