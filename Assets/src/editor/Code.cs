using System;
using System.Text;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Code.
/// </summary>
[Serializable]
public class Code : ScriptableObject
{
    /// <summary>
    /// Current Number Line
    /// </summary>
    public int Line;

    /// <summary>
    /// Current Number Column;
    /// </summary>
    public int Column;

    /// <summary>
    /// Current Number Column Index
    /// </summary>
    public int ColumnIndex;

    /// <summary>
    /// The total lines.
    /// </summary>
    public int TotalLines = 1;

    /// <summary>
    /// List with all lines and all words
    /// </summary>
    public List<List<string>> Lines = new List<List<string>>();

    public string CodeBuffer        = string.Empty;
    public string CurrentLine       = string.Empty;
    public string InterpreterBuffer = string.Empty;
    public string InterpreterBlock  = string.Empty;

    public bool InterpreterView;
    public bool BlockInspector;

    /// <summary>
    /// Initialize.
    /// </summary>
    /// <param name="obj">Object.</param>
    public Code Initialize()
    {
        this.Lines = new List<List<string>>();
        this.CurrentLine = string.Empty;
        this.CodeBuffer  = PythonBase.DefaultCode;

        SetColumnIndex();

        return this;
    }

    /// <summary>
    /// Initialize the specified Line and Column.
    /// </summary>
    /// <param name="Line">Line.</param>
    /// <param name="Column">Column.</param>
    public void Initialize(int Line, int Column)
    {
        this.Line   = Line;
        this.Column = Column;
        this.CurrentLine = GetLine(this.Line);
        SetColumnIndex();
    }

    public void OnEnable()
    {
        hideFlags = HideFlags.HideAndDontSave;
    }

    /// <summary>
    /// Sets the index of the column.
    /// </summary>
    public void SetColumnIndex()
    {   
        Trim();

        ColumnIndex = GetIndexColumn(this.Column,this.CurrentLine);

        ColumnIndex =   CurrentLine.Length == 0    ?     0 
        :       ColumnIndex > CurrentLine.Length   ?     CurrentLine.Length 
        :       ColumnIndex;
    }

    /// <summary>
    /// Gets the index column.
    /// </summary>
    /// <returns>The index column.</returns>
    /// <param name="column">Column.</param>
    public int GetIndexColumn(int column, string line)
    {
        if(column == 0)
            return 0;   

        int index = 0;
        for(int i = 0; i <= line.Length; i++) {

			if(line.Length > 0) {
                index = line[i] == '\t' ? index+4 : index+1;
                if(index >= column)
                    return ++i;
			}
        }

        return 0;
    }

    /// <summary>
    /// Gos up.
    /// </summary>
    public void GoUp()
    {
        Line--;

        CurrentLine = GetLine(Line);

        SetColumnIndex();
    }

    /// <summary>
    /// Gos down.
    /// </summary>
    public void GoDown()
    {
        Line++;

        CurrentLine = GetLine(Line);

        SetColumnIndex();
    }

    /// <summary>
    /// Gos the left.
    /// </summary>
    public void GoLeft()
    {

        if(Column == 0)
            Line--;

        if(Event.current.command)
            //Go to begin of line
            Column = Regex.Match(TabToSpace(CurrentLine),@"\w").Index;
        else {

            Column = Column > 0 ? Column-1: GetLine(Line).Length;

            Column = GetCharIndex(ColumnIndex-1) == '\t' ? Column-3 : Column;
        }

        SetColumnIndex();
    }

    /// <summary>
    /// Gos the right.
    /// </summary>
    public void GoRight()
    {
        if(Event.current.command)
            //Go to end of line 
            Column = TabToSpace(CurrentLine).Length;
        else if(ColumnIndex >= CurrentLine.Length && !InterpreterView) {    
            GoDown();
            Column = Regex.Match(TabToSpace(CurrentLine),@"\w").Index;
        }
        else
            Column = GetCharIndex(ColumnIndex) == '\t' ? Column+4 : Column+1;

        SetColumnIndex();       
    }

    /// <summary>
    /// Inserts the line.
    /// </summary>
    /// <param name="NumberLine">Number line.</param>
    public void InsertLine(int NumberLine)
    {
        this.Lines.Insert(NumberLine,new List<string>());
    }

    /// <summary>
    /// Inserts the text.
    /// </summary>
    /// <param name="c">char</param>
    public void InsertText(char c)
    {       
        string StringJoin = this.CurrentLine;

        //Insert new Line
        if(c == '\n') {

            string LeftText  = string.Empty;
            string RightText = string.Empty;

            for(int i = 0; i < StringJoin.Length; i++) {
                if(i >= this.ColumnIndex)
                    RightText += StringJoin[i];
                else
                    LeftText  += StringJoin[i];
            }

            Match spaces = Regex.Match(LeftText, @"(^\t+)|(^\s+)");

            RightText = RightText.Insert(0,spaces.Value);

            UpdateLineText(Line,LeftText);

            InsertLine(Line);

            UpdateLineText(++Line,RightText);

            TotalLines++;

            //Jump cursor after indentation.
            Match Indentation = Regex.Match(TabToSpace(LeftText),@"\w");
            Column = Indentation.Success ? Indentation.Index : Column;
            SetColumnIndex();

        }else {

            //Insert a single char          
            string newChar = (c.ToString());

            StringJoin = StringJoin.Insert(ColumnIndex, newChar);

            UpdateLineText(Line,StringJoin);

            Column = c == '\t' ? Column + 4 : ++Column;
            SetColumnIndex();
        }
    }

