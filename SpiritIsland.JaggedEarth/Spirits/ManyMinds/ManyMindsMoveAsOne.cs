using System.Linq;

namespace SpiritIsland.JaggedEarth {

	public class ManyMindsMoveAsOne : Spirit {

		public const string Name = "Many Minds Move as One";

		public override string Text => Name;

		public override SpecialRule[] SpecialRules => new SpecialRule[]{ FlyFastAsThought, AJoiningOfSwarmsAndFlocks };

		// !!! A Joining of... is not yet implemented
		static readonly SpecialRule AJoiningOfSwarmsAndFlocks = new SpecialRule("A Joining of Swarms and Flocks","Your presence may also count as beast. If something change a beast that is your presence, it affects 2 of your presence there.");
		static readonly SpecialRule FlyFastAsThought = new SpecialRule("Fly Fast as Thought","When you Gather or Push Beast, they may come from or go to lands up to 2 distant.");


		protected override void InitializeInternal( Board board, GameState gameState ) {
			// Put 1 presence and 1 beast on yoru starting borad, in a land with beast.
			var land = board.Spaces.First(x=>gameState.Tokens[x].Beasts.Any);

			Presence.PlaceOn(land, gameState);
			gameState.Tokens[land].Beasts.Count++;
		}

		static Track CardBoost() {
			return new Track( "Pay2ForExtraPlay" ) { Action = new Pay2EnergyToGainAPowerCard() };
		}

		public ManyMindsMoveAsOne()
			:base(
				new ManyMindsPresence(
					new PresenceTrack(Track.Energy0,Track.Energy1,Track.MkElement(Element.Air),Track.Energy2,Track.MkElement(Element.Animal),Track.Energy3,Track.Energy4),
					new PresenceTrack(Track.Card1,Track.Card2,CardBoost(),Track.Card3,Track.Card3,Track.Card4,Track.Card5)
				)
				 ,PowerCard.For<ADreadfulTideOfScurryingFlesh>()
				 ,PowerCard.For<BoonOfSwarmingBedevilment>()
				 ,PowerCard.For<EverMultiplyingSwarm>()
				 ,PowerCard.For<GuideTheWayOnFeatheredWings>()
				 ,PowerCard.For<PursueWithScratchesPecksAndStings>()
			) {
			// Growth
			growthOptionGroup = new GrowthOptionGroup(
				new GrowthOption(new ReclaimAll(),new DrawPowerCard()),
				new GrowthOption(new PlacePresence(1), new PlacePresence(0)),
				new GrowthOption(new PlacePresenceAndBeast(),new GainEnergy(1), new Gather1Beast())
			);

			// Innates
			InnatePowers = new InnatePower[] {
				InnatePower.For<TheTeemingHostArrives>(),
				InnatePower.For<BesetAndConfoundTheInvaders>()
			};

		}

		public override TokenPusher PushFactory( TargetSpaceCtx ctx ) => new BeastPusher(ctx);
		public override TokenGatherer GatherFactory( TargetSpaceCtx ctx ) => new BeastGatherer(ctx);

	}

	class ManyMindsPresence : SpiritPresence {
		public ManyMindsPresence(PresenceTrack energy, PresenceTrack cards ) : base( energy, cards ) {}

		public override void PlaceOn( Space space, GameState gs ) {
			base.PlaceOn( space, gs );
		}

		public override void RemoveFrom( Space space, GameState gs ) {
			base.RemoveFrom( space, gs );
		}

	}

	// Track Tokens Moved / Destroyed

	// When SS created, Add Beast
	// When SS destroyed, Remove Beast

	// When Beast destroyed, Optional destroy Presence
	// When Beast moved, optionally move presence


}
