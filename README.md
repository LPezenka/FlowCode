# FlowCode
Simple, flowchart based code interpreter using the C# scripting engine.

## Progress

### Backend
The backend is independent of the WPF application. An importer for draw.io networks is in the code; some rules and limitations apply, though.

So far, the following nodes can be processed:
- sequence nodes
- decision nodes
- predefined process nodes 
- terminal nodes

Loops can be modelled with decision nodes as either for-loops, do-while loops or while loops. No support for foreach loops is implemented at the time, nor is it planned for the near future.

An input handler can be passed to the ActionNode class to handle input. Likewise, an output handler ca be passed to handle output. Currently, two output handlers have been implemented:
- OutputWindow : opens a popup window containing the message
- OutputControl : Custom dragable WPF control on top of the canvas that displays a ListBox containing all previous messages.

#### To Do
- Support for arrays and lists is missing
- Support for template strings is missing
- Due to the mechanics of the variable type inferrence, a variable called lineInput remains in the scope. This might take some restructuring to solve

### Frontend 
Flowcharts can be modelled in a WPF application. All currently supported node types can be used.


A for loop (or counting loop) requires initialization, a boolean condition (answered with true/false), a loop body, and an increment (an update). The following graphic illustrates the implementation of a simple counting loop in FlowCode:

![For Loop](/Screenshots/For-Loop-With-Output.png "For Loop in FlowCode")

Recursive algorithms can be implemented by defining subprocesses and functions. The following graphics show two classical examples of recursive algorithms, the Fibonacci Sequence and the Eulidean Algorithm for the calculation of the greates common denominator of two numbers, implemented in FlowCode.

![Fibonacci Sequence](/Screenshots/Fibonacci.png "Fibonacci Sequence in FlowCode")
![Euclidean Algorithm as Flowchart](/Screenshots/Euklid.png "Euclidean Algorithm in FlowCode")

CargoTrucker is a C# implementation of the Java Hamster model implemented by Dr. Kerer at HTL Wien West. The library and visual playground is available [here](https://sites.google.com/htlwienwest.at/cargotrucker/startseite?pli=1&authuser=0). FlowCode is fully integrated with Cargo Trucker and allows students to learn computer science and programming concepts in a clearly structured, gamified manner, with instant visual feedback.

![CargoTrucker & FlowCode](/Screenshots/Cargotrucker.png "Integration of FlowCode with Cargotrucker")