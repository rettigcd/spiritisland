using System;
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

			var debug = gs.Tokens[ gs.Island.Boards[0][5] ];

			// Duplicate of InvaderPhase.ActAsync without the logging

			// Blight
			if(gs.BlightCard.IslandIsBlighted) {
				gs.Log( new InvaderActionEntry( "Island is blighted" ) );
				await gs.BlightCard.OnStartOfInvaders( gs );
			}

			// Fear
			await gs.Fear.Apply();

			// Ravage
			var deck = gs.InvaderDeck;
			gs.Log( new InvaderActionEntry( "Ravaging:" + deck.Ravage.Select(x=>x.Text).Join("/") ) );
			await Ravage();

			// Building
			gs.Log( new InvaderActionEntry( "Building:" + deck.Build.Select(x=>x.Text).Join("/") ) );
			await Build();

			// Exploring
//			deck.InitExplorers();
			gs.Log( new InvaderActionEntry( "Exploring:" + (deck.Explore.Count > 0 ? deck.Explore[0].Text : "-") ) );
			await Explore( deck.Explore.ToArray() );

			deck.Advance();
		}

		#region Explore

		public async Task ExploresSpace( Space space ) {
			// only gets called when explorer is actually going to explore
			var wilds = gs.Tokens[space].Wilds;
			if(wilds == 0) { 
				gs.Log( new InvaderActionEntry(space+":gains explorer") );
				await gs.Tokens.Add( Invader.Explorer, space );
			}
			else
				wilds.Count--;


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

		public async Task<string> Build( TokenCountDictionary tokens, BuildingEventArgs.BuildType buildType ) {
			var disease = tokens.Disease;
			if(disease.Any) {
				disease.Count--;
				return tokens.Space.Label +" build stopped by disease";
			}

			// Determine type to build
			int townCount = tokens.Sum( Invader.Town );
			int cityCount = tokens.Sum( Invader.City );
			TokenGroup invaderToAdd = townCount > cityCount ? Invader.City : Invader.Town;

			// check if we should
			bool shouldBuild = buildType switch {
				BuildingEventArgs.BuildType.CitiesOnly => invaderToAdd == Invader.City,
				BuildingEventArgs.BuildType.TownsOnly => invaderToAdd == Invader.Town,
				_ => true,
			};
			// build it
			if(shouldBuild)
				await gs.Tokens.Add( invaderToAdd, tokens.Space, 1 );

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
						gs.Log( new InvaderActionEntry( space.Label + ": gets " + buildResult ) );
					} else {
						gs.Log( new InvaderActionEntry( space.Label + ": no invaders " ) );
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

		async Task RavageTheseSpaces( List<Space> myRavageSpaces ) {
			await gs.PreRavaging?.InvokeAsync( gs, new RavagingEventArgs{ Spaces = myRavageSpaces } );

			if(myRavageSpaces == null) throw new InvalidOperationException( "dude! you forgot to schedule the ravages." );
			var ravageGroups = myRavageSpaces
				.Select( x => gs.Invaders.On( x, Cause.Ravage ) )
				.Where( group => group.Tokens.HasInvaders() )
				.Cast<InvaderGroup>()
				.ToArray();

			foreach(var grp in ravageGroups) {
				string ravageSpaceResults = await RavageSpace( grp );
				gs.Log( new InvaderActionEntry( ravageSpaceResults ) );
			}
		}

		public async Task<string> RavageSpace( InvaderGroup grp ) {

			// Do Ravage
			var cfg = gs.GetRavageConfiguration( grp.Space );
			var eng = new RavageEngine( gs, grp, cfg );
			var @event = await eng.Exec();

			if(@event != null) 
				await gs.InvadersRavaged.InvokeAsync(gs, @event);

			// Return Result / event
			return @event.ToString();
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
