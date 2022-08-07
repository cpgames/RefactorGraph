
# Refactor Graph
Refactor Graph is a node-based code refactoring tool for Visual Studio 2022.  

## Sample Graph:
![image](https://user-images.githubusercontent.com/49317353/183280565-9f4a11d5-d585-4d0f-b6e4-319620da01d7.png)

## Installation:
Manually [**[Download Latest Version]**](https://marketplace.visualstudio.com/manage/publishers/chillpillgames?src=ChillPillGames.RefactorGraph1) and run the .vsix file  
or  
In your Visual Studio go to Extensions -> Manage Extensions -> Select "Online" tab and type "RefactorGraph" in the search field.  
![image](https://user-images.githubusercontent.com/49317353/183313529-efb7196b-cf73-4fa4-92e4-6791cfa0c028.png)   
You will need to restart Visual Studio to finish the installation.

Note: This is a work in progress, and much of parsing functionality is still missing. Also there may be stability issues that may cause Visual Studio to crash, so make sure to save your work when using this extension. If you experience any issues, please report them [here](https://github.com/cpgames/RefactorGraph/issues).

## Concept:  
The document gets partitioned into chunks (partitions) using regex. Partitions are connected via doubly linked list.
Then any partition can be modified on individually or partitioned further into sub-partitions with a parent-child relationship. This makes it easy to perform editing, swapping, inserting, and removing parts of the documment as each partition works as a *reference*.

![RefactorGraph1](https://user-images.githubusercontent.com/49317353/175238323-a6287e3a-1afe-4d93-87ee-de55f2479cbb.png)

## Getting Started:
To get started, see [wiki](https://github.com/cpgames/RefactorGraph/wiki).  
Also take a look at sample [Tests](https://github.com/cpgames/RefactorGraph/tree/main/Tests) for premade examples ([how to run tests](https://github.com/cpgames/RefactorGraph/wiki/Running-Tests)).  
[Intro video](https://youtu.be/uCBUt6PwqXU) (it's a bit outdated, but still describes the concept). 

## License:
Refactor Graph is licensed under the MIT license.
