namespace SpiritIsland.PromoPack1;

class ScotlandBuildEngine : BuildEngine {

	public bool ShouldChartTheCoastline { get; set; }

	public override bool ShouldBuildOnSpace( SpaceState spaceState ) 
		=> base.ShouldBuildOnSpace( spaceState ) || ChartTheCoastline( spaceState );

	bool ChartTheCoastline( SpaceState spaceState )
		=> ShouldChartTheCoastline 
		&& spaceState.Space.IsCoastal 
		&& spaceState.Adjacent.Any( adj => adj.Has( Invader.City ) );
}


/*

Escalation Stage II Escalation.png
Ports Sprawl Outward: On the single board with the most Coastal Town/City, add 1 Town to the N lands with the fewest Town (N = # of players.)

1	(3)	10 (3/4/3)	Trading Port: After Setup, in Coastal lands, Explore Cards add 1 Town instead of 1 Explorer. "Coastal Lands" Invader cards do this for maximum 2 lands per board.
2	(4)	11 (4/4/3)	Seize Opportunity: During Setup, add 1 City to land #2. Place "Coastal Lands" as the 3rd Stage II card, and move the two Stage II Cards above it up by one. (New Deck Order: 11-22-1-C2-33333, where C is the Stage II Coastal Lands Card.)
3	(6)	13 (4/5/4)	Chart the Coastline: In Coastal lands, Build Cards affect lands without Invaders, so long as there is an adjacent City.
4	(7)	14 (5/5/4)	Ambition of a Minor Nation: During Setup, replace the bottom Stage I Card with the bottom Stage III Card. (New Deck Order: 11-22-3-C2-3333))
5	(8)	15 (5/6/4)	Runoff and Bilgewater: After a Ravage Action adds Blight to a Coastal Land, add 1 Blight to that board's Ocean (without cascading). Treat the Ocean as a Coastal Wetland for this rule and for Blight removal/movement.
6	(10) 16 (6/6/4)	Exports Fuel Inward Growth: After the Ravage step, add 1 Town to each Inland land that matches a Ravage card and is within 1 of town/city

Additional Loss Condition
Trade Hub: If the number of Coastal lands with City is ever greater than (2 x # of boards), the Invaders win.

*/