    /// <summary>
    /// Inserts the text interpreter.
    /// </summary>
    /// <param name="c">char</param>
    public void InsertTextInterpreter(char c)
    {
        CurrentLine = CurrentLine.Insert(ColumnIndex,c.ToString());
        Column = c == '\t' ? Column + 4 : ++Column;
        SetColumnIndex();
    }

    /// <summary>
    /// Appends the interpreter.
    /// </summary>
    /// <param name="output">Output string</param>
    public void AppendInterpreter(string output)
    {
        StringBuilder sb = new StringBuilder("\n"+InterpreterBuffer);
        string blockSeparator = this.BlockInspector ? "..." : ">>> ";
        string BreakLine      = String.IsNullOrEmpty(output) ? "" : "\n";
        sb.Insert(0, output + BreakLine + blockSeparator + CurrentLine);

        InterpreterBuffer = sb.ToString();
        CurrentLine = string.Empty;
        SetColumnIndex();
    }

    /// <summary>
    /// Remove one char from line
    /// </summary>
    public void RemoveText()
    {
        if(!ElementInList(Line) && !InterpreterView)
            return;

        if(Column > 0) {
            //Remove single char.
            GoLeft();
            string LineTab = CurrentLine;

            LineTab = LineTab.Remove(Math.Max(0,ColumnIndex),1);
            UpdateLineText(Line,LineTab);           

        }else if(!InterpreterView && Line != 1) {
            //Remove line.
            Lines[Line-2].Add(CurrentLine);

            Lines.RemoveAt(Line-1);
            GoUp();

            Column = GetLine(Line-1).IndexOf(CurrentLine);
            SetColumnIndex();
        }
    }

    /// <summary>
    /// Removes the range of text
    /// </summary>
    public void RemoveRange(int[] range)
    {
        CurrentLine = (CurrentLine).Remove(range[0],range[1]-range[0]);

        UpdateLineText(Line,CurrentLine);

        Column = range[0] + Regex.Matches(CurrentLine.Substring(0,range[0]),@"(\t)").Count*3;
        SetColumnIndex();
    }

    /// <summary>
    /// Updates the line text.
    /// </summary>
    /// <param name="LineNumber">Line number.</param>
    /// <param name="LineText">Line text.</param>
    private void UpdateLineText(int NumberLine, string LineText)
    {   
        if(InterpreterView) {
            CurrentLine = LineText;
            return;
        }

        if(!ElementInList(NumberLine))
            return;

        Lines[NumberLine-1] = new List<string>();

        foreach(Match results in Regex.Matches(LineText,@"(\t)|(\w+)|(\s+)|(.)"))
            Lines[NumberLine-1].Add(results.Value);

        CurrentLine = LineText;
    }

    /// <summary>
    /// Saves the code to memory.
    /// </summary>
    public void SaveCodeToBuffer()
    {
        StringBuilder text = new StringBuilder();

        foreach(List<String> Line in Lines) {   
            foreach(string word in Line)
                text.Append(word);

            text.AppendLine();
        }

        CodeBuffer = text.ToString();   
    }

    /// <summary>
    /// Gets the index of the char.
    /// </summary>
    /// <returns>The char index.</returns>
    /// <param name="index">Index.</param>
    public char GetCharIndex(int index)
    {
        return CurrentLine.ElementAtOrDefault(index);
    }

    /// <summary>
    /// Gets the line.
    /// </summary>
    /// <returns>The line.</returns>
    /// <param name="NumberLine">Number line.</param>
    public string GetLine(int NumberLine)
    {
        if(!ElementInList(NumberLine))
            return string.Empty;

        return string.Join("",Lines[NumberLine-1].ToArray());
    }

    /// <summary>
    /// Elements the in list.
    /// </summary>
    /// <returns><c>true</c>, if in list was elemented, <c>false</c> otherwise.</returns>
    /// <param name="NumberLine">Number line.</param>
    private bool ElementInList(int NumberLine)
    {
        return this.Lines.ElementAtOrDefault(NumberLine-1) != null;
    }

    /// <summary>
    /// Limit Column of Cursor
    /// </summary>
    /// <param name="Position">Column and Line position.</param>
    public void Trim()
    {
        string line = CurrentLine.Replace("\t", "    ");

        if(!InterpreterView) {
            Line = Line >= TotalLines ? TotalLines-1  : Line;

            Column = Column > line.Length ? line.Length : Column;

        } else 
            Column = Column > line.Length ? CurrentLine.Length : Column;

    }

    /// <summary>
    /// Replace Tabs to whitespaces
    /// </summary>
    /// <param name="value">Value.</param>
    private string TabToSpace(string value)
    {
        return value.Replace("\t", "    ");
    }

    /// <summary>
    /// String Class representation
    /// </summary>
    /// <returns>string represented the current instance</returns>
    public override string ToString()
    {
        return string.Format("Line: {0}, Column: {1}, ColumnIndex: {2}, CurrentLine: {3}, TotalLine: {4}", Line, Column, ColumnIndex, CurrentLine, TotalLines);
    }

}
