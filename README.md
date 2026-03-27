This patcher resolves a bug in the Cities:Skylines asset importer which would eventually cause a "too many threads" crash after importing several assets without a corresponding LOD mesh.

Although the editor supports automatic LOD generation and texture baking, both the `BuildLOD` and `BakeLODTextures` methods create a _new_ `TaskDistributor` each time they are invoked. This creates `Environment.ProcessorCount * 2` threads on each invocation, whichj are never cleaned up after the completion of each LOD mesh/texture generation pass. 

On a system with 24 cores, this meant the asset editor would crash after importing only three to four assets. 

The patch redirects `BuildLOD` and `BakeLODTextures` to use threads from the global `ThreadHelper.taskDistributor` pool instead of instantiating their own `TaskDistributor`. Since it only patches these two editor methods, there should be no side effects on gameplay or other editor tools. 
