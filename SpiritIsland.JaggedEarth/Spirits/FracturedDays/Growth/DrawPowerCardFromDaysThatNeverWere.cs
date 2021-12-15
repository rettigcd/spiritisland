using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	class DrawPowerCardFromDaysThatNeverWere : GrowthActionFactory  {

		public override async Task ActivateAsync( SelfCtx ctx ) {
			if( ctx.Self is not FracturedDaysSplitTheSky fracturedDays ) return;

			var minor = fracturedDays.DtnwMinor;
			var major = fracturedDays.DtnwMajor;

			PowerCard card = await ctx.Self.SelectPowerCard("Gain Power Card from Days That Never Were", minor.Union( major ), CardUse.AddToHand, Present.Always );
			if(card == null) return; // in case both hands are empty.

			fracturedDays.AddCardToHand( card );
			if( minor.Contains( card ))
				minor.Remove( card );
			else {
				major.Remove( card );
				await fracturedDays.ForgetPowerCard(Present.Always);
			}

		}

	}

}
