namespace SpiritIsland.JaggedEarth;

// UI - draw more than 2 innates 

public class StarlightSeeksItsForm : Spirit {

	#region Presence Track Slot Generators

	static readonly IconDescriptor Plus1Coin = new IconDescriptor {
		BackgroundImg = Img.Coin,
		Text = "+1"
	};

	static Track Track_Gain1Energy => new Track( "" ) {
		Icon = new IconDescriptor { Super = Plus1Coin }
	};

	const string PickElementName = "Pick element";
	static Track Track_PickElement { 
		get {
			return new Track( PickElementName ) {
				Icon = new IconDescriptor {
					BackgroundImg = Img.Coin,
					ContentImg = Img.Starlight_AssignElement
				}
			};
		}
	}

	readonly static Track NewGrowth1 = new Track( "so1" ) {  
		Icon = new IconDescriptor { BackgroundImg = Img.Starlight_GrowthOption1 },
	};
	readonly static Track NewGrowth2 = new Track( "so2" ) {  
		Icon = new IconDescriptor { BackgroundImg = Img.Starlight_GrowthOption2 },
	};
	readonly static Track NewGrowth3 = new Track( "so3" ) {
		Icon = new IconDescriptor {
			ContentImg = Img.Starlight_AssignElement,
			Super = Plus1Coin,
			BigSub = new IconDescriptor { BackgroundImg = Img.Starlight_GrowthOption3 },
		},
	};
	readonly static Track NewGrowth4 = new Track( "so4" ) {  
		Icon = new IconDescriptor { 
			BigSub = new IconDescriptor{ BackgroundImg = Img.Starlight_GrowthOption4 },
			Super = Plus1Coin,
		},
	};

	#endregion

	public const string Name = "Starlight Seeks Its Form";

	public override string Text => Name;

	static readonly SpecialRule GrowthBegetsGrowth = new SpecialRule(
		"Growth Begets Growth",
		"4 of your presence tracks unlock growth options. Upon emptying such a track, pick one of tis two Growth choices to be immediately available.  The other stays unavailable for the rest of the game. After you add presence from a space marged +1, gain 1 Energy."
	);

	static readonly SpecialRule SlowlyCoalescingNature = new SpecialRule(
		"Slowly Coalescing Nature",
		"After revealing a snowflake, place 1 Element Marger of your choice on it.  That element is permanent and is constantly available."
	);

	public override SpecialRule[] SpecialRules => new SpecialRule[]{ GrowthBegetsGrowth, SlowlyCoalescingNature };


	public StarlightSeeksItsForm():base(
		spirit => new SpiritPresence( spirit,
			new CompoundPresenceTrack(
				new PresenceTrack( Track.Energy1, Track.Energy2, Track_PickElement, Track.Energy4),
				new PresenceTrack( 0, NewGrowth1 ),
				new PresenceTrack( 0, Track_Gain1Energy, NewGrowth3 )
			),
			new CompoundPresenceTrack(
				new PresenceTrack( Track.Card2, Track_PickElement, Track_PickElement, Track.Card3 ),
				new PresenceTrack( 0, NewGrowth2 ),
				new PresenceTrack( 0, Track_Gain1Energy, NewGrowth4 )
			)
		)
		,PowerCard.For(typeof(BoonOfReimagining))
		,PowerCard.For(typeof(GatherTheScatteredLightOfStars))
		,PowerCard.For(typeof(PeaceOfTheNighttimeSky))
		,PowerCard.For(typeof(ShapeTheSelfAnew))
	) {
		GrowthTrack = new GrowthTrack( 3,
			new GrowthOption( new ReclaimN() ),
			new GrowthOption( new PlacePresence(0) ),
			new GrowthOption( new GainEnergy(1) ),
			new GrowthOption( new MovePresence(3) )
		);

		InnatePowers = new InnatePower[] {
			InnatePower.For(typeof(AirMovesEarthEndures)),
			InnatePower.For(typeof(FireBurnsWaterSoothes)),
			InnatePower.For(typeof(SiderealGuidance)),
			InnatePower.For(typeof(WoodSeeksGrowthHumansSeekFreedom)),
			InnatePower.For(typeof(StarsBlazeInTheDaytimeSky)),
		};
		Presence.Energy.TrackRevealed.Add( EnergyRevealed );
		Presence.CardPlays.TrackRevealed.Add( CardPlaysRevealed );

	}
	SelfCtx Bind() => ActionIsMyPower ? BindMyPowers() : BindSelf();
	async Task EnergyRevealed( TrackRevealedArgs args ) {
		if(args.Track == Track_Gain1Energy)
			Energy++;
		if(args.Track.Text == PickElementName) {
			var el = await this.SelectElementEx( "Select permanent element for this slot.", ElementList.AllElements );
			args.Track.Elements = new Element[] { el };
			Elements.Add(el);
			args.Track.Icon.ContentImg = el.GetTokenImg();
		}

		if(args.Track == NewGrowth1 ) {
			await new PickNewGrowthOption(
				new GrowthOption( new ReclaimHalf() ),
				new GrowthOption( new GainPowerCard(), new MovePresence( 1 ) )
			).ActivateAsync( Bind() );
		}
		if(args.Track == NewGrowth3 ) {
			var ctx = Bind();
			Energy++;
			await AssignNewElementToTrack( ctx, NewGrowth3 );
			await new PickNewGrowthOption(
				new GrowthOption( new ReclaimAll() ),
				new GrowthOption( new GainPowerCard(), new GainEnergy( 1 ) )
			).ActivateAsync( ctx );
		}
	}

	static async Task AssignNewElementToTrack( SelfCtx ctx, Track track ) {
		var el = await ctx.Self.SelectElementEx( "Select permanent element for this slot.", ElementList.AllElements );
		track.Elements = new Element[] { el };
		ctx.Self.Elements.Add(el);
		track.Icon.ContentImg = el.GetTokenImg();
	}


	async Task CardPlaysRevealed( TrackRevealedArgs args) {
		if(args.Track == Track_Gain1Energy)
			Energy++;
		if(args.Track.Text == PickElementName) {
			var el = await this.SelectElementEx( "Select permanent element for this slot.", ElementList.AllElements );
			args.Track.Elements = new Element[] { el };
			Elements.Add(el);
			args.Track.Icon.ContentImg = el.GetTokenImg();
		}

		if(args.Track == NewGrowth2) {
			await new PickNewGrowthOption(
				new GrowthOption( new GainEnergy( 3 ) ),
				new GrowthOption( new PlayExtraCardThisTurn( 1 ), new MovePresence( 2 ) )
			).ActivateAsync( Bind() );
		}
		if(args.Track == NewGrowth4) {
			Energy++;
			await new PickNewGrowthOption(
				new GrowthOption( new ApplyDamage() ),
				new GrowthOption( new PlayExtraCardThisTurn( 1 ), new MakePowerFast() )
			).ActivateAsync( Bind() );
		}
	}

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Put presence in land with blight
		board.Spaces.Tokens().First(s => s.Blight.Any).Adjust(Presence.Token, 1);
	}

}
