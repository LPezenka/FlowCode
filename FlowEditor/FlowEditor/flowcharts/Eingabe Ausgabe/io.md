# Input and Output
Input and ouput are fundamental operations that are required for most user interaction. The keywords for either operation can be user-defined. 

## Usage
Inputs follow the form: v = Eingabe
- where "Eingabe" is the german word for input. This keyword can be changed in the Config class
- variable type on the left hand side is automatically inferred by the code interpreter

Outputs follow the form: Ausgabe: v
- where "Ausgabe" is the german word for output. This keyword can be changed in the Config class
- string literals need to be delimited with double quotation marks, i.e., "this is a string", whereas *this is not*

## Examples

The following example flowcharts are included in the "Flowcharts" folder:

### template-strings
This example demonstrates how to read an integer variable, generate a template string and display it on screen
Complexity: Easy

The following concepts are implemented:
- input / output (displaying the message)
- template strings (to generate the output message)

### template-strings-with-loop
This example extends the previous one by adding a loop and combining the numbers from 1...10 in a template string before displaying the string
Complexity: Easy

The following concepts are implemented:
- input / output (displaying the message)
- template strings (to generate the output message)
- loops (for loop)

### bottles-on-the-wall
This example is an implementation of the classical folk ong "99 bottles of beer". A variable is initialized with a user-specified number of beer bottles and decremented in a loop. Each iteration of the loop displays a message indicating how many bottles are left.
Complexity: Easy

The following concepts are implemented:
- input / output (displaying the message)
- template strings (to generate the output message)
- loops (for loop)
- arithmetics (for calculating the remaining number of bottles on the wall)