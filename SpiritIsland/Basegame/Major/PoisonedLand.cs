using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class PoisonedLand {

		[MajorCard("Poisoned Land",3,Speed.Slow,Element.Earth,Element.Plant,Element.Animal)]
		[FromPresence(1)]
		static public async Task ActAsync(ActionEngine engine,Space target){
			var (_,gs) = engine;

			// Add 1 blight, destroy all dahan
			gs.AddBlight(target,1);
			await gs.DestoryDahan(target,engine.GameState.GetDahanOnSpace(target),DahanDestructionSource.PowerCard);

			bool hasBonus = engine.Self.Elements.Contains("3 earth,2 plant,2 animal");
			engine.AddFear( 1+(hasBonus?1:0) );
			await engine.DamageInvaders(target,7+(hasBonus?4:0));
		}

	}
}
