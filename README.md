
# Refactor Graph
### Node-based code refactoring tool for Visual Studio.

![image](https://user-images.githubusercontent.com/49317353/183232277-e833e090-f7bc-4646-b60e-3f5676d7aede.png)

[Latest version](https://marketplace.visualstudio.com/manage/publishers/chillpillgames?src=ChillPillGames.RefactorGraph1).

Note: This is a work in progress, and much of parsing functionality is still missing. Also there may be stability issues that may cause Visual Studio to crash, so make sure to save your work when using this extension. If you experience any issues, please report them [here](https://github.com/cpgames/RefactorGraph/issues).

### Concept:  

The document gets partitioned into chunks (partitions) using regex. Partitions are connected via doubly linked list.
Then any partition can be modified on individually or partitioned further into sub-partitions with a parent-child relationship. This makes it easy to perform editing, swapping, inserting, and removing parts of the documment as each partition works as a *reference*.

![RefactorGraph1](https://user-images.githubusercontent.com/49317353/175238323-a6287e3a-1afe-4d93-87ee-de55f2479cbb.png)

### Getting Started:

To get started, see [wiki](https://github.com/cpgames/RefactorGraph/wiki).

Intro video (a bit outdated): https://youtu.be/uCBUt6PwqXU
