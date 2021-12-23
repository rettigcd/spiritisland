﻿using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class FlowingAndSilentFormsDartBy {

		[SpiritCard("Flowing and Silent Forms Dart By",0, Element.Moon,Element.Air,Element.Water), Fast, FromPresence(0) ]
		static public async Task ActAsync(TargetSpaceCtx ctx) {

			// 2 fear if Invaders are present
			if(ctx.HasInvaders)
				ctx.AddFear( 2 );

			// When presence in target land would be Destroyed, its owner may, if possible instead Push that presence.
			// (do it for all spirits, not just the ones currently here)
			foreach(var spirit in ctx.GameState.Spirits)
				ctx.GameState.TimePasses_ThisRound.Push( new PushInsteadOfDestroy(spirit,ctx.Space).Restore );

			// You may Gather 1 presence / Sacred site of another Spirit (with their permission).
			await GatherSomeonesPresence( ctx );

		}


		class PushInsteadOfDestroy : IDestroyPresenceBehavour {

			readonly Spirit spirit;
			readonly IDestroyPresenceBehavour originalBehavior;
			readonly Space protectedSpace;

			public PushInsteadOfDestroy(Spirit spirit, Space protectedSpace) {
				this.spirit = spirit;
				this.protectedSpace = protectedSpace;
				this.originalBehavior = spirit.Presence.DestroyBehavior;
				spirit.Presence.DestroyBehavior = this;

			}

			public async Task DestroyPresenceApi( SpiritPresence presence, Space space, GameState gs, ActionType actionType ) {
				if( space == this.protectedSpace) {
					var dst = await spirit.Action.Decision(new Select.Space("Instead of destroying, push presence to:", space.Adjacent,Present.Done));
					if(dst != null) {
						presence.Move(space,dst,gs);
						return;
					}
				}
				await originalBehavior.DestroyPresenceApi(presence, space, gs, actionType );
			}

			public Task Restore( GameState _ ) {
				spirit.Presence.DestroyBehavior = originalBehavior;
				return Task.CompletedTask;
			}

		}


		static async Task GatherSomeonesPresence( TargetSpaceCtx ctx ) {
			var adj = ctx.Space.Adjacent;
			// Pick Spirit
			var nearbySpirits = ctx.GameState.Spirits.Where( s => s.Presence.Spaces.Intersect( adj ).Any() ).ToArray();
			Spirit other = ctx.GameState.Spirits.Length == 1 ? ctx.Self
				: await ctx.Decision( new Select.Spirit( "Flowin and Silent Forms Dard By", nearbySpirits ) );
			// Pick spot
			var source = await ctx.Decision( Select.DeployedPresence.Gather("Gather presence", ctx.Space, adj.Intersect(other.Presence.Spaces)) );
			if(source == null) return;
			// # to move
			int numToMove = (other.Presence.CountOn(source) > 1 && await ctx.Self.UserSelectsFirstText("# of presence to gather", "2", "1"))
				? 2
				: 1;
			// Get permission
			if(other != ctx.Self && await ctx.Self.UserSelectsFirstText($"Allow Shroud to Gather {numToMove} of your presence {source.Label} => {ctx.Space.Label} ?", "No, I don't want to move", "Yes, please"))
				return; // cancel

			// move
			while(numToMove-->0)
				other.Presence.Move(source,ctx.Space,ctx.GameState);
		}
	}

}
