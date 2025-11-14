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

#### To Do
- Compound expressions on the right hand side of the assignment operator are currently not supported. For example, y = x + 1 would have to be modelled as y = 0, and then y = x + 1.
- Nodes can currently not be deleted. This needs fixing.
- As for the CargoTrucker integration, drag and drop of playing fields needs to be implemented.

- Also, need to implement drag and drop of .vtn files, as well as passing them as command line args.

### Frontend 
Flowcharts can be modelled in a WPF application. All currently supported node types can be used.

![Euclidean Algorithm as Flowchart](/Screenshots/Euklid.png "Euclidean Algorithm in FlowCode")


![CargoTrucker & FlowCode](/Screenshots/CargoTrucker.png "Integration of FlowCode with Cargotrucker")