
== How Builds Work ==

Step 1:	Invader Cards (and other powers,scenarios, etc) add build tokens to a space.

Then, For all the spaces that have build tokens...

	Step 2: Various stopper tokens (disease, rampant green, infestatoin of spiders) stop the builds and are optionally consumed.

	Step 3: Towns/Cities are built.

== Behavior Overrides ==

England changes where the builds occur.

France adds some post-build actions
	- could transfer the build behavior to GameState, 
	  then we could get the build behavior out of the Invader cards and move logic in the Build Engine
	  Do we want to do that?


== Testing Notes ==

[Trait( "Feature", "Build" )]

Test:
	Pour Time Sideways
	Fear Cards that modify build
	Trading a Ravage for a build (pre-loading tokens)
	Do any cards cause an extra build.

== Pre-Build Hook ==

Used by
	Avoid the Dahan
	Trade Suffers
	Immigration Slows
	Pour Time Sideways

! We could get rid of this hook if we put that logic in a custom stopper for each thing that uses it.