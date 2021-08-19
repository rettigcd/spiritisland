using System;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class TheTreesAndStonesSpeakOfWar {

		[MajorCard( "The Trees and Stones Speak of War", 2, Speed.Fast, Element.Sun, Element.Earth, Element.Plant )]
		[FromPresence(1,Target.Dahan)]
		static public async Task ActionAsync( TargetSpaceCtx ctx ) {

			// if you have 2 sun, 2 earth, 2 plant
			bool bonus = ctx.Self.Elements.Contains("2 sun, 2 earth, 2 plant");

			// for each dahan in target land, 1 damage and defend 2

			// -- damage --
			await ctx.DamageInvaders( ctx.Target, ctx.GameState.GetDahanOnSpace( ctx.Target ) );

			// you may push up to 2 dahan
			Space[] dest = bonus ? await ctx.PushUpToNDahan( ctx.Target, 2 ) : Array.Empty<Space>();

			// -- defend --
			ctx.GameState.Defend( ctx.Target, ctx.GameState.GetDahanOnSpace( ctx.Target ) *2 );
			foreach(var d in dest)
				ctx.GameState.Defend( d, 2 );

		}

	}
}
