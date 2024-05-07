# Spitty Frog
Inpired by the timeless Zooma

## Menu Scene
* Start
* if there is top score record then "Top score: 1900"
* Exit

## Level Scene
Only one level so far.

#### Controlls
* Esc - exit to Main menu
* RMB or Space - fire
* anykey - loads main menu if game over

#### Game entities

* Frog
  * looks the side the mouse cursor points
  * if GameManager.isRun LMB or Space fire() a **ball** from the **Line**
  * if any **ball** from the **snake** meets the frog, GAME OVER
* Line
  * it's a part of UI as a preview and the same time a source of new **balls** to the **frog**
  * the **balls** are added to the top of the line by the **generator** and shits from the bottom by the **frog**
  * the size of the **line** is **10**
* Snake
  * it's a queue of positions filled by **balls** that appears at the left end from the **generator**
  * while Game isRun, every **2** seconds new **ball** is added to the left end and all the **balls** moves one step right
  * Inject procedure **off isRun**,place a fired **ball** to the left either right side from a target **ball**. All **balls** that's are at the right side from the injected **ball** moves one step to the right -> so it could happen the gameover here
  * Check procedure runs right after each **Inject**. If three or more balls are in a row, they all explodes, the **score** increases by the ball number by **score** formulas. All the balls that are on the right side from exploded ones moves to the left till there are no place to move.
  * after **check** despite of the result (but if not gameover) **on isRun**
* Snake ball
  * took a place in the **snake** queue
  * has 2 box colliders on their sides
  * by touching with a fired **ball** and base of the side makes decision to inject the ball to a side, so calls Snake.inject()
* Score
  * UI element, shows the current score
  * Increase by calling inrease(int count, Color color)
  * every increase put scores to the storage topScore
* Storage
  * loads saved topscore value
  * if not exists set to 0
  * provides with topScore value
  * provides with set topScore
  * if set value greater than existing, save to the disk
* Game over caption
  * shown if game is over
  * isRun is false
  * anykey loads the Main Menu
  