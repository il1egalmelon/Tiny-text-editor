# Tiny-text-editor

A text editor, yeah. You can compile it and use it pretty much anywhere. 
The release program.run is compiled with Mono on Ubuntu 22.04. Please do not use it unless you are extremely lazy. It is usually very out of date.

Default startup screen character size is 40x150 (Lines x Char).

Keybinds:

    ESC  -> Interactive mode
    I    -> Insert mode
    F1   -> Refresh screen manually
    F2   -> Save file
    F3   -> Debugger
    F4   -> Save and close the editor
    :    -> Command line
    U    -> Undo                        (Interactive mode)
    R    -> Redo                        (Interactive mode)
    PgUp -> Page up
    PgDn -> Page down
    E    -> Tab mode                    (Interactive mode)
    N    -> New tab                     (Interactive mode)
    Left -> Goto left tab               (Tab mode)
    Rght -> Goto right tab              (Tab mode)

Command line:

    clrl          - Does the same as the one in the debugger
    s             - Saves to same file
    q             - Saves to same file and quit
    goto {}       - Goto a line
    highlight {}  - Loads highlighter file
    linemode {}   - Line mode switch from DEC, BIN or HEX
    padnum {}     - How much the cursor can under/override the max line number
    linestatus {} - Line status from true or false
    l {}          - Saves and load a file

Debugs in command line:

    /DEBUG:historytime
    /DEBUG:size
    /DEBUG:setsize <line> <char>
  
Type ? in debugger to show the list of commands for the debugger.

Startup:

    In a folder named "config", go to the "startup.txt" file.
    In there, you can add your own startup command line commands.
    Every line is one command.

Text highlighting:

    This text editor has a very primative and sort of broken text highlighting feature.
    Just place your highlighting format file in the folder called "highlight".
    And you can load up the file in the command line with "highlight {}".

    The formatting for the highlighter file is as follows:
    {COLOR} {KEYWORD}
    or 
    quote {COLOR}

    These are the supported colors: black, red, green, yellow, blue, magenta, cyan, white

    Example file:
        quote cyan
        blue char
        blue short
        blue int
        blue long
        blue float
        blue double
        yellow void
        yellow unsigned
        yellow const
        yellow auto
        red if
        red else
        red break
        red continue
        red while
        red for
    
