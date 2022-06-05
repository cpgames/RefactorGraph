
# RefactorGraph

Visual Studio extension for your craziest refactoring needs.
## Concept:
Refactor graph works by capturing the active document (or selection) and splitting it into a chunk collection. Chunk collection is composed of text chunks that have a name, text, and index within the document. Chunks can then be ordered and merged back into a single chunk via Merge node. Processed chunk can then be set as output via SetDocument node, which would replace the current selection (or active document).
## Usage:
After installing the extension you can find it in Tools->Show Refactor Graphs
![](https://github.com/cpgames/RefactorGraph/blob/main/Documentation/1.png)

Add new Refactor Graph by clicking '+'
![](https://github.com/cpgames/RefactorGraph/blob/main/Documentation/2.png)

You can add nodes via toolbar by clicking the wrench icon:
![](https://github.com/cpgames/RefactorGraph/blob/main/Documentation/4.png)

Or you can right-click on graph designer itself to bring up the context menu:
![](https://github.com/cpgames/RefactorGraph/blob/main/Documentation/5.png)


## Nodes:
  
