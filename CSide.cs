using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

class TextEditor {
    private string DecimalCharacters = "0123456789";

    public int largestWindowX = 0;
    public int largestWindowY = 0;

    private int MaxLines = 40;
    private int MaxCharactersPerLine = 150;
    private int padnumber = 0;

    public static List<string> buffer;
    private int cursorLine;
    private int cursorPosition;
    private int saved;
    public static string FILEPATH;
    private int logindex;
    private int infobarcharlines;
    private List<bool> linenumberstatus = new List<bool>();

    private int historytime;
    private int historymaxtime;

    private static List<string> tabs = new List<string>();
    private static int tabnum = 0;
    private static List<string> tabbuffer = new List<string>();
    private static List<int> tabcursorbufferline = new List<int>();
    private static List<int> tabcursorbufferrow = new List<int>();

    public static string textmode = "Interactive";
    public static string additionaltext = "";
    public static string additionalstatus = "";
    public static string additionalstatus2 = "";

    bool highlightingstatus = false;
    public static List<string> highlightkeywords;
    public static string highlightquote = "";

    private string linemode = "DEC";

    public TextEditor() {
        buffer = new List<string>();
        cursorLine = 0;
        cursorPosition = 0;
        logindex = 0;
        saved = 0;
        FILEPATH = "";
        infobarcharlines = 5;
        historytime = 0;
        historymaxtime = 0;
        highlightkeywords = new List<string>();
    }

    public void RunTextEditor() {
        int errorstartup = 0;

        tabs.Add("Editor");
        linenumberstatus.Add(true);

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
        Console.Write("Enter the path of the file ($none.tagsig$ for no file): ");
        string filePath = Console.ReadLine();

        if (filePath == "$none.tagsig$") {
            filePath = "";
            buffer.Add("");
            goto afterfileinit;
        }

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

    afterfileinit:
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

        tabbuffer.Add(convertbuffer(buffer));
        tabcursorbufferline.Add(0);
        tabcursorbufferrow.Add(0);

        while (true) {
            RefreshScreen();

            if (historytime > historymaxtime) {
                historymaxtime = historytime;
            }

            var keyInfo = Console.ReadKey(true);
            var key = keyInfo.Key;
            if (key == ConsoleKey.UpArrow && (textmode == "Insert" || textmode == "Interactive")) {
                MoveCursorUp();
            } else if (key == ConsoleKey.DownArrow && (textmode == "Insert" || textmode == "Interactive")) {
                MoveCursorDown();
            } else if (key == ConsoleKey.RightArrow && (textmode == "Insert" || textmode == "Interactive")) {
                MoveCursorRight();
            } else if (key == ConsoleKey.LeftArrow && (textmode == "Insert" || textmode == "Interactive")) {
                MoveCursorLeft();
            } else if (key == ConsoleKey.PageDown) {
                for (int i = 0; i < MaxLines; i++) {
                    MoveCursorDown();
                }
            } else if (key == ConsoleKey.PageUp) {
                for (int i = 0; i < MaxLines; i++) {
                    MoveCursorUp();
                }
            } else if (key == ConsoleKey.Enter && textmode == "Insert") {
                InsertNewline();
                pushhistory();
            } else if (key == ConsoleKey.F4) {
                Console.Clear();
                SaveFile(0);
                break;
            } else if (key == ConsoleKey.I && (textmode == "Interactive" || textmode == "Tab")) {
                textmode = "Insert";
            } else if (key == ConsoleKey.Escape) {
                textmode = "Interactive";
            } else if (keyInfo.KeyChar == ':' && textmode == "Interactive") {
                commandhandler();
            } else if (key == ConsoleKey.F3) {
                Console.Clear();
                Debugger();
            } else if (key == ConsoleKey.F2) {
                Console.Clear();
                SaveFile(1);
            } else if (key == ConsoleKey.U && textmode == "Interactive") {
                undo();
            } else if (key == ConsoleKey.R && textmode == "Interactive") {
                redo();
            } else if (key == ConsoleKey.Spacebar && textmode == "Insert") {
                InsertCharacter(' ');
                pushhistory();
            } else if (key == ConsoleKey.Backspace && textmode == "Insert") {
                DeleteCharacter();
                pushhistory();
            } else if (key == ConsoleKey.F1 && textmode == "Insert") {
                RefreshScreen();
            } else if (DecimalCharacters.Contains(keyInfo.KeyChar) && textmode == "Interactive") {
                multishift(keyInfo.KeyChar.ToString());
            } else if (key == ConsoleKey.E && textmode == "Interactive") {
                textmode = "Tab";
            } else if (key == ConsoleKey.LeftArrow && textmode == "Tab") {
                try {
                    string throwaway = tabs[tabnum - 1];
                    tabnum--;

                    swapcursor(tabnum, tabnum + 1);
                    swapbuffer(tabnum, tabnum + 1);
                } catch (Exception) {

                }
            } else if (key == ConsoleKey.RightArrow && textmode == "Tab") {
                try {
                    string throwaway = tabs[tabnum + 1];
                    tabnum++;

                    swapcursor(tabnum, tabnum - 1);
                    swapbuffer(tabnum, tabnum - 1);
                } catch (Exception) {

                }
            } else if (key == ConsoleKey.N && textmode == "Tab") {
                int tablength = 0;
                for (int i = 0; i < tabs.Count(); i++) {
                    tablength += tabs[i].Length + 3;
                }
                tablength += 2;

                if (tablength >= MaxCharactersPerLine - 5) {

                } else {
                    tabs.Add("Tab");
                }

                createnewbuffer();
                createnewcursor();
                linenumberstatus.Add(true);
            } else if (key == ConsoleKey.D && textmode == "Tab") {
                try {
                    if (tabnum == 0) {
                        throw new InvalidOperationException("Wrong tab");
                    }

                    if (tabnum + 1 >= tabs.Count()) {
                        swapcursor(tabnum - 1, tabnum);
                        swapbuffer(tabnum - 1, tabnum);
                    } else {
                        swapcursor(tabnum + 1, tabnum);
                        swapbuffer(tabnum + 1, tabnum);
                    }

                    tabbuffer.RemoveAt(tabnum);
                    tabs.RemoveAt(tabnum);
                    tabcursorbufferrow.RemoveAt(tabnum);
                    tabcursorbufferline.RemoveAt(tabnum);

                    if (tabnum >= tabs.Count()) {
                        tabnum--;
                    }
                } catch (Exception) {

                }
            } else if (textmode == "Insert") {
                InsertCharacter(keyInfo.KeyChar);
                pushhistory();
            }
        }
    }

