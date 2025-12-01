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

Loops can be modelled with decision nodes.

An input handler can be passed to the ActionNode class to handle input. Likewise, an output handler ca be passed to handle output. Currently, two output handlers have been implemented:
- OutputWindow : opens a popup window containing the message
- OutputControl : Custom dragable WPF control on top of the canvas that displays a ListBox containing all previous messages.

#### To Do
- Compound expressions on the right hand side of the assignment operator are currently not supported. For example, y = x + 1 would have to be modelled as y = 0, and then y = x + 1.
- Edge routing for better overview and decreased Edge / Node intersections
- Support for template strings is missing

### Frontend 
Flowcharts can be modelled in a WPF application. All currently supported node types can be used.

![Euclidean Algorithm as Flowchart](/Screenshots/Euklid.png "Euclidean Algorithm in FlowCode")


![CargoTrucker & FlowCode](/Screenshots/Cargotrucker.png "Integration of FlowCode with Cargotrucker")