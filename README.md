StellarIO
A modern, pure browser-based real-time strategy game/space simulation in a Sci-Fi space setting. Build your empire. Colonize unknown planets. Forge allies. Fight for glory and control.

## Concept
- There are galaxies and systems as coordinates. Within these, there are distinct planets with their own coordinates.
- A player starts controlling a planet when they sign up and log in for the first time.
- The goal is to rule the galaxy and become the most influential entity.
- There is a scoreboard across all players based on points. Points can be achieved by building facilities, ships, and doing science.

## Galaxy, System, and Planets
- There are 10 galaxies. Each galaxy has 150 systems. Each system has between 6-12 planets (random).
- A planet has coordinates like this that depict the galaxy, system, and planet itself: `100:14:6`.

### Planet Stats
Each planet has distinctive stats:
- Relative Speed (random value between 90% and 110%)
- Relative Iron output (random value between 90% and 110%)
- Relative Silver output (random value between 90% and 110%)
- Relative Aluminium output (random value between 90% and 110%)
- Relative H2 output (random value between 90% and 110%)
- Relative Energy output (random value between 90% and 110%)

- Every planet starts with a set of 500 resources each.

### Resources
Resources are bound to planets. The current resources are:
- Iron
- Silver
- Aluminium
- H2
- Energy

- These resources are shown to the player just below the navigation bar, for the current planet he has selected. Default selection: Main planet (first one).

## Resource Generation
- Resources are gained every second per planet based on the level relative output of the planet and based on the level of mines and HQs present on that planet.

## Buildings
A player can build the following buildings on a planet. A building has a level which affects the output of resources. A building has a duration. That duration increases every level by 5%.
- **HQ** (+1% to all resource output per planet)
  - Cost level 0: Iron: 300, Silver: 100, Aluminum: 150, Energy: 90

- **Iron Mine** (+3% to iron output per planet)
  - Cost level 0: Iron: 100, Silver: 500, Aluminum: 150, Energy: 140

- **Silver Mine** (+3% to silver output per planet)
  - Cost level 0: Iron: 100, Silver: 200, Aluminum: 75, Energy: 30

- **Aluminum Workshop** (+3% to aluminum output per planet)
  - Cost level 0: Iron: 300, Silver: 100, Aluminum: 50, Energy: 500

- **H2 Condenser** (+3% to H2 output per planet)
  - Cost level 0: Iron: 350, Silver: 600, Aluminum: 50, Energy: 400

- **Fusion Reactor** (+3% to Energy output per planet / -1% H2 output per planet)
  - Cost level 0: Iron: 800, Silver: 900, Aluminum: 750, Energy: 50

- **Research Center** (-3% to science duration on that planet)
  - Cost level 0: Iron: 300, Silver: 100, Aluminum: 150, Energy: 90

- **Shipyard** (-3% to fleet construction duration on that planet)
  - Cost level 0: Iron: 300, Silver: 100, Aluminum: 150, Energy: 90

- Costs increase 5% per every level.

## Science
Science is bound to a player. Players need to do science to upgrade their ships, resource output, and more. Science can only be done on planets where a Research Center exists.
Currently, the following sciences exist:
- Jet engine (Propulsion)
- Lineardrive
- Transitorialdrive
- Laser beam concentrator (Weapon)
- Parashield (Defensive)
- Impulse beam (Weapon)
- Iron Mining (Resource Output +3% across all planets)

## Ships and Fighting
Ships belong to players. There are also battle points. In a fight between fleets (when attacking or defending a planet), there is a simulated battle based on the strength of the ships. When ships are destroyed, the owner of the destroyed ships receives negative battle points, while the destroyer gains battle points based on the ships he destroyed.

### Ship Actions
Ships can do any of these actions:
- Attack a planet (If the fight is won, it plunders the planet to the extent of available cargo space of the attacking fleet)
- Transport resources to a planet
- Move to another planet
- Colonize a planet that does not belong to a player (Only COL-01)

### Ship Stats
A ship has attack and defense points. Different ships have different propulsion, speed, weapons, and such. There are currently these ships:
- **SDT-14**: Small attack fighter (Cost: Iron 650 | Cargo: 500 | Propulsion: Jet engine)
- **SPD-15**: Battleship that houses 5 attack fighters (Cost: Iron 2500, Silver 1500, Energy 400 | Cargo: 2500 | Propulsion: Lineardrive)
- **STT-01**: Container ship that offers 50k of resource transportation capability (Cost: Iron 2800, Silver 1500, Energy 400 | Cargo: 50000 | Propulsion: Transitorialdrive)
- **COL-01**: Ship that can colonize additional planets for the player that sends them to an empty planet (Cost: Iron 16500, Silver 5500, Energy 5500 | Cargo: 255000 | Propulsion: Transitorialdrive)

### Building Ships
Ships can only be built on planets where there is a Shipyard. In order to build a ship, the player needs to have the required science unlocked.

## Flying and Propulsion
Different propulsion has different speeds regarding their targets.
- **Jet engine**: In same system min flight time: 3 hours, next neighbor system: 8 hours
- **Lineardrive**: In same system min flight time: 2 hours, next neighbor system: 3 hours
- **Transitorialdrive**: In same system min flight time: 2.5 hours, next neighbor system: 5 hours

- Duration of a flight increases if the targeted planet is further away (based on coordinates).

# MVP Functionality
- A user called `testuser` with password `test` is preseeded in the database.
- This user has 3 planets and an HQ level 1 on each planet.
- The MVP features the existing model, with necessary alterations for Authentication (e.g., additional properties for the User Class).
- A user can register with Email, Username, and Password.
  - If registration fails, an appropriate error message is displayed.
  - If registration is successful, a success message is displayed.
- A registered user can log in with Email and Password.
  - If authentication fails, an appropriate error message is displayed.
  - If authentication is successful, a message "Loading HQ..." is displayed for 3 seconds.
  - After "Loading HQ", the user is presented with an overview of their planets and resources.
- The resource generation has to work properly.
