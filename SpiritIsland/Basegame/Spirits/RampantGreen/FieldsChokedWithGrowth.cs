using SpiritIsland.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class FieldsChokedWithGrowth {

		// push 1 town -OR- push 3 dahan
		[SpiritCard( "Fields Choked with Growth", 0, Speed.Slow, Element.Sun, Element.Water, Element.Plant )]
		[FromPresence( 1 )]
		static public async Task ActionAsync( ActionEngine eng, Space target ) {
			const string pushDahanText = "3 dahan";
			var options = new List<string>();

			if(eng.GameState.HasDahan(target))
				options.Add(pushDahanText);
			if(eng.GameState.InvadersOn(target).HasTown)
				options.Add( "1 town" );

			if(options.Count == 0) return;

			string power = await eng.SelectText("Select item(s) to push",options.ToArray());

			if(power == pushDahanText)
				await eng.PushUpToNDahan(target,3);
			else
				await eng.PushUpToNInvaders(target,1,Invader.Town);
		}
	}
}
