For a list of many test methods, see:
- SpiritExtensions.cs
- SpaceExtensions.cs

# Init Tokens on a Space

```
space.Given_InitSummary("2E@1") - sets all visible tokens
space.Given_HasToken("2E@1") - sets named tokens
space.Given_ClearTokens() - removes visible tokens
```


# Init Spirits presence on a Space
	// Spirit-based
	spirit.Given_IsOn(space);
	spirit.Given_IsOn(space,count);

	// Space-based
	space.Given_HasTokens("1RSiS");
	space.Init( spirit.Presence.Token, count );
# Configure Game


var gameState = new GameConfiguration()
	.ConfigAdversary(new AdversaryConfig(Scotland.Name, level))
	.ConfigBoards("A")
	.ConfigSpirits(RiverSurges.Name)
	.BuildGame()

# Invaders Cards

## Get a Card for a specific SPACE
```
var card1 = space.BuildInvaderCard(); // card will match other spaces also.
var card2 = spaceSpec.BuildInvaderCard(); // card will match other spaces also.
```
## Get a Card for a specific TERRAIN
```
var terrainCard = InvaderDeckBuilder.Stage1( Terrain.Sand );
var coast = InvaderDeckBuilder.Stage2Coastal;
```

DO NOT
- Pull it from InvaderDeckBuilder.Level1Cards
- Search the Deck until it matches land (gs.InvaderDeck.Unreavealed.First(x=>x.Matches(a1)))
- Search the Deck until title matches

## Build

### Flow:
- BuildSlot.ActivateCard( card )
- Engine.ActivateCard( card )
- Adds Build Tokens, then.. does builds.
- Engine.DoAllBuildsInSpace( space )
- Engine.TryToDo1Build()

## Build on all spaces for a specific Invader Card
```
// see above for generating Invader Card for specific Terrain
await invaderCard.When_Building();

```
## Ravaging

Flow:
	RavageSlot.ActivateCard( card )
		> Engine.ActivateCard( card )
			> Adds Ravage Tokens, then does ravages.
				> Engine.DoAllRavagesOn1Space( space )
					> ravageSpace.Ravage();
						> RavageBehavior.Exec( this );
							> IConfigRavages(...)
							> do RavageSequence()


# Configure Spirit

## Set Spirit's Elements
```
spirit.Configure().Elements("3 moon,2 animal");
```

## Reveal Presence Track

```
await spirit.Presence.CardPlays.Given_SlotsAreRevealed(3);
```

## Actions
The Active/Resolved state of Actions are tracked in 2 ways:
1) Non-Innate Actions are Active when they are in the AvailableActions
2) Innate Actions are Active until they are added to the Used-Innates list.

### Make IActionFactory _Active_
Instead of adding to Hand and then Playing (which requires Energy)
```
spirit.AddActionFactory( card );
```
### Mark IActionFactory as _Used_
```
spirit.MarkAsResolved( card );
```
### Resolve an Active action
Also marks it as _Used_.
```
spirit.ResolveActionAsync(cards[2], Phase.Fast);
```


# Test a Power Card

## Call Directly
- Don't need: scope, ActionScope.Owner, nor Spirit initialization
- Already know target (spirit or space) and want to specify via code
- Doesn't require setting up targetting Source/Range/Destination
```
await BlazingRenewal.ActAsync( spirit.Target(spirit) ).AwaitUser(...);
await InfinitVitality.ActAsync( spirit.Target(space) );
```
## Invoke through Spirit
- Use when you need ActionScope.Owner initialized and spirit initialization
- Specify target via User Selection (u.NextDecsiion.Choose(...))
- Requires setting up targetting Source/Range/Destination
```
spirit.When_ResolvingCard<T>( user=>{...} );
spirit.Task When_ResolvingInnate<T>( user=>{...} );
```

These methods come with .ShouldComplete()


