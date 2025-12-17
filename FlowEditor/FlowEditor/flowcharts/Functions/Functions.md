# Functions
Functions are predefined processes that can take input parameters and return a value. In addition to the input parameters and return value, each function has a name. Functions with no return value are referred to as subroutines. They can (but do not need to) receive parameters from the caller, but do not return anything. Hence, in their case, the communication between caller and call target is unidirectional, as opposed to bidirectional in the case of functions.

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

The following example flowcharts are included in the "Flowcharts" folder:

### Bark
Complexity: Easy
In this example, a subroutine is called that simply displays a message ("Woof, woof"). No parameters or return values are used.
The following concepts are implemented:
- input / output (displaying the message)
- subroutines

### Welcome Message
Complexity: Easy
In this example, the user is prompted for a name. The name is then passed to a subroutine, which displays a welcome message.
The following concepts are implemented:
- input / output (reading user input and displaying welcome message)
- subroutines
- variables (the input is stored in a string variable)
- parameters (the name is passed to the subroutine)
- template strings (to generate the welcome message)

### Multiplication
Complexity: Easy
In this example, a subroutine is used to multiply two numbers and output the result using a template string.
The following concepts are implemented:
- input / output (reading user inputs for the factors and displaying the result)
- subroutines
- variables (the factors are stored in integer variables)
- parameters (the numbers are passed to the subroutine)
- template strings (to generate the result message)

### Multiplication Table
Complexity: Medium
This example is an extension of the previous one. Rather than just performing one multiplication, a user-specified number is multiplied by the numbers 1...9. Results are output with template strings
The following concepts are implemented:
- input / output (reading user inputs for the factors and displaying the result)
- loops (do while loop)
- subroutines
- variables (the factors are stored in integer variables)
- parameters (the numbers are passed to the subroutine)
- template strings (to generate the result message)

### Addition
Complexity: Easy
In this example, a return value is introduced, hence turning subroutines into functions. However, in the signature of the Add function, the names of the in-parameters are omitted. In such a case, the name of the variable at the time of the function call is internally used. Note that this is not standard behaviour for most programming languages.
The following concepts are implemented:
- variables (the value is stored in an integer variable)
- functions (to process the addition and return the incremented value)

### Rock, Paper, Scissors
Complexity: Medium
In this classical game, a random number generator is used. The following concepts are implemented:
- input / output (reading player inputs and displaying messages)
- while loops (play rounds until the player chooses to quit)
- subroutines (play a round; there are no parameters and no return value)
- functions (to generate a random number and translate it to the corresponding string value - either "rock", "paper", or "scissors")
- variables (storing both the human players and the pc players choice)
- alternatives (for deciding who won the round)