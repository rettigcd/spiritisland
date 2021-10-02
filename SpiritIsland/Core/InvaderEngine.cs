﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {
	public class InvaderEngine {

		protected readonly GameState gs;

		#region constructor

		public InvaderEngine(GameState gs ) {
			this.gs = gs;
		}

		#endregion

		public async Task DoInvaderPhase() {

			// Duplicate of InvaderPhase.ActAsync without the logging

			// Blight
			if(gs.BlightCard.IslandIsBlighted) {
				gs.Log( "Island is blighted" );
				await gs.BlightCard.OnStartOfInvaders( gs );
			}

			// Fear
			await gs.Fear.Apply();

			// Ravage
			var deck = gs.InvaderDeck;
			gs.Log( "Ravaging:" + deck.Ravage.Select(x=>x.Text).Join("/") );
			await Ravage();

			// Building
			gs.Log( "Building:" + deck.Build.Select(x=>x.Text).Join("/") );
			await Build();

			// Exploring
			deck.TurnOverExploreCards();
			gs.Log( "Exploring:" + (deck.Explore.Count > 0 ? deck.Explore[0].Text : "-") );
			await Explore( deck.Explore.ToArray() );

			deck.Advance();
		}

		#region Explore

		public virtual async Task ExploresSpace( Space space ) {
			gs.Log(space+":gains explorer");
			await gs.Tokens.Add( Invader.Explorer, space );
		}

		public async Task Explore( params InvaderCard[] invaderCards ) {

			bool HasTownOrCity( Space space ) { return gs.Tokens[ space ].HasAny(Invader.Town,Invader.City); }

			HashSet<Space> sources = gs.Island.AllSpaces
				.Where( s => s.Terrain == Terrain.Ocean || HasTownOrCity(s) )
				.ToHashSet();

			List<Space> spacesThatMatchCards = gs.Island.AllSpaces
				.Where( space => invaderCards.Any(card=>card.Matches(space)) )
				.ToList();

			// Run special event cards over it
			var args = new ExploreEventArgs( sources, spacesThatMatchCards );
			await gs.PreExplore.InvokeAsync( gs, args );

			// Add new spaces
			var spacesToExplore = args.WillExplore.ToArray();

			// Explore
			foreach(var b in spacesToExplore)
				await ExploresSpace( b );

		}

		#endregion

		#region Build

		public virtual async Task<string> Build( TokenCountDictionary counts, BuildingEventArgs.BuildType buildType ) {
			// Determine type to build
			int townCount = counts.Sum( Invader.Town );
			int cityCount = counts.Sum( Invader.City );
			TokenGroup invaderToAdd = townCount > cityCount ? Invader.City : Invader.Town;

			// check if we should
			bool shouldBuild = buildType switch {
				BuildingEventArgs.BuildType.CitiesOnly => invaderToAdd == Invader.City,
				BuildingEventArgs.BuildType.TownsOnly => invaderToAdd == Invader.Town,
				_ => true,
			};
			// build it
			if(shouldBuild)
				await gs.Tokens.Add( invaderToAdd, counts.Space, 1 );

			return invaderToAdd.Label;
		}

		public async Task Build() {

			var buildSpaces = gs.InvaderDeck.Build
				.SelectMany(card => gs.Island.AllSpaces.Where( card.Matches ))
				.GroupBy( s => s )
				.ToDictionary( grp => grp.Key, grp => grp.Count() )
				.ToCountDict();

			await BuildTheseSpaces( buildSpaces );
		}

		private async Task BuildTheseSpaces( CountDictionary<Space> myBuildSpaces ) {
			var args = new BuildingEventArgs {
				BuildTypes = new Dictionary<Space, BuildingEventArgs.BuildType>(),
				SpaceCounts = myBuildSpaces,
			};

			await gs.PreBuilding.InvokeAsync( gs, args );

			List<Space> buildSpacesWithInvaders = new List<Space>();
			foreach(var space in args.SpaceCounts.Keys.OrderBy( x => x.Label )) {
				int count = args.SpaceCounts[space];
				var tokens = gs.Tokens[space];
				while(count-- > 0) {
					if(tokens.HasInvaders()) {
						var buildType = args.BuildTypes.ContainsKey( space )
							? args.BuildTypes[space]
							: BuildingEventArgs.BuildType.TownsAndCities;
						var buildResult = await Build( gs.Tokens[space], buildType );
						gs.Log( space.Label + ": gets " + buildResult );
					} else {
						gs.Log( space.Label + ": no invaders " );
					}

				}
			}
		}

		#endregion

		#region Ravage

		/// <summary>
		/// Ravages whatever is in scheduledRavageSpaces
		/// </summary>
		/// <returns></returns>
		public async Task Ravage() {

			var ravageSpaces = gs.InvaderDeck.Ravage
				.SelectMany(card => gs.Island.AllSpaces.Where( card.Matches ))
				.ToList();

			await RavageTheseSpaces( ravageSpaces );
		}

		private async Task RavageTheseSpaces( List<Space> myRavageSpaces ) {
			await gs.PreRavaging?.InvokeAsync( gs, myRavageSpaces );

			if(myRavageSpaces == null) throw new InvalidOperationException( "dude! you forgot to schedule the ravages." );
			var ravageGroups = myRavageSpaces
				.Select( x => gs.Invaders.On( x, Cause.Ravage ) )
				.Where( group => group.Tokens.HasInvaders() )
				.Cast<InvaderGroup>()
				.ToArray();

			foreach(var grp in ravageGroups) {
				string ravageSpaceResults = await RavageSpace( grp );
				gs.Log( ravageSpaceResults );
			}
		}

		public virtual async Task<string> RavageSpace( InvaderGroup grp ) {
			var cfg = gs.GetRavageConfiguration( grp.Space );
			var eng = new RavageEngine( gs, grp, cfg );
			await eng.Exec();
			return grp.Space.Label + ": " + eng.log.Join( "  " );
		}

		#endregion

		#region move to Testing

		/// <summary>
		/// Test Ravaging...
		/// </summary>
		public async Task TestRavage( InvaderCard invaderCard ) {
			if(invaderCard == null) return;

			var ravageSpaces = gs.Island.Boards.SelectMany( board => board.Spaces )
				.Where( invaderCard.Matches )
				.ToList();

			await RavageTheseSpaces( ravageSpaces );

		}

		public async Task TestBuild( InvaderCard invaderCard ) {

			// Build normal
			var myBuildSpaces = gs.Island.AllSpaces
				.Where( invaderCard.Matches )
				.GroupBy( s => s )
				.ToDictionary( grp => grp.Key, grp => grp.Count() )
				.ToCountDict();

			await BuildTheseSpaces( myBuildSpaces );
		}

		#endregion

	}

}
