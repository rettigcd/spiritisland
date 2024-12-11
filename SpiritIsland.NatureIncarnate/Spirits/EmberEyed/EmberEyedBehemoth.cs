
namespace SpiritIsland.NatureIncarnate;

public class EmberEyedBehemoth : Spirit {

	public const string Name = "Ember-Eyed Behemoth";

	public override string SpiritName => Name;

	class DiscardPowerCardWithFireFromHand : DiscardCards {
		public DiscardPowerCardWithFireFromHand() 
			: base( "Discard a Power Card with fire", spirit => spirit.Hand.Where( card => 0 < card.Elements[Element.Fire] ) ) 
		{}
	}

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
		InnatePowers = [
			InnatePower.For(typeof(SmashStompAndFlatten))
		];
		SpecialRules = [TheBehemothRises.Rule, UnrelentingStrides.Rule];

		AvailableActionMods.Add(new UnrelentingStrides(this));
	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Put 2 presenct + Incarna in highest # wetlands that is adjacent to ANY Jungle
		Space start = board.Spaces.Last( s => s.Adjacent_Existing.Any(x=>x.IsJungle ) ).ScopeSpace;
		start.Setup(Presence.Token, 2);
		start.Setup(Incarna,1);
	}

	public override async Task DoGrowth( GameState gameState ) {
		await base.DoGrowth( gameState );

		// Remove 4th growth after it has been used
		if(GrowthTrack.Groups.Length == 4 && Incarna.Empowered) {
			GrowthTrack = new( GrowthTrack.Groups.Take( 3 ).ToArray() );
			ActionScope.Current.Log( new Log.LayoutChanged( $"Fourth growth option removed from {Name}." ) );
		}
	}

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

