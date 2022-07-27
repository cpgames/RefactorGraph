
# Refactor Graph

![image](https://user-images.githubusercontent.com/49317353/175891778-9c9d7a1b-8472-4485-b62c-5b14873fc685.png)

Node-based code refactoring tool for Visual Studio.

Install latest version here: https://marketplace.visualstudio.com/manage/publishers/chillpillgames?src=ChillPillGames.RefactorGraph1

WARNING: This is a work in progress, and much of parsing functionality is still missing. Also there are stability issues that may cause Visual Studio to crash, so make sure to SAVE YOUR WORK when using this extension. If you experience any issues, please report them here: https://github.com/cpgames/RefactorGraph/issues 
Also repo is missing some submodules, I will update it later.

## Concept:  

The document gets partitioned into pieces (using regex or indexing, or some other rule), partitions are connected via doubly linked list.
Then each partition is worked on individually (or partitioned further into sub-partitions). Which makes it easy to perform edits, and also things like swapping/inserting/removing.
Finally you don't need to manually "put the document back together", as simply traversing the linked list gives the final result.
![RefactorGraph1](https://user-images.githubusercontent.com/49317353/175238323-a6287e3a-1afe-4d93-87ee-de55f2479cbb.png)

Multi-document refactor is now supported! Use DTE nodes to enumerate projects and files in the project.

To get started, watch the Introduction video: https://www.youtube.com/watch?v=uCBUt6PwqXU

Also see examples (more added soon): https://github.com/cpgames/RefactorGraph/tree/main/Examples
