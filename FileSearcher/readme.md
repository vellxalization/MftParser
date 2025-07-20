# DFS file searcher

## How to use
Run the program with the next arguments:
1. Volume letter;
2. Name of the file (case-sensitive).

By default, when the program finds first match, it will print the full path to the file and shut down. You can add "-m" or "--multiple" argument so that it will continue to work until all directories in the volume are scanned (might take some time depending on the depth of the file tree)