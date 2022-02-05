﻿namespace SpiritIsland.JaggedEarth;

public class ManyMindsMoveAsOne : Spirit {

	public const string Name = "Many Minds Move as One";

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[]{ FlyFastAsThought, AJoiningOfSwarmsAndFlocks };

	static readonly SpecialRule AJoiningOfSwarmsAndFlocks = new SpecialRule("A Joining of Swarms and Flocks","Your presence may also count as beast. If something change a beast that is your presence, it affects 2 of your presence there.");
	static readonly SpecialRule FlyFastAsThought = new SpecialRule("Fly Fast as Thought","When you Gather or Push Beast, they may come from or go to lands up to 2 distant.");

	static Track CardBoost => new Track( "Pay2ForExtraPlay" ) { 
		Action = new Pay2EnergyToGainAPowerCard(),
		Icon = new IconDescriptor { ContentImg = Img.GainCard,
			Super = new IconDescriptor { BackgroundImg = Img.Coin, Text= "—2" }
		}
	};

	public ManyMindsMoveAsOne()
		:base(
			new ManyMindsPresence(
				new PresenceTrack(Track.Energy0,Track.Energy1,Track.MkEnergy(Element.Air),Track.Energy2,Track.MkEnergy(Element.Animal),Track.Energy3,Track.Energy4),
				new PresenceTrack(Track.Card1,Track.Card2,CardBoost,Track.Card3,Track.Card3,Track.Card4,Track.Card5)
			)
				,PowerCard.For<ADreadfulTideOfScurryingFlesh>()
				,PowerCard.For<BoonOfSwarmingBedevilment>()
				,PowerCard.For<EverMultiplyingSwarm>()
				,PowerCard.For<GuideTheWayOnFeatheredWings>()
				,PowerCard.For<PursueWithScratchesPecksAndStings>()
		) {
		// Growth
		Growth = new Growth(
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

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Put 1 presence and 1 beast on yoru starting borad, in a land with beast.
		var land = board.Spaces.First(x=>gameState.Tokens[x].Beasts.Any);

		(Presence as ManyMindsPresence).Watch(gameState,this);

		Presence.PlaceOn(land, gameState);
		gameState.Tokens[land].Beasts.Init(1);

	}


	public override TokenPusher PushFactory( TargetSpaceCtx ctx ) => new BeastPusher(ctx);
	public override TokenGatherer GatherFactory( TargetSpaceCtx ctx ) => new BeastGatherer(ctx);

}