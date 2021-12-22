using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

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
			Growth = new GrowthOptionGroup(
				new GrowthOption( new ReclaimAll(), new DrawPowerCard(), new GainEnergy(1)),
				new GrowthOption( new PlacePresence(2,Target.TownCityOrBlight), new PlacePresence(2,Target.TownCityOrBlight)),
				new GrowthOption( new DrawPowerCard(), new PlacePresenceOrDisease(), new GainEnergy(1))
			);
			InnatePowers = new InnatePower[] {
				InnatePower.For<EpidemicsRunRampant>(),
				InnatePower.For<SavageRevenge>()
			};
		}

		public override string Text => Name;

		public override SpecialRule[] SpecialRules => new SpecialRule[] {  
			TerrorOfASlowlyUnfoldingPlague, 
			LingeringPestilence.Rule, 
			WreakVengeanceForTheLandsCorruption.Rule
		};

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// Put 2 presence ontyour starting board:
			// 1 in a land with blight.
			Presence.PlaceOn(board.Spaces.First(s=>gameState.Tokens[s].Blight.Any), gameState);
			// 1 in a Wetland without dahan
			Presence.PlaceOn(board.Spaces.First(s=>s.IsWetland && !gameState.Tokens[s].Dahan.Any), gameState);

			gameState.InvaderEngine.StopBuildWithDiseaseBehavior = TerrorOfASlowyUnfoldingPlague_PreBuild_DiseaseChecker;

		}

		static SpecialRule TerrorOfASlowlyUnfoldingPlague => new SpecialRule(
			"The Terror of a Slowly Unfolding Plague",
			"When disease would prevent a Build on a board with your presence, you may let the Build happen (removing no disease).  If you do, 1 fear."
		);

		async Task<bool> TerrorOfASlowyUnfoldingPlague_PreBuild_DiseaseChecker( TokenCountDictionary tokens, GameState gs ) {
			var disease = tokens.Disease;
			bool stoppedByDisease = disease.Any 
				&& await this.UserSelectsFirstText($"Build pending on {tokens.Space.Label}.", "Stop build, -1 Disease", "+1 Fear, keep Disease ");

			if( stoppedByDisease )
				await disease.Remove(1, RemoveReason.UsedUp);
			else 
				gs.Fear.AddDirect(new FearArgs { FromDestroyedInvaders = false, count=1, space = tokens.Space } );
			return stoppedByDisease;
		}

		public override TokenBinding ConstructBadlands( TargetSpaceCtx ctx ) {
			return new WreakVengeanceForTheLandsCorruption( ctx.Tokens );
		}

	}

}
