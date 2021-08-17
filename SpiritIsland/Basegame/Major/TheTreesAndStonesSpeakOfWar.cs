using System;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class TheTreesAndStonesSpeakOfWar {

		[MajorCard( "The Trees and Stones Speak of War", 2, Speed.Fast, Element.Sun, Element.Earth, Element.Plant )]
		[FromPresence(1,Target.Dahan)]
		static public async Task ActionAsync( ActionEngine engine, Space target ) {

			// if you have 2 sun, 2 earth, 2 plant
			bool bonus = engine.Self.Elements.Contains("2 sun, 2 earth, 2 plant");

			// for each dahan in target land, 1 damage and defend 2

			// -- damage --
			await engine.DamageInvaders( target, engine.GameState.GetDahanOnSpace(target) );

			// you may push up to 2 dahan
			Space[] dest = bonus ? await engine.PushUpToNDahan( target, 2 ) : Array.Empty<Space>();

			// -- defend --
			engine.GameState.Defend( target, engine.GameState.GetDahanOnSpace(target)*2 );
			foreach(var d in dest)
				engine.GameState.Defend( d, 2 );

		}

	}
}
