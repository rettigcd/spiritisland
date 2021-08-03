﻿using SpiritIsland.Core;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Retreat : IFearCard {

		[FearLevel( 1, "Each player may Push up to 2 Explorer from an Inland land." )]
		public async Task Level1( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var engine = new ActionEngine(spirit,gs);
				var options = gs.Island.AllSpaces.Where(s=>!s.IsCostal && gs.InvadersOn(s).HasExplorer ).ToArray();
				if(options.Length==0) break;
				var target = await engine.SelectSpace("Fear:select land to push up to 2 invaders",options);
				await engine.PushUpToNInvaders(target,2,Invader.Explorer);
			}
		}

		[FearLevel( 2, "Each player may Push up to 3 Explorer / Town from an Inland land." )]
		public Task Level2( GameState gs ) {
			throw new System.NotImplementedException();
		}

		[FearLevel( 3, "Each player may Push any number of Explorer / Town from one land." )]
		public Task Level3( GameState gs ) {
			throw new System.NotImplementedException();
		}
	}
}