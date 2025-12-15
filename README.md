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

![Fibonacci Sequence](/Hints/fibonacci.png "Fibonacci Sequence in FlowCode")

![Euclidean Algorithm as Flowchart](/Screenshots/Euklid.png "Euclidean Algorithm in FlowCode")


![CargoTrucker & FlowCode](/Screenshots/Cargotrucker.png "Integration of FlowCode with Cargotrucker")