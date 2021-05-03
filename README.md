# RoboCode

Robocode is a competitive environment in which competitors implement custom tank robots which battle in an 
arena. I designed, architected, and implemented a Finite State Machine decision-making AI that will act as 
part of a team of autonomous agents. I implemented a single software robot from the ground up that 
functions in the Robocode .NET environment.

“ManhattanProject” uses a robust, extensible, decision-making abstract structure of Finite State Machine. 
“ManhattanProject”  will enqueue an event when a certain requirement is fulfilled.  “ManhattanProject” dequeue 
and apply the event to get the next state(or stay in the same state) each tick of the game. The FSM has a total 
of four states - start, attack, ram and flee, and includes six events - hasTarget, noTarget, ramable, notRamable, 
lowEnergy and highEnergy, for it to react on. “ManhattanProject” will react to an event differently in different states. 

