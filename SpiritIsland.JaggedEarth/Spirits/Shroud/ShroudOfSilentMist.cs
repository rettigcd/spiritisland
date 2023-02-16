namespace SpiritIsland.JaggedEarth;

// Slow And Silent Death

public class ShroudOfSilentMist : Spirit {

	public const string Name = "Shroud of Silent Mist";

	public override string Text => Name;

	static readonly SpecialRule GatherPowerFromTheCoolAndDark = new SpecialRule(
		"Gather Power from the Cool and Dark",
		"Once a turn, when you Gain a Power Card without fire, gain 1 Energy"
	);

	public static Track MovePresence => new Track( "Moveonepresence.png" ){ // Same as Downpour
		Action=new MovePresence(1),
		Icon = new IconDescriptor { BackgroundImg = Img.MovePresence }
	};


	public override SpecialRule[] SpecialRules => new SpecialRule[]{ 
		GatherPowerFromTheCoolAndDark, 
		MistsShiftAndFlow.Rule, 
		SlowAndSilentDeathHealer.Rule,
	};

	public ShroudOfSilentMist():base(new SpiritPresence(
			new PresenceTrack(1, Track.Energy0,Track.Energy1,Track.WaterEnergy,Track.Energy2,Track.AirEnergy),
			new PresenceTrack(1, Track.Card1,Track.Card2,MovePresence,Track.MoonEnergy,Track.Card3,Track.Card4,Track.CardReclaim1,Track.Card5)
		)
		,PowerCard.For<FlowingAndSilentFormsDartBy>()
		,PowerCard.For<UnnervingPall>()
		,PowerCard.For<DissolvingVapors>()
		,PowerCard.For<TheFogClosesIn>()
	) {
		this.GrowthTrack = new GrowthTrack(
			new GrowthOption( new ReclaimAll(), new DrawPowerCard() ),
			new GrowthOption( new PlacePresence(0), new PlacePresence(0) ),
			new GrowthOption( new DrawPowerCard(), new PlacePresence(3,Target.Mountain, Target.Wetland ) )
		);


		this.InnatePowers = new InnatePower[] {
			InnatePower.For<SuffocatingShroud>(),
			InnatePower.For<LostInTheSwirlingHaze>()
		};
	}

	bool gainedCoolEnergyThisTurn = false;

	protected override void InitializeInternal( Board board, GameState gameState ) {

		gameState.Healer = new SlowAndSilentDeathHealer(this);

		// Place presence in:
		// (a) Highest # mountains,
		board.Spaces.Where(s => s.IsMountain).Last().Tokens.Adjust(Presence.Token, 1);
		// (b) highest # wetlands
		board.Spaces.Where(s => s.IsWetland).Last().Tokens.Adjust(Presence.Token, 1);

		gameState.TimePasses_WholeGame += GameState_TimePasses_WholeGame;
	}

	Task GameState_TimePasses_WholeGame( GameState gs ) {
		gainedCoolEnergyThisTurn = false;

		static bool SpaceHasDamagedInvaders( SpaceState ss ) => ss.InvaderTokens().Any( i=>i.RemainingHealth<i.FullHealth );

		// During Time Passes:
		int myLandsWithDamagedInvaders = this.Presence.SpaceStates.Count( SpaceHasDamagedInvaders );

		// 1 fear (max 5) per land of yours with Damaged Invaders.
		gs.Fear.AddDirect(new FearArgs( Math.Min(5,myLandsWithDamagedInvaders) ) );

		// Gain 1 Energy per 3 lands of yours with Damaged Invaders."
		Energy += (myLandsWithDamagedInvaders / 3);

		return Task.CompletedTask;
	}

	#region Draw Cards (Gather from the Cool And Dark)

	public override async Task<DrawCardResult> Draw( GameState gameState ) {
		var result = await base.Draw( gameState );
		CheckForCoolEnergy( result.Selected );
		return result;
	}

	public override async Task<DrawCardResult> DrawMajor( GameState gameState, bool forgetCard = true, int numberToDraw = 4, int numberToKeep=1 ) {
		var result = await base.DrawMajor( gameState, forgetCard, numberToDraw, numberToKeep );
		CheckForCoolEnergy( result.Selected );
		return result;
	}

	public override async Task<DrawCardResult> DrawMinor( GameState gameState, int numberToDraw = 4, int numberToKeep = 1 ) {
		var result = await base.DrawMinor( gameState, numberToDraw, numberToKeep );
		CheckForCoolEnergy( result.Selected );
		return result;
	}
	void CheckForCoolEnergy(PowerCard card ) {
		if(gainedCoolEnergyThisTurn) return;
		if(card.Elements[Element.Fire]>0) return;
		Energy++;
		gainedCoolEnergyThisTurn = true;
	}

	#endregion

	public override async Task<Space> TargetsSpace( 
		SelfCtx ctx,
		string prompt,
		IPreselect _,
		TargetingSourceCriteria sourceCriteria, 
		params TargetCriteria[] targetCriteria
	) {
		var x = new MistsShiftAndFlow(ctx,prompt,sourceCriteria,targetCriteria);
		return (await x.TargetAndFlow()).Space;
	}

}

class SlowAndSilentDeathHealer : Healer {

	readonly ShroudOfSilentMist spirit;

	public SlowAndSilentDeathHealer(ShroudOfSilentMist spirit ) { this.spirit = spirit; }

	public static readonly SpecialRule Rule = new SpecialRule(
		"Slow and Silent Death",
		"Invaders and dahan in your lands don't heal Damage.  During Time PAsses: 1 fear (max 5) per land of yours with Damaged Invaders.  Gain 1 Energy per 3 lands of yours with Damaged Invaders."
	);

	//public override void HealAll( GameState gs ) {
	//	// Invaders and dahan in your lands don't heal Damage.
	//	skipHealSpaces.UnionWith( spirit.Presence.Spaces );
	//	base.HealAll( gs );
	//}

	public override void HealSpace( SpaceState tokens ) {
		if( !tokens.Has(spirit.Token) )
			base.HealSpace( tokens );
	}

}