# A simple file browser that uses $I30 index of the volume to show folder's content

## How to use:
1) Run the project with a single argument - the letter of the volume
2) Use commands "list" or "cd" to navigate

## Commands
* "list" or "ls" - prints the content of the current open folder in the format: "time of alteration - flags - name"
<br>Flags are file attributes. There are 23 attributes in total that can be present in a file, although we only display 9 most relevant. They are:
<br> 1. (R)eadonly;
<br> 2. (H)idden;
<br> 3. (S)ystem;
<br> 4. (D)irectory;
<br> 5. (A)rchive;
<br> 6. (T)emporary;
<br> 7. Reparse (P)oint;
<br> 8. (C)ompressed;
<br> 9. (E)ncrypted.
<br>If any of the attributes is absent, it is replaced with a hyphen ('-')
* "cd" - changes current working folder
<br> You can specify both absolute and relative paths to the file. If any folder in the path contains a space in the name, enclose the whole path in quotation marks (e.g. "D:/folder with space")
