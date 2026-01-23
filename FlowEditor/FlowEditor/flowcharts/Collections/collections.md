# Input and Output
Collections can store multiple values of the same type. They are useful when related data must be grouped together.

## Usage
Lists are defined using square brackets `[]`, with elements separated by commas. For example, a list of integers can be defined as follows:
	numbers = [1, 2, 3, 4, 5]

Currently, no other collection types (like arrays, sets or dictionaries) are supported.
A single item within a collection can be accessed using its index (position in the list), with indexing starting at 0. For example, to access the first element of the `numbers` list:
	first_number = numbers[0]

The property `Count` can be used to get the number of elements in a collection. For example:
	number_of_elements = numbers.Count

## Examples

The following example flowcharts are included in the "Flowcharts" folder:

### List-Sum
This example demonstrates how to sum up all the numbers in a list and display the result.
Complexity: Moderate

The following concepts are implemented:
- input / output (displaying the message)
- loops (for adding all items)
- collections (to store multiple numbers)

### Recursive-Max
This example demonstrates how to find the maximum number in a list using recursion.
Complexity: Hard

The following concepts are implemented:
- input / output (displaying the message)
- collections (to store multiple numbers)
- recursions (for finding the maximum number)