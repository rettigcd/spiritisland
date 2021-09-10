﻿using SpiritIsland;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class SuddenAmbush {

		public const string Name = "Sudden Ambush";

		[SpiritCard( SuddenAmbush.Name, 2, Speed.Fast, Element.Fire, Element.Air, Element.Animal )]
		[FromPresence(1)]
		static public async Task Act( TargetSpaceCtx ctx ) {
			var target = ctx.Space;
			// you may gather 1 dahan
			await ctx.GatherUpToN(target, 1, TokenType.Dahan );

			// Each dahan destroys 1 explorer
			int dahahCount = ctx.GameState.DahanGetCount( target );
			await ctx.Invaders.Destroy( dahahCount, Invader.Explorer);
		}

	}
}
