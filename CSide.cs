using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class TextEditor {
    private int MaxLines = 50;
    private int MaxCharactersPerLine = 100;

    private List<string> buffer;
    private int cursorLine;
    private int cursorPosition;
    private int saved;
    private string FILEPATH;
    private int logindex;

    public TextEditor() {
        buffer = new List<string>();
        cursorLine = 0;
        cursorPosition = 0;
        logindex = 0;
        saved = 0;
        FILEPATH = "";
    }

    public void Run() {
        Console.Clear();
        File.WriteAllText("latestlog.txt", "");

        // Ask the user to load a file
        Console.Write("Enter the path of the file: ");
        string filePath = Console.ReadLine();

        if (!File.Exists(filePath)) {
            string write = "Write in this file";
            File.WriteAllText(filePath, write);
        }

        FILEPATH = filePath;

        LoadFile(filePath);

        while (true) {
            RefreshScreen();
            var keyInfo = Console.ReadKey(true);
            var key = keyInfo.Key;
            if (key == ConsoleKey.UpArrow) {
                MoveCursorUp();
            }
            else if (key == ConsoleKey.DownArrow) {
                MoveCursorDown();
            }
            else if (key == ConsoleKey.RightArrow) {
                MoveCursorRight();
            }
            else if (key == ConsoleKey.LeftArrow) {
                MoveCursorLeft();
            }
            else if (key == ConsoleKey.Enter) {
                InsertNewline();
            }
            else if (key == ConsoleKey.Escape) {
                Console.Clear();
                SaveFile(0);
                break;
            }
            else if (key == ConsoleKey.F3) {
                Console.Clear();
                Debugger();
            }
            else if (key == ConsoleKey.F2) {
                Console.Clear();
                SaveFile(1);
            }
            else if (key == ConsoleKey.Spacebar) {
                InsertCharacter(' ');
            }
            else if (key == ConsoleKey.Backspace) {
                DeleteCharacter();
            }
            else if (key == ConsoleKey.F1) {
                RefreshScreen();
            }
            else { 
                InsertCharacter(keyInfo.KeyChar);
            }
        }
    }
    
    private void LoadFile(string path) {
        string[] temp = File.ReadAllLines(path);

        for (int i = 0; i < temp.Length; i++) {
            buffer.Add(temp[i]);
        }

        if (buffer.Count < (MaxLines + 1)) {
            for (int i = 0; (MaxLines + 1) > i; i++) {
                buffer.Add("");
            }
        }
    }

    private void SaveFile(int i) {
        if (i == 0) {
            Remove();
        }
        
        Console.Write("Enter the path of the file to save: ");
        string filePath = Console.ReadLine();

        try {
            File.WriteAllLines(filePath, buffer);
            Console.WriteLine("File saved successfully.");
            saved = 1;
        }
        catch (Exception ex) {
            Console.WriteLine("Error saving file: " + ex.Message);
        }

        Console.Write("Press any key to continue...");
        Console.ReadKey(true);
        Console.WriteLine("");
    }

    private void Remove() {
        List<bool> temp = new List<bool>();

        for (int i = 0; i < buffer.Count; i++) {
            if (buffer[i] == "") {
                temp.Add(true);
            }
            else {
                temp.Add(false);
            }
        }

        for (int i = buffer.Count - 1; i >= 0; i--) {
            if (temp[i] == true) {
                buffer.RemoveAt(i);
            }
            else {
                break;
            }
        }
    }

    private int Min(int a, int b) {
        return a < b ? a : b;
    }

    private int Max(int a, int b) {
        return a > b ? a : b;
    }

    private void RefreshScreen() {
        Console.Clear();
        saved = 0;

        try {
            int visibleLines = Min(Console.WindowHeight, MaxLines);
            int startLine = Max(0, cursorLine - visibleLines + 1);
                
            for (int i = startLine; i < startLine + visibleLines; i++) {
                string line = buffer[i];
                string displayLineText = line != null ? line.PadRight(MaxCharactersPerLine) : new string(' ', MaxCharactersPerLine);
                Console.WriteLine(displayLineText);
            }

            Console.SetCursorPosition(cursorPosition, cursorLine - startLine);
            
            // Place cursor at the end of the line
            string currentLine = buffer[cursorLine];
            int lineLength = currentLine != null ? currentLine.Length : 0;
            Console.SetCursorPosition(Min(lineLength, cursorPosition), cursorLine - startLine);
        } catch (Exception e) {
            File.AppendAllText("latestlog.txt", Convert.ToString(logindex) + ": " + e + "\n");
            logindex++;
        }
    }

    private void MoveCursorUp() {
        if (cursorLine > 0) {
            cursorLine--;
            if (cursorLine < Console.WindowTop) {
                Console.SetCursorPosition(Console.WindowLeft, Math.Max(0, Console.WindowTop - 1));
            }

            int lineLength = 0;

            if (buffer[cursorLine] != null) {
                lineLength = buffer[cursorLine].Length;
            }
            if (lineLength < cursorPosition) {
                cursorPosition = lineLength;
            }
        }
    }

    private void MoveCursorDown() {
        if (cursorLine < buffer.Count() - 1) {
            cursorLine++;
            if (cursorLine >= Console.WindowTop + Console.WindowHeight) {
                Console.SetCursorPosition(Console.WindowLeft, Console.WindowTop + 1);
            }

            int lineLength = 0;

            if (buffer[cursorLine] != null) {
                lineLength = buffer[cursorLine].Length;
            }
            if (lineLength < cursorPosition) {
                cursorPosition = lineLength;
            }
        }
    }

    private void MoveCursorRight() {
        if (cursorPosition < MaxCharactersPerLine - 1 && cursorPosition < buffer[cursorLine].Count()) {
            cursorPosition++;
        }
    }

    private void MoveCursorLeft() {
        if (cursorPosition > 0) {
            cursorPosition--;
        }
    }

    private void InsertNewline()
    {
        // Shift lines down to make room for the new line
        buffer.Insert(cursorLine + 1, "");

        cursorLine++;
        cursorPosition = 0;
    } 

    private void InsertCharacter(char c) {
        if (buffer[cursorLine] == null) {
            buffer[cursorLine] = string.Empty;
        }

        if (cursorPosition <= buffer[cursorLine].Length) {
            if (c == '\t') {
                for (int i = 0; i < 4; i++) {
                    buffer[cursorLine] = buffer[cursorLine].Insert(cursorPosition, " ");
                    cursorPosition++;
                }
            }
            else {
                buffer[cursorLine] = buffer[cursorLine].Insert(cursorPosition, c.ToString());
                cursorPosition++;
            }
        }
    }

    private void DeleteCharacter() {
        if (cursorPosition > 0 && buffer[cursorLine] != null && cursorPosition <= buffer[cursorLine].Length) {
            buffer[cursorLine] = buffer[cursorLine].Remove(cursorPosition - 1, 1);
            cursorPosition--;
        }

        if (cursorPosition <= 0 && buffer[cursorLine] != null && cursorPosition <= buffer[cursorLine].Length) {
            if (buffer[cursorLine] == "" && cursorLine != 0) {
                buffer.RemoveAt(cursorLine);
                cursorLine--;
            }
        }
    }

    private void Debugger() {
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.Clear();
        Console.WriteLine("DEBUGGER");
        while (true) {
            Console.Write("> ");
            string input = Console.ReadLine();

            File.AppendAllText("latestlog.txt", Convert.ToString(logindex) + ": " + input + "\n");
            logindex++;

            if (input == "") {
                // Do nothing lmao
            } else if (input == "help") {
                Console.Write(    "F1   -> Refresh Screen\n" +
                                  "F2   -> Save\n" +
                                  "F3   -> Debugger\n" +
                                  "ESC  -> Save and close editor\n");
            } else if (input == "exit") {
                break;
            } else if (input == "?") {
                Console.Write(    "help -> All editor keybinds\n" +
                                  "exit -> Exits debugger\n" +
                                  "?    -> This menu\n" +
                                  "dump -> Dump text file\n" +
                                  "comp -> Compile code\n" +
                                  "run  -> Run a program\n" +
                                  "save -> Save the file\n" +
                                  "load -> Load a file\n" +
                                  "size -> Window size of editor\n" +
                                  "quit -> Quit without saving\n" +
                                  "s    -> Save to same file\n");
            } else if (input == "dump") {
                foreach (string temp in buffer) {
                    Console.WriteLine(temp);
                }
            } else if (input == "comp") {
                Console.WriteLine("FEATURE NOT AVAILABLE IN THIS VERSION");
            } else if (input == "run") {
                Console.WriteLine("FEATURE NOT AVAILABLE IN THIS VERSION");
            } else if (input == "save") {
                SaveFile(1);
            } else if (input == "load") {
                if (saved == 1) {
                    Console.Write("Enter the path of the file: ");
                    string tempinput = Console.ReadLine();

                    if (!File.Exists(tempinput)) {
                        string write = "Write in this file";
                        File.WriteAllText(tempinput, write);
                    }
                    buffer.Clear();

                    FILEPATH = tempinput;

                    LoadFile(tempinput);
                }
                else {
                    Console.WriteLine("ERROR: Save the file first!");
                }
            } else if (input == "size") {
                Console.Write("Max lines: ");
                int tempinputline = Convert.ToInt32(Console.ReadLine());

                Console.Write("Max char: ");
                int tempinputchar = Convert.ToInt32(Console.ReadLine());

                MaxLines = tempinputline;
                MaxCharactersPerLine = tempinputchar;
            } else if (input == "quit") {
                Console.Write("Type \"Y\" to continue: ");
                string temp1 = Console.ReadLine();

                if (temp1 == "Y") {
                    Environment.Exit(0);
                }
            } else if (input == "s") {
                try {
                    File.WriteAllLines(FILEPATH, buffer);
                    Console.WriteLine("File saved successfully.");
                    saved = 1;
                }
                catch (Exception ex) {
                    Console.WriteLine("Error saving file: " + ex.Message);
                } 
            } else {
                Console.WriteLine("Invalid command");
            }
        }
        Console.BackgroundColor = ConsoleColor.Black;
    }
}

class Program {
    static void Main(string[] args) {
        TextEditor editor = new TextEditor();
        editor.Run();
    }
}
