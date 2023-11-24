using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class TextEditor {
    private string DecimalCharacters = "0123456789";

    private int MaxLines = 40;
    private int MaxCharactersPerLine = 150;

    public static List<string> buffer;
    private int cursorLine;
    private int cursorPosition;
    private int saved;
    public static string? FILEPATH;
    private int logindex;
    private int infobarcharlines;

    private int historytime;
    private int historymaxtime;

    public static string textmode = "Interactive";
    public static string additionaltext = "";
    public static string additionalstatus = "";
    public static string additionalstatus2 = "";

    bool highlightingstatus = false;
    public static List<string> highlightkeywords;
    public static string highlightquote = "";

    public TextEditor() {
        buffer = new List<string>();
        cursorLine = 0;
        cursorPosition = 0;
        logindex = 0;
        saved = 0;
        FILEPATH = "";
        infobarcharlines = 3;
        historytime = 0;
        historymaxtime = 0;
        highlightkeywords = new List<string>();
    }

    public void Run() {
        int errorstartup = 0;

        try {
            System.IO.Directory.Delete("history", true);
        } catch (Exception ex) {
            Console.WriteLine(ex);
            errorstartup = 1;
        }

        try {
            System.IO.Directory.CreateDirectory("history"); 
        } catch (Exception ex) {
            Console.WriteLine(ex);
            errorstartup = 1;
        }

        if (errorstartup == 1) {
            Console.Write("\nPress any key to continue...");

            Console.ReadKey(true);
        }

        try {
            Console.Clear();
            if (!System.IO.Directory.Exists("highlight")) {
                System.IO.Directory.CreateDirectory("highlight");
            }
        } catch (Exception ex) {
            Console.WriteLine(ex);
            Console.Write("\nPress any key to continue...");

            Console.ReadKey(true);
        }

        try {
            Console.Clear();
            if (!System.IO.Directory.Exists("config")) {
                System.IO.Directory.CreateDirectory("config");
            }
        } catch (Exception ex) {
            Console.WriteLine(ex);
            Console.Write("\nPress any key to continue...");

            Console.ReadKey(true);
        }

        Console.Clear();
        File.WriteAllText("latestlog.txt", "");

    gobacktoloadfile:
        Console.Write("Enter the path of the file: ");
        string filePath = Console.ReadLine();
        
    gobacktoaskloadfile:
        if (!File.Exists(filePath)) {
            Console.Write("File doesn't exist, do you want to create it (Y / N): ");
            string filecreationthing = Console.ReadLine();

            if (filecreationthing == "Y") {
                string write = "Write in this file";
                File.WriteAllText(filePath, write);
            } else if (filecreationthing == "N") {
                goto gobacktoloadfile;
            } else {
                goto gobacktoaskloadfile;
            }
        }

        FILEPATH = filePath;

        LoadFile(filePath);

        pushhistory();

        if (File.Exists("config/startup.txt")) {
            string[] startupcmd = File.ReadAllLines("config/startup.txt");
            foreach (string command in startupcmd) {
                additionaltext = command;
                additionalstatus = "[startup]";
                commands();
                additionaltext = "";
                additionalstatus = "";
            }
        } else {
            File.Create("config/startup.txt");
        }

        while (true) {
            RefreshScreen();

            if (historytime > historymaxtime) {
                historymaxtime = historytime;
            }

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
            else if (key == ConsoleKey.Enter && textmode == "Insert") {
                InsertNewline();
                pushhistory();
            }
            else if (key == ConsoleKey.F4) {
                Console.Clear();
                SaveFile(0);
                break;
            }
            else if (key == ConsoleKey.I && textmode == "Interactive") {
                textmode = "Insert";
            }
            else if (key == ConsoleKey.Escape && textmode == "Insert") {
                textmode = "Interactive";
            }
            else if (keyInfo.KeyChar == ':' && textmode == "Interactive") {
                commandhandler();
            }
            else if (key == ConsoleKey.F3) {
                Console.Clear();
                Debugger();
            }
            else if (key == ConsoleKey.F2) {
                Console.Clear();
                SaveFile(1);
            }
            else if (key == ConsoleKey.U && textmode == "Interactive") {
                undo();
            }
            else if (key == ConsoleKey.R && textmode == "Interactive") {
                redo();
            }
            else if (key == ConsoleKey.Spacebar && textmode == "Insert") {
                InsertCharacter(' ');
                pushhistory();
            }
            else if (key == ConsoleKey.Backspace && textmode == "Insert" ) {
                DeleteCharacter();
                pushhistory();
            }
            else if (key == ConsoleKey.F1 && textmode == "Insert" ) {
                RefreshScreen();
            }
            else if (DecimalCharacters.Contains(keyInfo.KeyChar) && textmode == "Interactive") {
                multishift(keyInfo.KeyChar.ToString());
            }
            else if (textmode == "Insert"){ 
                InsertCharacter(keyInfo.KeyChar);
                pushhistory();
            }
        }
    }
    
    private void LoadFile(string path) {
        try {
            string[] temp = File.ReadAllLines(path);

            for (int i = 0; i < temp.Length; i++) {
                buffer.Add(temp[i]);
            }

            /*
            if (buffer.Count < (MaxLines + 1)) {
                for (int i = 0; (MaxLines + 1) > i; i++) {
                    buffer.Add("");
                }
            }
            */
        } catch (Exception) {

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

    private void multishift(string input) {
        int num = int.Parse(input.ToString());
        var keyInfo = Console.ReadKey(true);
        var key = keyInfo.Key;

        if (DecimalCharacters.Contains(keyInfo.KeyChar)) {
            multishift(input + keyInfo.KeyChar.ToString());
        } else if (key == ConsoleKey.UpArrow) {
            for (int i = 0; i < num; i++) {
                MoveCursorUp();
            }
        } else if (key == ConsoleKey.DownArrow) {
            for (int i = 0; i < num; i++) {
                MoveCursorDown();
            }
        } else if (key == ConsoleKey.LeftArrow) {
            for (int i = 0; i < num; i++) {
                MoveCursorLeft();
            }
        } else if (key == ConsoleKey.RightArrow) {
            for (int i = 0; i < num; i++) {
                MoveCursorRight();
            }
        }
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

        infobar();

        int visibleLines = Min(Console.WindowHeight, MaxLines);
        int startLine = Max(0, cursorLine - visibleLines + 1);

        try {
            for (int i = startLine; i < startLine + visibleLines; i++) {
                string line = buffer[i];
                string displayLineText = line != null ? line.PadRight(MaxCharactersPerLine) : new string(' ', MaxCharactersPerLine);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(i.ToString("D8"));
                Console.Write(" | ");
                Console.ResetColor();
                if (highlightingstatus == false) {
                    Console.WriteLine(displayLineText);
                }
                else {
                    WriteLineColor(displayLineText, highlightkeywords.ToArray(), highlightquote);
                }
            }
        } catch (Exception e) {
            File.AppendAllText("latestlog.txt", Convert.ToString(logindex) + ": " + e + "\n");
            logindex++;
        }

        Console.SetCursorPosition(cursorPosition + 12, cursorLine - startLine);
            
        // Place cursor at the end of the line
        string currentLine = buffer[cursorLine];
        int lineLength = currentLine != null ? currentLine.Length : 0;
        try {
            Console.SetCursorPosition(Min(lineLength, cursorPosition) + 11, cursorLine - startLine + infobarcharlines);
        } catch (Exception) {

        }
    }

    private void commands() {
        if (additionaltext == "s") {
            try {
                File.WriteAllLines(FILEPATH, buffer);
                additionaltext = "File saved successfully, press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
                saved = 1;
            }
            catch (Exception ex) {
                additionaltext = "Error saving file: " + ex.Message + ", press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
                saved = 1;
            } 
        }
        else if (additionaltext == "q") {
            additionaltext = "s";
            commands();
            Environment.Exit(0);
        }
        else if (additionaltext == "/DEBUG:historytime") {
            additionaltext = Convert.ToString(historytime) + ", press any key to continue...";
            RefreshScreen();
            Console.ReadKey(true);
        }
        else if (additionaltext == "/DEBUG:size") {
            additionaltext = "Lines: " + Convert.ToString(MaxLines) + 
                             ", Chars: " + Convert.ToString(MaxCharactersPerLine) + 
                             ", press any key to continue...";
            RefreshScreen();
            Console.ReadKey(true);
        }
        else if (additionaltext.Contains("/DEBUG:setsize")) {
            string[] temp = additionaltext.Split(" ");

            try {
                MaxLines = Convert.ToInt32(temp[1]);
                MaxCharactersPerLine = Convert.ToInt32(temp[2]);
                additionaltext = "Changed to: " + temp[1] + "x" + temp[2] + ", press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
            } catch (Exception) {
                additionaltext = "Invalid, press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
            }
        }
        else if (additionaltext.Contains("goto")) {
            string[] temp = additionaltext.Split(" ");

            try {
                int tempmax = buffer.Count;
                if (Convert.ToInt32(temp[1]) <= tempmax && Convert.ToInt32(temp[1]) >= 0) {
                    cursorLine = Convert.ToInt32(temp[1]);
                } else {
                    //This will throw an error
                    Convert.ToInt32("fuck");
                }
            } catch (Exception) {
                additionaltext = "Invalid, press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
            }
        } 
        else if (additionaltext == "clrl") {
            for (int i = buffer.Count - 1; i >= 0; i--) {
                if (buffer[i] == "") {
                    if (cursorLine == i) {
                        cursorLine--;
                    }
                    buffer.RemoveAt(i);
                } else {
                    break;
                }
            }
        }
        else if (additionaltext.Contains("highlight")) {
            try {
                if (additionaltext.Split(" ")[1] == "default") {
                    highlightingstatus = false;
                }

                highlightkeywords.Clear();
                highlightingstatus = true;

                string[] keywords = File.ReadAllLines("highlight/" + (additionaltext.Split(" "))[1]);
                RefreshScreen();

                int quoted = 0;

                foreach (string keywordsin in keywords) {
                    string[] temp = keywordsin.Split(" ");

                    if (temp[0] == "quote") {
                        highlightquote = temp[1];
                        quoted = 1;
                    }
                    else {
                        highlightkeywords.Add(keywordsin);
                    }
                }

                if (quoted == 0) {
                    highlightquote = "white";
                }

                additionaltext = "Loaded: " + additionaltext.Split(" ")[1] + ", press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
            } catch (Exception) {
                additionaltext = "Invalid, press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
            }
        }
        else {
            additionaltext = "Invalid, press any key to continue...";
            RefreshScreen();
            Console.ReadKey(true);
        }
    }

    private void commandhandler() {
        additionalstatus = "[Command]";
        while (true) {
            RefreshScreen();
            var keyInfo = Console.ReadKey(true);
            var key = keyInfo.Key;

            char c = keyInfo.KeyChar;
            if (key == ConsoleKey.Enter) {
                commands();
                break;
            }
            else if (key == ConsoleKey.Backspace) {
                try {
                    additionaltext = additionaltext.Remove(additionaltext.Length - 1, 1);
                } catch (Exception) {

                }
            }
            else {
                additionaltext += c;
            }
        }
        additionaltext = "";
        additionalstatus = "";
    }

    private void undo() {
        try {
            if (historytime > 1) {
                buffer.Clear();
                pophistory();
                LoadFile("history/" + Convert.ToString(historytime - 1));
            }
        } catch (Exception) {

        }
    }

    private void redo() {
        try {
            if (historytime + 1 <= historymaxtime) {
                buffer.Clear();
                historytime++;
                LoadFile("history/" + Convert.ToString(historytime - 1));
            }
        } catch (Exception) {

        }
    }

    private void pushhistory() {
        try {
            File.WriteAllText("history/" + Convert.ToString(historytime), "");
            foreach (string x in buffer) {
                File.AppendAllText(("history/" + Convert.ToString(historytime)), x + "\n");
            }
            historytime++;
        } catch (Exception) {

        }
    }

    private void pophistory() {
        try {
            historytime--;
        } catch (Exception) {
            
        }
    }

    private void infobar() {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[" + textmode + "]" + additionalstatus);
        Console.WriteLine(additionaltext);

        //info bar seperator line
        Console.Write("---------+");
        for (int i = 0; i < MaxCharactersPerLine - 10; i++) {
            Console.Write("-");
        }
        Console.WriteLine("");

        Console.ResetColor();
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

    private void InsertNewline() {
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

        if (cursorPosition == 0 && buffer[cursorLine] != null && cursorPosition == buffer[cursorLine].Length) {
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
                                  "ESC  -> Interactive mode\n" +
                                  "I    -> Insert mode\n" +
                                  "F4   -> Save and exit the editor\n" +
                                  ":    -> Command mode\n");
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
                                  "s    -> Save to same file\n" +
                                  "clrl -> Clear excess null lines\n");
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
            } else if (input == "clrl") {
                for (int i = buffer.Count - 1; i >= 0; i--) {
                    if (buffer[i] == "") {
                        buffer.RemoveAt(i);
                    } else {
                        break;
                    }
                }
            } else {
                Console.WriteLine("Invalid command");
            }
        }
        Console.ResetColor();
    }

    public static void WriteLineColor(string text, string[] keywords, string quoteColor) {
        foreach (string keyword in keywords) {
            string[] parts = keyword.Split(' ');

            if (parts.Length == 2) {
                string color = parts[0].ToLower();
                string word = parts[1];

                string startTag = GetColorStartTag(color);
                string endTag = GetColorEndTag();

                string pattern = $@"(?<![a-zA-Z0-9]){Regex.Escape(word)}(?![a-zA-Z0-9])";
                text = Regex.Replace(text, pattern, $"{startTag}$&{endTag}");
            }
        }

        string quotePattern = @"(""[^""]*"")|('[^']*')";
        string quoteStartTag = GetColorStartTag(quoteColor);
        string quoteEndTag = GetColorEndTag();

        text = Regex.Replace(text, quotePattern, $"{quoteStartTag}$&{quoteEndTag}");

        Console.WriteLine(text);
    }

    private static string GetColorStartTag(string color) {
        switch (color.ToLower()) {
            case "black":
                return "\u001b[30m";
            case "red":
                return "\u001b[31m";
            case "green":
                return "\u001b[32m";
            case "yellow":
                return "\u001b[33m";
            case "blue":
                return "\u001b[34m";
            case "magenta":
                return "\u001b[35m";
            case "cyan":
                return "\u001b[36m";
            case "white":
                return "\u001b[37m";
            default:
                return "";
        }
    }

    private static string GetColorEndTag() {
        return "\u001b[0m";
    }
}

class Program {
    static void Main(string[] args) {
        TextEditor editor = new TextEditor();
        editor.Run();
    }
}
