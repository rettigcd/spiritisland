﻿using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	// UI - draw more than 2 innates 

	// !!! BUG with forgetting card, leaving <null> in Hand.

	public class StarlightSeeksItsForm : Spirit {

		#region Presence Track Slot Generators

		static readonly IconDescriptor Plus1Coin = new IconDescriptor {
			BackgroundImg = ImageNames.Coin,
			Text = "+1"
		};

		static Track Track_Gain1Energy => new Track( "" ) {
			Action = new Gain1EnergyOnReveal(),
			Icon = new IconDescriptor { Super = Plus1Coin }
		};

		static Track Track_PickElement { 
			get { 
				var t = new Track("Pick element");
				t.Action = new AssignElement(t);	
				t.Icon = new IconDescriptor {
					BackgroundImg = ImageNames.Coin,
					ContentImg = ImageNames.AssignElement
				};
				return t;
			}
		}

		static Track NewGrowth1 => new Track( "so1" ) {  
			Action = new PickNewGrowthOption( false
				,new GrowthOption(new ReclaimHalf())
				,new GrowthOption(new DrawPowerCard(), new MovePresence(1))
			),
			Icon = new IconDescriptor { BackgroundImg = ImageNames.Starlight.GrowthOption1 },
		};
		static Track NewGrowth2 => new Track( "so2" ) {  
			Action = new PickNewGrowthOption( false
				,new GrowthOption(new GainEnergy(3))
				,new GrowthOption(new PlayExtraCardThisTurn(),new MovePresence(2))
			),
			Icon = new IconDescriptor { BackgroundImg = ImageNames.Starlight.GrowthOption2 },
		};
		static Track NewGrowth3 => new PickNewGrowthOption( true
				, new GrowthOption( new ReclaimAll() )
				, new GrowthOption( new DrawPowerCard(), new GainEnergy( 1 ) )
			).AssignElementFor( new Track( "so3" ) {
				Icon = new IconDescriptor { 
					ContentImg = ImageNames.AssignElement,
					Super = Plus1Coin,
					BigSub = new IconDescriptor{ BackgroundImg = ImageNames.Starlight.GrowthOption3 },
				},
			} );

		static Track NewGrowth4 => new Track( "so4" ) {  
			Action = new PickNewGrowthOption( true
				,new GrowthOption(new ApplyDamage())
				,new GrowthOption(new PlayExtraCardThisTurn(), new MakePowerFast() )
			),
			Icon = new IconDescriptor { 
				BigSub = new IconDescriptor{ BackgroundImg = ImageNames.Starlight.GrowthOption4 },
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
			new SpiritPresence(
				new CompoundPresenceTrack(
					new PresenceTrack( Track.Energy1, Track.Energy2, Track_PickElement, Track.Energy4),
					new PresenceTrack( 0, NewGrowth1 ),
					new PresenceTrack( 0, Track_Gain1Energy, NewGrowth3 ) // missing +1 energies
				),
				new CompoundPresenceTrack(
					new PresenceTrack( Track.Card2, Track_PickElement, Track_PickElement, Track.Card3 ),
					new PresenceTrack( 0, NewGrowth2 ),
					new PresenceTrack( 0, Track_Gain1Energy, NewGrowth4 ) // !!! missing +1 energies
				)
			)
			,PowerCard.For<BoonOfReimagining>()
			,PowerCard.For<GatherTheScatteredLightOfStars>()
			,PowerCard.For<PeaceOfTheNighttimeSky>()
			,PowerCard.For<ShapeTheSelfAnew>()
		) {
			Growth = new GrowthOptionGroup(
				new GrowthOption( new Reclaim1() ),
				new GrowthOption( new PlacePresence(0) ),
				new GrowthOption( new GainEnergy(1) ),
				new GrowthOption( new MovePresence(3) )
			).Pick(3);

			InnatePowers = new InnatePower[] {
				InnatePower.For<AirMovesEarthEndures>(),
				InnatePower.For<FireBurnsWaterSoothes>(),
				InnatePower.For<SiderealGuidance>(),
				InnatePower.For<WoodSeeksGrowthHumansSeekFreedom>(),
				InnatePower.For<StarsBlazeInTheDaytimeSky>(),
			};
		}

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// Put presence in land with blight
			Presence.PlaceOn( board.Spaces.First(s=>gameState.Tokens[s].Blight.Any), gameState );
		}

	}

	class ApplyDamage : GrowthActionFactory {

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			var space = await ctx.Self.Action.Decision(new Decision.TargetSpace("Select land to apply 2 Damage.", ctx.Self.Presence.Spaces,Present.Always));
			await ctx.Target(space).DamageInvaders(2);
		}

	}

	class MakePowerFast : GrowthActionFactory {

		public override Task ActivateAsync( SpiritGameStateCtx ctx ) {
			ctx.Self.AddActionFactory( new MakeActionFast() );
			return Task.CompletedTask;
		}

	}

	class Gain1EnergyOnReveal : GrowthActionFactory, ITrackActionFactory {
		bool ran;
		public bool RunAfterGrowthResult => false;

		public override Task ActivateAsync( SpiritGameStateCtx ctx ) {
			if(!ran) { 
				ctx.Self.Energy++;
				ran = true;
			}
			return Task.CompletedTask;
		}
	}


	class AssignElement : GrowthActionFactory, ITrackActionFactory {
		readonly Track track;
		public AssignElement( Track track ) { this.track = track; }

		public bool RunAfterGrowthResult => false;

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			await AssignNewElementToTrack( ctx, track );
			track.Action = null;
		}

		public static async Task AssignNewElementToTrack( SpiritGameStateCtx ctx, Track track ) {
			var el = await ctx.Self.SelectElementEx( "Select permanent element for this slot.", ElementList.AllElements );
			track.Elements = new Element[] { el };
			ctx.Self.Elements[el]++; // !!! this needs unit tests to ensure that they get 1 element (not 0, not more)
			track.Icon.ContentImg = ImageNames.For(el);
		}
	}

}
