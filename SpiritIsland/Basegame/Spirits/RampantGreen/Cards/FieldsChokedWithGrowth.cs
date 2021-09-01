using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class FieldsChokedWithGrowth {

		// push 1 town -OR- push 3 dahan
		[SpiritCard( "Fields Choked with Growth", 0, Speed.Slow, Element.Sun, Element.Water, Element.Plant )]
		[FromPresence( 1 )]
		static public async Task ActionAsync( TargetSpaceCtx ctx ) {
			const string pushDahanText = "3 dahan";
			var options = new List<string>();

			if(ctx.HasDahan)
				options.Add(pushDahanText);
			if(ctx.PowerInvaders.Counts.Has(Invader.Town))
				options.Add( "1 town" );

			if(options.Count == 0) return;

			string power = await ctx.Self.SelectText("Select item(s) to push",options.ToArray());

			if(power == pushDahanText)
				await ctx.PowerPushUpToNDahan(3);
			else
				await ctx.PowerPushUpToNInvaders(1,Invader.Town); // PowerPush!
		}
	}
}
