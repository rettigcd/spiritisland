namespace SpiritIsland.NatureIncarnate;

public class DancingPresence : SpiritPresence {

	#region Custome Track spaces

	static ImpendingEnergyTrack One => new ImpendingEnergyTrack( 1, 1 ) {
		Icon = new IconDescriptor { 
			BackgroundImg = Img.Coin, 
			Text = "1",
			Sub = new IconDescriptor { 
				BackgroundImg = Img.Icon_ImpendingCard,
				ContentImg = Img.Coin,
				Text = "1"
			}
		}
	};

	static ImpendingEnergyTrack TwoImpendingEnergy => new ImpendingEnergyTrack( 0, 2 ) {
		Icon = new IconDescriptor {
			BackgroundImg = Img.Icon_ImpendingCard,
			ContentImg = Img.Coin,
			Text = "2",
		}
	};

	static Track FourAny => Track.MkEnergy(4,Element.Any);

	static Track MoonFire => new Track("moon/fire",Element.Moon,Element.Fire){
		Icon = new IconDescriptor {
			ContentImg = Img.Token_Moon,
			ContentImg2 = Img.Token_Fire,
		}
	};

	static Track AdditionalImpending => new Track("+1 Impending" ) {
		Icon = new IconDescriptor {
			Super = new IconDescriptor {  Text = "+1" },
			Sub = new IconDescriptor{ BackgroundImg = Img.Icon_ImpendingCard },
		},
		Action = new BoostImpendingPlays()
	};

	static Track GatherDahan => new Track( "Gather 1 Dahan" ) {
		Icon = new IconDescriptor { BackgroundImg = Img.Land_Gather_Dahan },
		Action = new Gather1Dahan(),
	};

	static Track MovePresence => new Track( "Moveonepresence.png" ) {
		Action = new MovePresence( 1 ),
		Icon = new IconDescriptor { BackgroundImg = Img.MovePresence }
	};

	class BoostImpendingPlays : IActionFactory {
		public string Name => "Bost Impending Plays";

		public string Text => Name;
		public bool CouldActivateDuring( Phase speed, Spirit spirit ) => true; 
		public Task ActivateAsync( SelfCtx ctx ) {
			if(ctx.Self is DancesUpEarthquakes due)
				++due.BonusImpendingPlays;
			return Task.CompletedTask;
		}

	}

	#endregion Custome Track spaces

	public DancingPresence() 
		:base(
			new PresenceTrack( One, MovePresence, Track.Energy2, AdditionalImpending, Track.Energy3, TwoImpendingEnergy, FourAny ),
			new PresenceTrack( Track.Card2, GatherDahan, MoonFire, AdditionalImpending, Track.EarthEnergy, Track.Card3, Track.Card4 )
		) {
		}
}

public class ImpendingEnergyTrack : Track {
	public ImpendingEnergyTrack( int energy, int impendingEnergy ) : base( $"{energy}/{impendingEnergy}" ) {
		Energy = energy;
		ImpendingEnergy = impendingEnergy;
	}
	public int ImpendingEnergy { get; }
}