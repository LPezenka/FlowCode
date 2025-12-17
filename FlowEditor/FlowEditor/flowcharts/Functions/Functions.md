# Functions
Functions are predefined processes that can take input parameters and return a value. In addition to the input parameters and return value, each function has a name.

## Usage
Functions are modelled by
- First, creating a Terminal Node and setting Type to "Start"
	- Input parameters are declared in the textbox "Input Params", with no type declaration. 
	- Types are inferred at runtime
- Secondly, implementing the function body as usual
- Thirdly, creating a Terminal Node and setting Type to "End"
	- The return variable is specified in the textbox "return Variable" 
- Finally, creating a Process Call Node and calling the defined function in the form FunctionName(Params)

## Examples

### Rock, Paper, Scissors
Complexity: Medium
In this classical game, a random number generator is used. The following concepts are implemented:
- input / output (reading player inputs and displaying messages)
- while loops (play rounds until the player chooses to quit)
- sub processes (play a round; there are no parameters and no return value)
- functions (to generate a random number and translate it to the corresponding string value - either "rock", "paper", or "scissors")
- variables (storing both the human players and the pc players choice)
- alternatives (for deciding who won the round)