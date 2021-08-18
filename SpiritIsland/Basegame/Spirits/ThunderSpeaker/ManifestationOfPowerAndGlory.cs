﻿using SpiritIsland;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class ManifestationOfPowerAndGlory {

		[SpiritCard( "Manifestation of Power and Glory", 3, Speed.Slow, Element.Sun, Element.Fire, Element.Air )]
		[FromPresence(0,Target.Dahan)]
		static public async Task Act( ActionEngine engine, Space target ) {
			// 1 fear
			engine.AddFear(1);
			// each dahan deals damange equal to the number of your presense in the target land
			int dahan = engine.GameState.GetDahanOnSpace( target );
			int presence = engine.Self.Presence.On(target);
			await engine.DamageInvaders(target, dahan*presence);
		}
	}
}
