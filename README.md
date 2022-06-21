
# RefactorGraph

Visual Studio extension for your craziest refactoring needs.  

## Concept:  

The document gets partitioned into pieces (using regex or indexing, or some other rule), partitions are connected via doubly linked list.
Then each partition is worked on individually (or partitioned further into sub-partitions). Which makes it easy to perform non-destructive edits, and also things like swapping/inserting/removing.
Finally you don't need to manually "put the document back together", as simply traversing the linked list gives the final result.

Multi-document refactor is not supported currently (as there are still potential bugs, and would rather not mess up the entire project). But I will likely add it in the future once the code is more solid.

To get started, see wiki: https://github.com/cpgames/RefactorGraph/wiki
