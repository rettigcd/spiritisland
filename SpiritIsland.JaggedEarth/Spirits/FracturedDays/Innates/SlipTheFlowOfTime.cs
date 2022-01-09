using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	[InnatePower("Slip the Flow of Time", "You may use this Power any number of times. Cost to Use: 1 Time per previous use this turn"), Fast, AnySpirit]
	[RepeatWithTime]
	class SlipTheFlowOfTime {

		// !!! "You may use this Power any number of times.
		// Cost to Use: 1 Time per previous use this turn.

		[InnateOption("3 moon,1 air","Target Spirit may Resolve 1 slow Power now.")]
		static public Task Option1( TargetSpiritCtx ctx ) {

			var speedChanger = new SpeedChanger( ctx.OtherCtx, Phase.Fast, 2 );
			return speedChanger.FindAndExecute();

			//// -------------
			//// Select Actions to resolve
			//// -------------
			//IActionFactory[] options = this.GetAvailableActions( speed ).ToArray();
			//IActionFactory option = await this.SelectFactory( "Select " + speed + " to resolve:", options, present );
			//if(option == null)
			//	return false;

			//// if user clicked a slow card that was made fast, // slow card won't be in the options
			//if(!options.Contains( option ))
			//	// find the fast version of the slow card that was clicked
			//	option = options.Cast<IActionFactory>()
			//		.First( factory => factory == option );

			//if(!options.Contains( option ))
			//	throw new Exception( "Dude! - You selected something that wasn't an option" );

			//await TakeAction( option, ctx );
			//return true;

			var otherCtx = ctx.OtherCtx;
			return otherCtx.Self.ResolveAction( Phase.Slow,Present.Done, otherCtx );

		}

		[InnateOption("2 sun,2 moon","Target Spirit may Reclaim 1 Power Card from their discarded or played cards.",1)]
		static public Task Option2( TargetSpiritCtx ctx ) {
			return ctx.Other.Reclaim1FromDiscardOrPlayed();
		}

		[InnateOption("3 sun,2 air","Target Spirit may play a Power Card by paying its cost.",2)]
		static public Task Option3( TargetSpiritCtx ctx ) {
			return ctx.Other.SelectAndPlayCardsFromHand( 1 );
		}

	}

}
