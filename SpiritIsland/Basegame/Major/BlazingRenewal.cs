
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class BlazingRenewal {

		[MajorCard("Blazing Renewal",5,Speed.Fast,Element.Fire,Element.Earth,Element.Plant)]
		[TargetSpirit]
		static public async Task ActAsync(ActionEngine engine, Spirit target ) {

			var targetEngine = new ActionEngine(target,engine.GameState);

			// target spirit adds 2 of their destroyed presence
			int max = await targetEngine.SelectNumber("Select # of destroyed presence to return to board", target.Presence.Destroyed );
			if(max==0) return;

			// Note:
			// * Not using targets Api because the source is not their presence nor SS
			// * Not using originators Api, beacuse it is not originators decision
			// If either spirit has a range-extension, should it apply to this?

			// into a single land, up to range 2 from your presence.
			var targetSpaceOptions = engine.Self.Presence.Spaces.SelectMany(x=>x.SpacesWithin(2)).Distinct().ToArray();
			var landTarget = await targetEngine.SelectSpace("Select new presence destination",targetSpaceOptions);

			// Add it!
			for(int i=0;i<max;++i)
				target.Presence.PlaceFromBoard(Track.Destroyed,landTarget);

			// if any presene was added, 2 damage to each town/city in that land.
			var grp = engine.GameState.InvadersOn(landTarget);
			grp.ApplyDamageToEach(2,Invader.Town,Invader.City);

			// if you have 3 fire, 3 earth , 2 plant, 4 damage in that land
			if(engine.Self.Elements.Contains("3 fire,3 earth,2 plant"))
				await grp.ApplySmartDamageToGroup(4);


		}
	}
}
