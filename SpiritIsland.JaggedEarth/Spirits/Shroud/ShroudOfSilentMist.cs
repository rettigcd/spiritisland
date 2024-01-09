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
		Action = new MovePresence(1),
		Icon = new IconDescriptor { BackgroundImg = Img.MovePresence }
	};


	public override SpecialRule[] SpecialRules => new SpecialRule[]{ 
		GatherPowerFromTheCoolAndDark, 
		MistsShiftAndFlow.Rule, 
		SlowAndSilentDeathHealer.Rule,
	};

	public ShroudOfSilentMist():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack(1, Track.Energy0,Track.Energy1,Track.WaterEnergy,Track.Energy2,Track.AirEnergy),
			new PresenceTrack(1, Track.Card1,Track.Card2,MovePresence,Track.MoonEnergy,Track.Card3,Track.Card4,Track.CardReclaim1,Track.Card5)
		)
		,new GrowthTrack(
			new GrowthOption( new ReclaimAll(), new GainPowerCard() ),
			new GrowthOption( new PlacePresence( 0 ), new PlacePresence( 0 ) ),
			new GrowthOption( new GainPowerCard(), new PlacePresence( 3, Filter.Mountain, Filter.Wetland ) )
		)
		, PowerCard.For(typeof(FlowingAndSilentFormsDartBy))
		,PowerCard.For(typeof(UnnervingPall))
		,PowerCard.For(typeof(DissolvingVapors))
		,PowerCard.For(typeof(TheFogClosesIn))
	) {
		InnatePowers = new InnatePower[] {
			InnatePower.For(typeof(SuffocatingShroud)), 
			InnatePower.For(typeof(LostInTheSwirlingHaze))
		};
	}

	bool _gainedCoolEnergyThisTurn = false;

	protected override void InitializeInternal( Board board, GameState gameState ) {

		gameState.Healer = new SlowAndSilentDeathHealer(this);

		// Place presence in:
		// (a) Highest # mountains,
		board.Spaces.Where(s => s.IsMountain).Last().Tokens.Setup(Presence.Token, 1);
		// (b) highest # wetlands
		board.Spaces.Where(s => s.IsWetland).Last().Tokens.Setup(Presence.Token, 1);

	}

	#region IRunWhenTimePasses

	public override async Task TimePasses( GameState gameState ) {
		await base.TimePasses( gameState );

		_gainedCoolEnergyThisTurn = false;

		static bool SpaceHasDamagedInvaders( SpaceState ss ) => ss.InvaderTokens().Any( i => i.RemainingHealth < i.FullHealth );

		// During Time Passes:
		SpaceState[] myLandsWithDamagedInvaders = Presence.Lands.Tokens().Where( SpaceHasDamagedInvaders ).ToArray();

		// 1 fear (max 5) per land of yours with Damaged Invaders.
		int remaining = 5;
		foreach(var ss in myLandsWithDamagedInvaders) {
			ss.AddFear(1);
			if(--remaining == 0) break;
		}

		// Gain 1 Energy per 3 lands of yours with Damaged Invaders.
		Energy += myLandsWithDamagedInvaders.Length / 3;

	}
	#endregion IRunWhenTimePasses

	#region Draw Cards (Gather from the Cool And Dark)

	protected override async Task<DrawCardResult> DrawInner( PowerCardDeck deck, int numberToDraw, int numberToKeep, bool forgetACard ) {
		var result = await base.DrawInner( deck, numberToDraw, numberToKeep, forgetACard );
		CheckForCoolEnergy( result.Selected );
		return result;
	}

	void CheckForCoolEnergy(PowerCard card ) {
		if(_gainedCoolEnergyThisTurn) return;
		if(card.Elements[Element.Fire]>0) return;
		Energy++;
		_gainedCoolEnergyThisTurn = true;
	}

	#endregion

	public override async Task<Space> TargetsSpace( 
		string prompt,
		IPreselect preselect,
		TargetingSourceCriteria sourceCriteria, 
		params TargetCriteria[] targetCriteria
	) {
		// we no-longer have a cood way to do this.
		//if(!Presence.CanMove)
		//	return await base.TargetsSpace( prompt, preselect, sourceCriteria, targetCriteria );

		MistsShiftAndFlow x = new MistsShiftAndFlow(this,prompt,sourceCriteria,targetCriteria);
		return (await x.TargetAndFlow()).Space;
	}

	protected override object CustomMementoValue { 
		get => _gainedCoolEnergyThisTurn;
		set => _gainedCoolEnergyThisTurn = (bool)value;
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
		if( !spirit.Presence.IsOn( tokens ) )
			base.HealSpace( tokens );
	}

}