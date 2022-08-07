
# Refactor Graph
### Node-based code refactoring tool for Visual Studio.
[**[Download Latest Version]**](https://marketplace.visualstudio.com/manage/publishers/chillpillgames?src=ChillPillGames.RefactorGraph1)

![image](https://user-images.githubusercontent.com/49317353/183280565-9f4a11d5-d585-4d0f-b6e4-319620da01d7.png)

Note: This is a work in progress, and much of parsing functionality is still missing. Also there may be stability issues that may cause Visual Studio to crash, so make sure to save your work when using this extension. If you experience any issues, please report them [here](https://github.com/cpgames/RefactorGraph/issues).

### Concept:  

The document gets partitioned into chunks (partitions) using regex. Partitions are connected via doubly linked list.
Then any partition can be modified on individually or partitioned further into sub-partitions with a parent-child relationship. This makes it easy to perform editing, swapping, inserting, and removing parts of the documment as each partition works as a *reference*.

![RefactorGraph1](https://user-images.githubusercontent.com/49317353/175238323-a6287e3a-1afe-4d93-87ee-de55f2479cbb.png)

### Getting Started:

To get started, see [wiki](https://github.com/cpgames/RefactorGraph/wiki).

[Intro video](https://youtu.be/uCBUt6PwqXU) (it's a bit outdated, but still describes the concept). 
