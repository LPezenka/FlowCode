# FlowCode
Simple, flowchart based code interpreter using the C# scripting engine.

## Progress

### Backend
The backend is independent of the WPF application. An importer for draw.io networks is in the code; some rules apply, though.

So far, the following nodes can be processed:
- sequence nodes
- decision nodes
- predefined process nodes 
- terminal nodes

Loops can be modelled with decision nodes.

An input handler can be passed to the ActionNode class to handle input. 

### Frontend 
Flowcharts can be modelled in a WPF application. All currently supported node types can be used.

![Euclidean Algorithm as Flowchart](/Screenshots/Euklid.png)
