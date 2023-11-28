namespace SpiritIsland.FeatherAndFlame;

public class FinderPresence : SpiritPresence {

	#region Custom Tracks

	static Track OneMoon => Track.MkEnergy(1,Element.Moon);
	static Track TwoWater => Track.MkEnergy(2,Element.Water);

	static Track Move1Air => new Track( "Moveonepresence.png", Element.Air ) {
		Action = new MovePresence( 1 ),
		Icon = new IconDescriptor {
			BackgroundImg = Img.MovePresence,
			Sub = new IconDescriptor { BackgroundImg = Img.Token_Air }
		}
	};

	static Track Plus2 => new Track( "+2" ) {
		Action = new GainEnergy(2),
		Icon = new IconDescriptor {
			Text = "+2",
			BackgroundImg = Img.Coin,
		}
	};

	static Track PlusOneAny => new Track( "+1,Any", Element.Any ) {
		Action = new GainEnergy(1),
		Icon = new IconDescriptor {
			Text = "+1",
			BackgroundImg = Img.Coin,
			Sub = new IconDescriptor { BackgroundImg = Img.Token_Any }
		}
	};

	static Track ExtraCardPlay => new Track( "+1 Card PLay" ) {
		Action = new PlayExtraCardThisTurn(1),
		Icon = new IconDescriptor { BackgroundImg = Img.CardPlayPlusN, Text = "+1" }
	};

	static Track OneCoinExtendRange => new Track( "+1,+1 range" ) {
		Action = new CompoundActionFactory(new GainEnergy(1), new ExtendRange(1)),
		Icon = new IconDescriptor {
			BackgroundImg = Img.Coin,
			Text = "+1",
			BigSub = new IconDescriptor { BackgroundImg = Img.PlusOneRange }
		}
	};

	#endregion


	public FinderPresence(Spirit spirit):base(spirit,
		new FinderTrack( Track.Energy0, Track.SunEnergy, OneMoon, TwoWater, Move1Air, Plus2, PlusOneAny ),
		new FinderTrack( Track.Card1, Track.EarthEnergy, ExtraCardPlay, ExtraCardPlay, OneCoinExtendRange, Track.Push1TownCity )
	) {
		var energy = ((FinderTrack)Energy).LinkedSlots;
		FinderTrack.LinkedSlot energy0    = energy[0];
		FinderTrack.LinkedSlot sunEnergy  = energy[1];
		FinderTrack.LinkedSlot oneMoon    = energy[2];
		FinderTrack.LinkedSlot twoWater   = energy[3];
		FinderTrack.LinkedSlot move1Air   = energy[4];
		FinderTrack.LinkedSlot plus2      = energy[5];
		FinderTrack.LinkedSlot plusOneAny = energy[6];

		var card = ((FinderTrack)CardPlays).LinkedSlots;
		FinderTrack.LinkedSlot card1              = card[0];
		FinderTrack.LinkedSlot earthEnergy        = card[1];
		FinderTrack.LinkedSlot extraCardPlay1     = card[2];
		FinderTrack.LinkedSlot extraCardPlay2     = card[3];
		FinderTrack.LinkedSlot oneCoinExtendRange = card[4];
		FinderTrack.LinkedSlot push1TownCity      = card[5];

		// Top row links - 4 links
		sunEnergy.TwoWay( energy0, twoWater );
		plus2.TwoWay( twoWater, plusOneAny );
		// Bottom row links - 4 links
		earthEnergy.TwoWay( card1, extraCardPlay1 );
		extraCardPlay2.TwoWay( extraCardPlay1, push1TownCity );
		// Middle Row - link up and down and across - 10 links
		oneMoon.TwoWay( sunEnergy, twoWater, move1Air, earthEnergy );
		move1Air.TwoWay( plus2, extraCardPlay2 );
		oneCoinExtendRange.TwoWay( plus2, plusOneAny, extraCardPlay2, push1TownCity );

		energy0.Reveal();
		card1.Reveal();

		base.InitEnergyAndCardPlays(); // need to rescan because spaces weren't revealed yet.
	}

}
