namespace SpiritIsland.NatureIncarnate;

public class EmberEyedBehemoth : Spirit {

	public const string Name = "Ember-Eyed Behemoth";

	public override string Text => Name;

	class DiscardPowerCardWithFireFromHand : DiscardCards {
		public DiscardPowerCardWithFireFromHand() 
			: base( "Discard a Power Card with fire", spirit => spirit.Hand.Where( card => 0 < card.Elements[Element.Fire] ) ) 
		{}
	}

	public override SpecialRule[] SpecialRules => new SpecialRule[] {
		new SpecialRule("The Behemoth Rises","You have an Incarna.  Once/turn, during any phase, you may push it or Add/Move it to any of your Sacred Sites."),
		UnrelentingStrikes_Rule
	};

	public EmberEyedBehemoth():base( 
		spirit => new SpiritPresence(spirit,
			new PresenceTrack(Track.Energy0,Track.Energy1,Track.MkEnergy(2,Element.Fire),Track.Energy3,Track.EarthEnergy,Track.MkEnergy(4,Element.Plant), Track.MkEnergy(5,Element.Fire)),
			new PresenceTrack(Track.Card1,Track.Card2,Track.Card2,Track.Card3,Track.FireEnergy,Track.Card4),
			new Incarna(spirit, "EEB", Img.EEB_Incarna, Img.EEB_Incarna_Empowered)
		)
		, new GrowthTrack(
			new GrowthGroup(
				new ReclaimAll(),
				new GainPowerCard()
			),
			new GrowthGroup(
				new PlacePresence( 3, Filter.Jungle, Filter.Presence ),
				new PlacePresence( 0 )
			),
			new GrowthGroup(
				new GainPowerCard(),
				new PlacePresence( 1 ),
				new GainEnergy( 3 ),
				new DiscardPowerCardWithFireFromHand()
			),
			new GrowthGroup(
				new ReclaimAllWithFire(),
				new EmpowerIncarna(),
				new MoveOnlyIncarna( 1 )
			)
		)
		, PowerCard.For(typeof(TerrifyingRampage))
		,PowerCard.For(typeof(BlazingIntimidation))
		,PowerCard.For(typeof(SurgingLahar))
		,PowerCard.For(typeof(ExaltationOfGraspingRoots))
	) {
		_smash = new RepeatableInnatePower( typeof( SmashStompAndFlatten ) );
		InnatePowers = new InnatePower[] { _smash };
		_behemoth = new TheBehemothRises();
	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Put 2 presenct + Incarna in highest # wetlands that is adjacent to ANY Jungle
		SpaceState start = board.Spaces.Last( s => s.Adjacent_Existing.Any(x=>x.IsJungle ) ).Tokens;
		start.Setup(Presence.Token, 2);
		start.Setup(Incarna,1);
	}

	public override async Task DoGrowth( GameState gameState ) {
		// Reset Behemoth before Growth so it can be used before growth
		_behemoth.Used = 0; _behemoth.MaxUses = 2;
		AddActionFactory( _behemoth );

		await base.DoGrowth( gameState );

		// Remove 4th growth after it has been used
		if(GrowthTrack.Groups.Length == 4 && Incarna.Empowered) {
			GrowthTrack = new( GrowthTrack.Groups.Take( 3 ).ToArray() );
			ActionScope.Current.Log( new Log.LayoutChanged( $"Fourth growth option removed from {Name}." ) );
		}

		// Reset Smash last, since it depends on Embpowered Incarna
		_smash.Used = 0;
		_smash.MaxUses = Incarna.Empowered ? 2 : 1;
	}

	public override async Task TakeActionAsync( IActionFactory factory, Phase phase ) {

		await base.TakeActionAsync( factory, phase );

		if(factory is not IHaveDynamicUseCounts ihduc) return;

		// It is easier to Restore actions than it is to prevent their removal. (I tried it the other way - so I know.)
		// If we were to prevent removal, we would either have to:
		//	 1) reimpliment the innards of TakeActionAsync - violating encapsulation / duplication principles. OR
		//	 2) override RemoveFromUnresolved...() to ignore the 'remove' call - which is super complicated because
		//	    - the method Can't tell if we are calling it explicitly or due to action being used.

		// Track and Restore Actions having Additional Uses
		++ihduc.Used;
		if(HasMoreUses( ihduc ))
			AddActionFactory( factory );

		Check_UnrelentingStrides();
	}

	#region Unrelenting Strides

	static SpecialRule UnrelentingStrikes_Rule => new SpecialRule( 
		"Unrelenting Strides", 
		"On any turn that you don't use Innate Powers, you may use The Behemoth Rises an additional time." 
	);

	void Check_UnrelentingStrides() { // special rule

		// Using Behemoth twice, removes remaining Smash
		if(_behemoth.Used == 2                      // used tiwce
			&& HasMoreUses( _smash )                // has remaining Smash
		) RemoveFromUnresolvedActions( _smash );	// remove it

		// Using Smash once, reduces limits remaining Bohemoth
		if(_smash.Used == 1                         // used once
			&& HasMoreUses( _behemoth )             // has remaining Bohemoth
		) {
			_behemoth.MaxUses = 1;                  // limit to 1
			if(!HasMoreUses( _behemoth ))               // but no-longer has more
				RemoveFromUnresolvedActions( _behemoth ); // must remove it
		}
	}

	static bool HasMoreUses( IHaveDynamicUseCounts x ) => x.Used < x.MaxUses;

	readonly RepeatableInnatePower _smash;
	readonly TheBehemothRises _behemoth;

	#endregion

	protected override object CustomMementoValue { 
		get => GrowthTrack.Groups;
		set {
			var options = (GrowthGroup[])value;
			if(options.Length != GrowthTrack.Groups.Length) {
				GrowthTrack = new( options );
				ActionScope.Current.Log( new Log.LayoutChanged( $"Rewind >> Restoring growth options for {Name}." ) );
			}
			Incarna.Empowered = options.Length == 3;
		}
	}

}
