# Cola-Corpses-Recode
Just a remaster / sequel to one of my previous projects

## Project Goal
I made this project with the intention of revisiting one of my favorite past projects to refine the various systems that I made. Throughout my time as a programmer I have, naturally, gained many new skills and I feel that I can use them to better my previous work.

## Major Differences

### Map
- The game now uses hexagons as the based for the randomly generated map instead of squares in order to get the rooms to feel a bit more natural
- Maps now have multiple floors
- This new map generation now supports premade rooms that have a set layout and items within them
  - This allows more stylized rooms to be present while still being randomly mixed in with the rest of the map
  - These preset "rooms" also include staircases and clearings that can be used in many different ways
- Rooms now have themes that allow for different tile, ceiling and wall variations to differentiate one room from another.
  - Each theme also has variations for tiles, ceilings and walls to give the room itself a little more natural feeling rather than just one repeated material
- Tiles, Ceilings and Walls now use combined meshes to save processing power and allow the game to run more smoothly even when generating several hundred rooms

### Weapons
- Traits and Favors return from the last game with new underlying frameworks which will allow me to easily add new ones in the future
  - New flavors and traits will be coming as well
- Each Weapon possesses a primary and secondary action similar to the last game, but this new framework allows for mixing and matching different primaries and secondaries
  - Should be noted that new primary and secondary actions are being added as well, in case that part was not clear
- Crafting may or may not return, not sure yet
  - May be traded out in favor of random weapon drops. Crafting made the game very easy, since you could usually get whatever you wanted, so I want to change how the player aquires weapons.
 
### Misc Stuff
- I have a large focus now on avoiding the use on GetComponent in order to cut down on processing power needed at run time
- Models and art style will be more consistent with the pixelated style instead of whatever I did with the previous game
