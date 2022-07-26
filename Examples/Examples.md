### Examples

To use examples, copy .rgraph files to `<YourProject>/RefactorGraphs` folder, then click "Reload refactor graphs", then open corresponding example .cs file in Visual Studio.

#### RemoveParameter

Removes second parameter in function and function call. 

![Graph](https://github.com/cpgames/RefactorGraph/blob/main/Examples/RemoveParameter/RemoveParameter0.png)

Before:

![Before](https://github.com/cpgames/RefactorGraph/blob/main/Examples/RemoveParameter/RemoveParameter1.png)

After:

![Before](https://github.com/cpgames/RefactorGraph/blob/main/Examples/RemoveParameter/RemoveParameter2.png)

Starting from the Bus node: 
1. Remove second parameter in function Foo. We can find that parameter by name "param2".
2. Rasterize partition is necessary if we want to perform multiple partition operations on the same partition.
3. For the function call, we can't use parameter name, so we need to create a collection of parameters and remove the second one.

---

#### SortParameters

Sort function parameters in descending order by parameter names

![Graph](https://github.com/cpgames/RefactorGraph/blob/main/Examples/SortParameters/SortParameters0.png)

Before:

![Before](https://github.com/cpgames/RefactorGraph/blob/main/Examples/SortParameters/SortParameters1.png)

After:

![Before](https://github.com/cpgames/RefactorGraph/blob/main/Examples/SortParameters/SortParameters2.png)

First we need to extract function parameters, note PartitionByParameters node that breaks up parameter block into individual parameter partitions. Then we use PartitionByFirstRegexMatch to extract parameter name (which in our example is a second word, so using regex `\w*\s*\K\w*` would skip the first word + space).
Next we create PartitionSortingMap which maps parameter names to parenting parameter partion and will be used for sorting function.
Finally using SortPartitions node that accepts (optional) sorting map.

---
