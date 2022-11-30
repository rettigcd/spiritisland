namespace SpiritIsland.JaggedEarth;

public class VengeanceAsABurningPlague : Spirit {

	public const string Name = "Vengeance as a Burning Plague";

	public VengeanceAsABurningPlague() : base(
		new VengeancePresence(
			new PresenceTrack(Track.Energy1,Track.Energy2,Track.AnimalEnergy,Track.Energy3,Track.Energy4),
			new PresenceTrack(Track.Card1, Track.Card2, Track. FireEnergy, Track.Card2, Track.Card3, Track.Card3, Track.Card4)
		)
		,PowerCard.For<FetidBreathSpreadsInfection>()
		,PowerCard.For<FieryVengeance>()
		,PowerCard.For<Plaguebearers>()
		,PowerCard.For<StrikeLowWithSuddenFevers>()
	) {
		GrowthTrack = new GrowthTrack(
			new GrowthOption( new ReclaimAll(), new DrawPowerCard(), new GainEnergy(1)),
			new GrowthOption( new PlacePresence(2,Target.TownOrCity, Target.Blight ), new PlacePresence(2, Target.TownOrCity, Target.Blight ) ),
			new GrowthOption( new DrawPowerCard(), new PlacePresenceOrDisease(), new GainEnergy(1))
		);
		InnatePowers = new InnatePower[] {
			InnatePower.For<EpidemicsRunRampant>(),
			InnatePower.For<SavageRevenge>()
		};
	}

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] {  
		TerrorOfASlowlyUnfoldingPlague.Rule,
		LingeringPestilence.Rule,
		WreakVengeanceForTheLandsCorruption.Rule
	};

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Put 2 presence ontyour starting board:
		// 1 in a land with blight.
		Presence.Adjust(gameState.Tokens[board.Spaces.First(s=>gameState.Tokens[s].Blight.Any)], 1);
		// 1 in a Wetland without dahan
		Presence.Adjust(gameState.Tokens[board.Spaces.First(s=>s.IsWetland && !gameState.Tokens[s].Dahan.Any)], 1);

		gameState.GetBuildEngine = () => new TerrorOfASlowlyUnfoldingPlague(this);

	}

	// BuildEngine
	class TerrorOfASlowlyUnfoldingPlague : BuildEngine  {

		static public SpecialRule Rule => new SpecialRule(
			"The Terror of a Slowly Unfolding Plague",
			"When disease would prevent a Build on a board with your presence, you may let the Build happen (removing no disease).  If you do, 1 fear."
		);

		readonly VengeanceAsABurningPlague spirit;
		public TerrorOfASlowlyUnfoldingPlague(VengeanceAsABurningPlague spirit) { this.spirit = spirit; }
		protected override async Task<bool> StopBuildWithDiseaseBehavior() {
			var disease = tokens.Disease;
			bool stoppedByDisease = disease.Any 
				&& await spirit.UserSelectsFirstText($"Build pending on {tokens.Space.Label}.", "Stop build, -1 Disease", "+1 Fear, keep Disease ");

			if( stoppedByDisease )
				await disease.Bind(actionId).Remove(1, RemoveReason.UsedUp);
			else 
				gameState.Fear.AddDirect(new FearArgs { FromDestroyedInvaders = false, count=1, space = tokens.Space } );
			return stoppedByDisease;

		}

	}

	public override SelfCtx Bind( GameState gameState, UnitOfWork actionId, Cause cause = default ) 
		=> new VengenceCtx( this, gameState, actionId, cause );

}

public class VengenceCtx : SelfCtx {
	public VengenceCtx( Spirit spirit, GameState gameState, UnitOfWork actionId, Cause cause ) : base( spirit, gameState, actionId, cause ) { }
	public override TargetSpaceCtx Target( Space space ) => new VengenceSpaceCtx( this, space );
}

public class VengenceSpaceCtx : TargetSpaceCtx {
	public VengenceSpaceCtx( VengenceCtx ctx, Space target):base( ctx, target ) { }
	public override TokenBinding Badlands => new WreakVengeanceForTheLandsCorruption( Tokens, CurrentActionId );
}
