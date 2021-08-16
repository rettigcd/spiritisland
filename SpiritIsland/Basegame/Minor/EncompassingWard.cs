namespace SpiritIsland.Basegame {

	public class EncompassingWard {

		public const string Name = "Encompassing Ward";

		[MinorCard(EncompassingWard.Name,1,Speed.Fast,Element.Sun,Element.Water,Element.Earth)]
		[TargetSpirit]
		static public void Act(ActionEngine engine,Spirit target){
			// defend 2 in every land where spirit has presence
			foreach(var space in target.Presence.Spaces)
				engine.GameState.Defend(space,2);
		}
	}
}
