# Tiny-text-editor

A text editor, yeah. You can compile it and use it pretty much anywhere. 
The release program.run is compiled with Mono on Ubuntu 22.04. Please do not use it unless you are extremely lazy.

Default startup screen character size is 50x100 (Lines x Char).

Keybinds:

    ESC -> Interactive mode
    I   -> Insert mode
    F1  -> Refresh screen manually
    F2  -> Save file
    F3  -> Debugger
    F4  -> Save and close the editor
    :   -> Command line
    U   -> Undo
    R   -> Redo

Command line:

    clrl    - Does the same as the one in the debugger
    s       - Saves to same file
    q       - Saves to same file and quit
    goto {} - Goto a line

Debugs in command line:

    /DEBUG:historytime
    /DEBUG:size
  
Type ? in debugger to show the list of commands for the debugger.