    public void createnewbuffer() {
        tabbuffer.Add("");
    }

    public void swapbuffer(int tobuffer, int frombuffer) {
        try {
            string store = convertbuffer(buffer);

            tabbuffer[frombuffer] = store;
            buffer.Clear();

            string load = tabbuffer[tobuffer].ToString();
            string[] loadarray = load.Split('\n');

            foreach (string temp in loadarray) {
                buffer.Add(temp);
            }
        } catch (Exception) {

        }
    }

    public void createnewcursor() {
        tabcursorbufferline.Add(0);
        tabcursorbufferrow.Add(0);
    }

    public void swapcursor(int tobuffer, int frombuffer) {
        try {
            tabcursorbufferline[frombuffer] = cursorLine;
            tabcursorbufferrow[frombuffer] = cursorPosition;

            cursorLine = tabcursorbufferline[tobuffer];
            cursorPosition = tabcursorbufferrow[tobuffer];
        } catch (Exception) {

        }
    }

    private string convertbuffer(List<string> input) {
        string converted = "";

        try {
            foreach (string line in input) {
                converted += line + "\n";
            }

            converted = converted.Remove(converted.Length - 1, 1);

            return converted;
        } catch (Exception) {
            return converted;
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
        } catch (Exception ex) {
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
            } else {
                temp.Add(false);
            }
        }

