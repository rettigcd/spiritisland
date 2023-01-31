
namespace SpiritIsland.Tests.Fear; 

public class AllFearCards_Tests {

	[Theory]
	[MemberData( nameof( FearCards ) )]
	public void HaveProperlyFormattedDescriptions( string title, string[] descriptions ) {
		var descriptiondProblem = new List<string>();
		for(int i = 0; i < 3; ++i) {
			var problems = new List<string>();
			string description = descriptions[i];
			void Log( string msg ) => problems.Add( msg );

			if(char.IsLower( description[0] )) Log( "Capitalize sentence." );
			if(description[0]==' ' ) Log( "Trim-Start" );
			if(description.Contains( "Terror Level" )) Log( "Terror Level??" );

			// Not checking: destroyedpresence, presence, fast, slow, blight
			if(description.Contains( "strife" ))     Log( "Strife" );
			if(description.Contains( "fear" ))       Log( "Fear" );
			if(description.Contains( "blight" ))     Log( "Blight" );
			if(description.Contains( "badlands" ))   Log( "Badlands" );
			if(description.Contains( "wilds" ))      Log( "Wilds" );
			if(description.Contains( "beast" ))      Log( "Beast" );
			if(description.Contains( "disease" ))    Log( "Disease" );
			if(description.Contains( "invader" ))    Log( "Invader" );
			if(description.Contains( "dahan" ))      Log( "Dahan" );
			if(description.Contains( "coastal" ))    Log( "Coastal" );
			if(description.Contains( "SacredSite" )) Log( "Sacred Site" );
			if(description.Contains( "sacred site" ))Log( "Sacred Site" );
			if(description.Contains( "explore" ))    Log( "Explore" );
			if(description.Contains( "town" ))       Log( "Town" );
			if(description.Contains( "city" ))       Log( "City" );
			if(description.Contains( "ravage" ))     Log( "Ravage" );

			if(description.Contains( "damage" ))     Log( "Damage" );
			if(description.Contains( "defend" ))     Log( "Defend" );

			if(description.Contains( "explorers" ))  Log( "Explore(S)" );
			if(description.Contains( "towns" ))      Log( "Town(S)" );
			if(description.Contains( "cities" ))     Log( "City(S)" );
			if(description.Contains( "beasts" ))     Log( "Beast(S)" );
			if(description.Contains( "ostal" ))      Log( "Coastal" );

			if(description.Contains( " / " ))        Log( "(/) divider" );
			if(description.Contains( ".  " ))        Log( "1-space" );
			if(!description.EndsWith( '.' ))         Log( "(.) at end" );
			if(description.EndsWith(' '))            Log( "Trim-End");
			if(0 <problems.Count)
				descriptiondProblem.Add($"{i+1}:"+problems.Join(","));
		}
		descriptiondProblem.Join("\r\n").ShouldBeEmpty(title);
	}

	public static IEnumerable<object[]> FearCards => ConfigurableTestFixture.GameBuilder.BuildFearCards()
		// Split into parts that can be serialized so they appear as separate tests
		// https://stackoverflow.com/questions/30574322/memberdata-tests-show-up-as-one-test-instead-of-many
		.Select(c=>new object[]{ c.Text, new string[] { c.GetDescription(1), c.GetDescription(2), c.GetDescription(3) } });

}
