# RoboCode

Robocode is a competitive environment in which competitors implement custom tank robots which battle in an 
arena. I designed, architected, and implemented a **Finite State Machine** decision-making AI that will act as 
part of a team of autonomous agents. I implemented a single software robot from the ground up that 
functions in the Robocode **.NET environment**.

“ManhattanProject” uses a robust, extensible, decision-making abstract structure of Finite State Machine. 
“ManhattanProject”  will enqueue an event when a certain requirement is fulfilled.  “ManhattanProject” dequeue 
and apply the event to get the next state(or stay in the same state) each tick of the game. The FSM has a total 
of four states - start, attack, ram and flee, and includes six events - hasTarget, noTarget, ramable, notRamable, 
lowEnergy and highEnergy, for it to react on. “ManhattanProject” will react to an event differently in different states. 

“ManhattanProject” uses a robust, extensible, decision-making abstract structure of Finite State Machine. “ManhattanProject” 
will enqueue an event when a certain requirement is fulfilled.  “ManhattanProject” dequeue and apply the event to get the next 
state(or stay in the same state) each tick of the game. “ManhattanProject” will react to an event differently in different states. 
![image](https://user-images.githubusercontent.com/44207825/116946407-0798a880-ac48-11eb-919e-6cabcb5423ec.png)

“ManhattanProject” will alway stay sideways (perpendicular - 17°) to its target except when it’s trying to ram[3]. It will also 
change its direction every 20 ticks to prevent being too predictive. “ManhattanProject” always moves up and down while the 
enemy’s bullet comes from the left. It gives “ManhattanProject” a better chance of dodging the bullet.
![image](https://user-images.githubusercontent.com/44207825/116946517-4d557100-ac48-11eb-8742-54f011288c58.png)

“ManhattanProject” has a maximum shooting range of 600 units. It will not shoot any unit if it is 600 units away from it. 
Once the target gets in range, “ManhattanProject” will start shooting the target until it runs out of energy, the energy is 
outside of its range again, or energy is dead. “ManhattanProject” will shoot a bullet with fire power ranging from 1.0 - 3.0 
depending on how far the energy is.
![image](https://user-images.githubusercontent.com/44207825/116946550-69591280-ac48-11eb-9266-6146b442f0d4.png)