        for (int i = buffer.Count - 1; i >= 0; i--) {
            if (temp[i] == true) {
                buffer.RemoveAt(i);
            } else {
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

    private void cleanbuffer() {
        try {
            for (int i = 0; i < buffer.Count(); i++) {
                buffer[i].Replace("\t", "    ");
            }
        } catch (Exception) {

        }
    }

    private void sizeofscreen() {
        largestWindowX = Console.LargestWindowWidth;
        largestWindowY = Console.LargestWindowHeight;
    }

    private void RefreshScreen() {
        if (!buffer.Any()) {
            buffer.Add("");
        }

        try {
            sizeofscreen();

            cleanbuffer();

            Console.Clear();
            saved = 0;

            infobar();

            int visibleLines = Min(Console.WindowHeight, MaxLines);
            int startLine = Max(0, cursorLine - visibleLines + 1 + padnumber);

            try {
                for (int i = startLine; i < startLine + visibleLines; i++) {
                    string line = buffer[i];
                    string displayLineText = line != null ? line.PadRight(MaxCharactersPerLine) : new string(' ', MaxCharactersPerLine);

                    if (linenumberstatus[tabnum] == true) {
                        Console.ForegroundColor = ConsoleColor.Green;
                        if (linemode == "DEC") {
                            string inter = ReverseString(i.ToString("D8")).Substring(0, 8);
                            Console.Write(ReverseString(inter));
                        } else if (linemode == "HEX") {
                            string inter = ReverseString(i.ToString("X8")).Substring(0, 8);
                            Console.Write(ReverseString(inter));
                        } else if (linemode == "BIN") {
                            string bin = Convert.ToString((int)(i & 0xFF), 2).PadLeft(8, '0').Substring(0, 8);
                            Console.Write(bin.PadLeft(8, '0'));
                        } else {
                            string inter = ReverseString(i.ToString("D8")).Substring(0, 8);
                            Console.Write(ReverseString(inter));
                        }
                        Console.Write(" | ");
                        Console.ResetColor();
                    }

                    if (highlightingstatus == false) {
                        Console.WriteLine(displayLineText);
                    } else {
                        WriteLineColor(displayLineText, highlightkeywords.ToArray(), highlightquote);
                    }
                }
            } catch (Exception e) {
                File.AppendAllText("latestlog.txt", Convert.ToString(logindex) + ": " + e + "\n");
                logindex++;
            }

            if (linenumberstatus[tabnum] == true) {
                Console.SetCursorPosition(cursorPosition + 12, cursorLine - startLine);
            } else {
                Console.SetCursorPosition(cursorPosition, cursorLine - startLine);
            }

            // Place cursor at the end of the line
            string currentLine = buffer[cursorLine];
            int lineLength = currentLine != null ? currentLine.Length : 0;
            try {
                if (linenumberstatus[tabnum] == true) {
                    Console.SetCursorPosition(Min(lineLength, cursorPosition) + 11, cursorLine - startLine + infobarcharlines);
                } else {
                    Console.SetCursorPosition(Min(lineLength, cursorPosition), cursorLine - startLine + infobarcharlines);
                }
            } catch (Exception) {

            }
        } catch (Exception) {

        }
    }

    private void commands() {
        if (additionaltext == "s" && tabnum == 0) {
            try {
                File.WriteAllLines(FILEPATH, buffer);
                additionaltext = "File saved successfully, press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
                saved = 1;
            } catch (Exception ex) {
                additionaltext = "Error saving file: " + ex.Message + ", press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
                saved = 1;
            }
        } else if (additionaltext == "q" && tabnum == 0) {
            additionaltext = "s";
            commands();
            Environment.Exit(0);
        } else if (additionaltext == "/DEBUG:historytime") {
            additionaltext = Convert.ToString(historytime) + ", press any key to continue...";
            RefreshScreen();
            Console.ReadKey(true);
        } else if (additionaltext == "/DEBUG:size") {
            additionaltext = "Lines: " + Convert.ToString(MaxLines) +
                             ", Chars: " + Convert.ToString(MaxCharactersPerLine) +
                             ", press any key to continue...";
            RefreshScreen();
            Console.ReadKey(true);
        } else if (additionaltext.Split(' ')[0] == "/DEBUG:setsize") {
            string[] temp = additionaltext.Split(' ');

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
        } else if (additionaltext.Split(' ')[0] == "goto") {
            string[] temp = additionaltext.Split(' ');

            try {
                int tempmax = buffer.Count;
                if (temp[1] == "last") {
                    cursorLine = buffer.Count() - 1;
                } else if (Convert.ToInt32(temp[1]) <= tempmax && Convert.ToInt32(temp[1]) >= 0) {
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
        } else if (additionaltext == "clrl") {
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
        } else if (additionaltext.Split(' ')[0] == "highlight") {
            int done = 0;

        beforefuckery:
            if (done == 1) {
                goto outoffuckery;
            }

            try {
                if (additionaltext.Split(' ')[1] == "default") {
                    highlightingstatus = false;
                    done = 1;
                    goto beforefuckery;
                }

                highlightkeywords.Clear();
                highlightingstatus = true;

                string[] keywords = File.ReadAllLines("highlight/" + (additionaltext.Split(' '))[1]);
                RefreshScreen();

                int quoted = 0;

                foreach (string keywordsin in keywords) {
                    string[] temp = keywordsin.Split(' ');

                    if (temp[0] == "quote") {
                        highlightquote = temp[1];
                        quoted = 1;
                    } else {
                        highlightkeywords.Add(keywordsin);
                    }
                }

                if (quoted == 0) {
                    highlightquote = "white";
                }

                additionaltext = "Loaded: " + additionaltext.Split(' ')[1] + ", press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
            } catch (Exception) {
                additionaltext = "Invalid, press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
            }
        outoffuckery:
            if (done == 1) {
                additionaltext = "Reset highlighting, press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
            }
        } else if (additionaltext.Split(' ')[0] == "linemode") {
            try {
                if (additionaltext.Split(' ')[1] == "HEX") {
                    linemode = "HEX";
                    additionaltext = "Switched to HEX line mode, press any key to continue...";
                    RefreshScreen();
                    Console.ReadKey(true);
                } else if (additionaltext.Split(' ')[1] == "DEC") {
                    linemode = "DEC";
                    additionaltext = "Switched to DEC line mode, press any key to continue...";
                    RefreshScreen();
                    Console.ReadKey(true);
                } else if (additionaltext.Split(' ')[1] == "BIN") {
                    linemode = "BIN";
                    additionaltext = "Switched to BIN line mode, press any key to continue...";
                    RefreshScreen();
                    Console.ReadKey(true);
                } else {
                    additionaltext = "Invalid, press any key to continue...";
                    RefreshScreen();
                    Console.ReadKey(true);
                }
            } catch (Exception) {
                additionaltext = "Invalid, press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
            }
        } else if (additionaltext.Split(' ')[0] == "padnum") {
            try {
                padnumber = Convert.ToInt32(additionaltext.Split(' ')[1]);
                additionaltext = "Padded lines: " + padnumber + ", press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
            } catch (Exception) {
                additionaltext = "Invalid, press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
            }
        } else if (additionaltext.Split(' ')[0] == "linestatus") {
            try {
                if (additionaltext.Split(' ')[1] == "true") {
                    linenumberstatus[tabnum] = true;

                    additionaltext = "Showed lines, press any key to continue...";
                    RefreshScreen();
                    Console.ReadKey(true);
                } else if (additionaltext.Split(' ')[1] == "false") {
                    linenumberstatus[tabnum] = false;

                    additionaltext = "Hid lines, press any key to continue...";
                    RefreshScreen();
                    Console.ReadKey(true);
                } else {
                    additionaltext = "Invalid, press any key to continue...";
                    RefreshScreen();
                    Console.ReadKey(true);
                }
            } catch (Exception) {
                additionaltext = "Invalid, press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
            }
        } else if (additionaltext.Split(' ')[0] == "l" && tabnum == 0) {
            try {
                string file = additionaltext.Substring(2);

                additionaltext = "s";
                commands();

                FILEPATH = file;

                buffer.Clear();

                LoadFile(file);

                if (!buffer.Any()) {
                    buffer.Add("");
                }

                additionaltext = "Loaded: " + file + ", press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
            } catch (Exception) {
                additionaltext = "Invalid, press any key to continue...";
                RefreshScreen();
                Console.ReadKey(true);
            }
        } else if (additionaltext.Split(' ')[0] == "maxsize") {
            MaxLines = largestWindowY - 6;
            MaxCharactersPerLine = largestWindowX - 11;
        } else {
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
            } else if (key == ConsoleKey.Backspace) {
                try {
                    additionaltext = additionaltext.Remove(additionaltext.Length - 1, 1);
                } catch (Exception) {

                }
            } else {
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
        Console.ForegroundColor = ConsoleColor.Cyan;
        for (int i = 0; i < tabs.Count(); i++) {
            Console.Write("| ");
            if (i == tabnum) {
                Console.ForegroundColor = ConsoleColor.Magenta;
            }

            Console.Write(tabs[i] + " ");

            Console.ForegroundColor = ConsoleColor.Cyan;
        }
        Console.WriteLine("|");

        Console.ForegroundColor = ConsoleColor.Green;
        for (int i = 0; i < MaxCharactersPerLine; i++) {
            Console.Write("-");
        }
        Console.WriteLine("");

        //text mode
        Console.WriteLine("[" + textmode + "]" + additionalstatus);
        Console.WriteLine(additionaltext);

        //info bar seperator line
        if (linenumberstatus[tabnum] == true) {
            Console.Write("---------+");
        } else {
            Console.Write("----------");
        }
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
            } else {
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

    private string ReverseString(string s) {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    private void Debugger() {
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.Clear();
        Console.WriteLine("DEBUGGER");
        Console.WriteLine("(!!) WARN: RUNNING CERTAIN COMMANDS MAY SCREW UP YOUR FILES\n");
        while (true) {
            Console.Write("> ");
            string input = Console.ReadLine();

            File.AppendAllText("latestlog.txt", Convert.ToString(logindex) + ": " + input + "\n");
            logindex++;

            if (input == "") {
                // Do nothing lmao
            } else if (input == "help") {
                Console.Write("F1   -> Refresh Screen\n" +
                                  "F2   -> Save\n" +
                                  "F3   -> Debugger\n" +
                                  "ESC  -> Interactive mode\n" +
                                  "I    -> Insert mode\n" +
                                  "F4   -> Save and exit the editor\n" +
                                  ":    -> Command mode\n" +
                                  "U    -> Undo\n" +
                                  "R    -> Redo\n" +
                                  "PgUp -> Page up\n" +
                                  "PgDn -> Page down\n");
            } else if (input == "exit") {
                break;
            } else if (input == "?") {
                Console.Write("help -> All editor keybinds\n" +
                                  "exit -> Exits debugger\n" +
                                  "?    -> This menu\n" +
                                  "dump -> Dump text file\n" +
                                  "run  -> Run a program\n" +
                                  "save -> Save the file\n" +
                                  "load -> Load a file\n" +
                                  "size -> Window size of editor\n" +
                                  "quit -> Quit without saving\n" +
                                  "s    -> Save to same file\n" +
                                  "clrl -> Clear excess null lines\n" +
                                  "cmdh -> Command line help\n" +
                                  "tab  -> Tab information\n");
            } else if (input == "dump") {
                foreach (string temp in buffer) {
                    Console.WriteLine(temp);
                }
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
                } else {
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
                } catch (Exception ex) {
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
            } else if (input == "cmdh") {
                Console.Write("clrl         - Clear excess lines\n" +
                                  "s            - Save to same file\n" +
                                  "q            - Save and quit\n" +
                                  "goto {}      - Goto {} line\n" +
                                  "highlight {} - Loads {} highlight file\n" +
                                  "linemode{}   - Use a different line mode\n" +
                                  "padnum {}    - {} lines the cursor can over/under run\n");
            } else if (input == "tab") {
                Console.WriteLine("Current tab number: " + tabnum);
                Console.WriteLine("Total tab number:" + tabs.Count);
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
        editor.RunTextEditor();
    }
}
