using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[Serializable]
public class EditorView 
{

    protected PythonBase CurrentTarget;

    public Code Buffer;

    [SerializeField]
    private Rect HighLine, HighLightSelection, LayoutRect, PositionSyntax;

    [SerializeField]
    private Vector2 PositionScroll = Vector2.zero;

    [SerializeField]
    private Vector2 Padding    = new Vector2(44, 15);

    [SerializeField]
    private Vector2 FontSizeXY = new Vector2(9, 19);

    [SerializeField]
    private int FocusID;

    [SerializeField]
    private bool isSelection, Focused;

    public bool InterpreterView;

    private float Bottom;

    private Interpreter PythonMachine = new Interpreter();

    /// <summary>
    /// Use for match code
    /// </summary>
    private const string MatchCode = @"(\t)|(\w+)|(\s+)|(.)";

    private EditorViewStyles Style = new EditorViewStyles();

    //Delegate For Repaint Inspector.
    public delegate void CodeRepaint();

    public event CodeRepaint RepaintAction;

    /// <summary>
    /// Enable
    /// </summary>
    public void OnEnable(UnityEngine.Object ObjectReference)
    {

        CurrentTarget = (PythonBase)ObjectReference;

        if(Buffer == null)
            Buffer = ScriptableObject.CreateInstance<Code>();

        if(CurrentTarget.InMemory) 
            //Load buffer from Memory
            Buffer = EditorDataBase.Instance.GetInstance(CurrentTarget.GetInstanceID());

        else if(CurrentTarget.FileCreated)
            //Load buffer from file
            LoadFileStream();
        else
            //Load buffer by default
            Buffer.Initialize();
    }

    private void LoadFileStream()
    {
        try {
            StreamReader file = new StreamReader(CurrentTarget.FilePath, 
                    System.Text.Encoding.UTF8);

            Buffer.CodeBuffer = file.ReadToEnd();
        } catch(Exception ex) {
            //Show Message error
            PythonInspector.DialogError(ex.Message);

            CurrentTarget.Reset();
        }
    }

    /// <summary>
    /// Editors the view controll.
    /// </summary>
    public void EditorViewGUI(bool IsInterpreter)
    {
        //Get box rect and background
        GetBoxRect();
        //Begin ScrollView of box
        PositionScroll = GUI.BeginScrollView(new Rect(0, LayoutRect.y, LayoutRect.width + 15, LayoutRect.height),   
                PositionScroll, new Rect(0, LayoutRect.yMin, 800, 23 * Buffer.TotalLines));         
        //Draw Line Numbers.
        EventRepainted();
        //Draw Cursor for text
        Cursor();
        //HighLight Current Line.
        HighlighLine();
        //HightLigh Selection Text
        HighlighSelected();

        GUI.EndScrollView();

        if(InterpreterView) 
            DrawInterpreter();
        //KeyBoard Events
        KeyBoardController();
    }

    /// <summary>
    /// Events the repainted.
    /// </summary>
    private void EventRepainted()
    {
        if(Event.current.type == EventType.repaint) 
        {
            if(!InterpreterView)
                //Draw Code on Inspector
                DrawCodeOnGUI();
            else
                DrawInterpreterOnGUI();

            //Draw Number of Lines
            LineNumbers();

            //Trim Column to end of lines
            Buffer.Trim();

        }
    }

    private void GetBoxRect()
    {
        //Code Rect Layout
        PositionSyntax = LayoutRect = GUILayoutUtility.GetRect(0, Screen.width, 1, Screen.height - Padding.y);

        //Background ColorScheme
        GUI.Box(LayoutRect, GUIContent.none, Style.Background);

        //Bottom value of box
        Bottom = LayoutRect.yMax-40;
    }

    /// <summary>
    /// Draws code on Inspector
    /// </summary>
    private void DrawCodeOnGUI()
    {
        if(Buffer.Lines.Count == 0) {
            Buffer.Lines = new List<List<string>>();

            using(StringReader readerLine = new StringReader(Buffer.CodeBuffer)) {

                string line = string.Empty;

                while((line = readerLine.ReadLine()) != null) {

                    List<string> words = new List<string>();

                    Regex pattern = new Regex(MatchCode);

                    foreach(Match results in pattern.Matches(line))
                        words.Add(results.Value);

                    Buffer.Lines.Add(words);
                }
            }

            if(Buffer.CodeBuffer == string.Empty)
                Buffer.Lines.Add(new List<string>());
        }

        Buffer.TotalLines = 1;
        PositionSyntax.y += 5;

        Style.BlockComment = false;

        for(int i = 0; i < Buffer.Lines.Count; i++) {

            PositionSyntax.x = Padding.x;

            //Reset Lines styles
            Style.ResetLineStyles();

            for(int j = 0; j < Buffer.Lines[i].Count; j++) {

                string word = TabToSpace(Buffer.Lines[i][j]);

                PositionSyntax.width = FontSizeXY.x * word.Length;

                Style.FontGUIStyle.normal.textColor = Style.CheckWordStyle(word);

                //Draw word in GUI Label
                GUI.Label(PositionSyntax, word, Style.FontGUIStyle);

                PositionSyntax.x += PositionSyntax.width;

            }
            Buffer.TotalLines++;
            PositionSyntax.y += FontSizeXY.y;
        }
    }

    /// <summary>
    /// Draws the interpreter on GU.
    /// </summary>
    private void DrawInterpreterOnGUI()
    {
        PositionSyntax.y = Bottom;

        PositionSyntax.x = 45;

        GUI.Label(PositionSyntax,TabToSpace(Buffer.CurrentLine),Style.FontGUIStyle);

        PositionSyntax.y -= FontSizeXY.y + 10;

        using(StringReader readerLine = new StringReader(Buffer.InterpreterBuffer)) {

            string line = string.Empty;

            while((line = readerLine.ReadLine()) != null) {

                PositionSyntax.x = 20;

                line = TabToSpace(line);

                PositionSyntax.width = FontSizeXY.x * line.Length;

                GUI.Label(PositionSyntax,line,Style.FontGUIStyle);

                PositionSyntax.y -= FontSizeXY.y;

            }
        }
    }

    private void DrawInterpreter()
    {
        isSelection = false;

        Rect Pointer = new Rect(18,Bottom,50,10);

        Style.FontGUIStyle.normal.textColor = Color.white;

        //Separate line
        GUI.Box(new Rect(0, Bottom-7, Screen.width , 1), GUIContent.none, Style.BackgroundLines);
        GUI.Label(Pointer, Buffer.BlockInspector ? "..." : ">>>", Style.Interpreter);
    }

    public void InitializeInterpreter()
    {
        float PointerX = Mathf.Max(Padding.x+15, Event.current.mousePosition.x);

        Vector2 VectorXY = ToNumberLine(PointerX, 0);

        Buffer.Line   = 0;
        Buffer.TotalLines = 0;
        Buffer.Column = (int)VectorXY.x;
        Buffer.SetColumnIndex();

        Repaint();
    }

    /// <summary>
    /// Draw Line Numbers
    /// </summary>
    private void LineNumbers()
    {
        //Background Lines
        GUI.Box(new Rect(PositionScroll.x, LayoutRect.y, InterpreterView ? 15 : 40 , Screen.height + PositionScroll.y), GUIContent.none, Style.BackgroundLines);

        if(InterpreterView)
            return;

        Rect RectLineNumbers = new Rect(PositionScroll.x + 3, LayoutRect.y + 5, 30, LayoutRect.height - Padding.y);
        for(int i = 1; i <= Buffer.TotalLines+(int)(PositionScroll.y / Buffer.TotalLines-1); i++) {

            //Draw number.
            Style.NumberLines.Draw(RectLineNumbers, new GUIContent(i.ToString()), true, false, false, false);
            //Increase line height.
            RectLineNumbers.y += FontSizeXY.y;

        }
    }

    /// <summary>
    /// Cursors
    /// </summary>
    public void Cursor()
    {
        //Cursor for Editing Text.
        EditorGUIUtility.AddCursorRect(new Rect(LayoutRect.x, LayoutRect.y, LayoutRect.width + PositionScroll.x, LayoutRect.height - 15), MouseCursor.Text);

        if(!isSelection) {

            Vector2 PositionCursor = ToPixelLine(new Vector2(Buffer.Column,Buffer.Line));

            Rect CursorRect = new Rect(PositionCursor.x, !InterpreterView ? PositionCursor.y : Bottom, 1, FontSizeXY.y);    

            GUI.Box(CursorRect, GUIContent.none, Style.Cursor);

        }
    }

    /// <summary>
    /// Highlight line clicked
    /// </summary>
    private void HighlighLine()
    {
        if(Event.current.type == EventType.MouseDown)
        {

            if(!InterpreterView) {
                //Mouse Position X Y
                float PointerX = Event.current.mousePosition.x;
                float PointerY = Event.current.mousePosition.y;

                Vector2 VectorXY = ToNumberLine(PointerX, PointerY); 

                Buffer.Initialize((int)VectorXY.y, (int)VectorXY.x);

                isSelection = false;

                Repaint();
            }
        }

        if(InterpreterView)
            return;

        float LinePixel = ToPixelLine(new Vector2(Buffer.Column,Buffer.Line)).y;
        HighLine = new Rect(0, LinePixel, Screen.width, FontSizeXY.y);

        HighLine.width = Screen.width + PositionScroll.x;

        if(!isSelection)
            GUI.Box(HighLine, GUIContent.none, Style.HighLine);

    }

    /// <summary>
    /// Highlighs the selected.
    /// </summary>
    private void HighlighSelected()
    {
        //Double Cliked (select a single word)
        if(Event.current.type == EventType.MouseDown && Event.current.clickCount == 2)
        {
            isSelection = true;

            string LineSpace = TabToSpace(Buffer.CurrentLine);

            //Selected Word
            int   begin = 0, index = 0;
            float width = 0;
            //Extract single word
            foreach(Match word in Regex.Matches(LineSpace,MatchCode)) {
                if(width == 0)
                    for(int i = 0;  i < word.Length; i++) {

                        //Begin of word
                        begin = i     == 0 ? index : begin; 
                        //word width
                        width = index == Buffer.Column ? word.Length : 0;

                        index = width == 0 ? index+1 : index;
                    }
            }

            Vector2 Pixels = ToPixelLine(new Vector2(begin,Buffer.Line));
            width *= FontSizeXY.x;

            HighLightSelection = new Rect(Pixels.x,Pixels.y,width,FontSizeXY.y);

        }
        //TODO
        if(Event.current.type == EventType.dragUpdated) { }

        //Draw Selection
        if(isSelection)
            GUI.Box(HighLightSelection, GUIContent.none, Style.HighLine);
    }

    /// <summary>
    /// Focus of Code Editor
    /// </summary>
    private void FocusControl()
    {
        //TODO: FIX!
        GUIUtility.keyboardControl = FocusID;

        FocusID = GUIUtility.GetControlID(Math.Abs(GetHashCode()), FocusType.Keyboard);

        GUIUtility.keyboardControl = Focused ? FocusID : GUIUtility.keyboardControl;

        Focused = (FocusID > 0) ? 
            (GUIUtility.keyboardControl == FocusID) : false;

    }

    /// <summary>
    /// Keies the board controller.
    /// </summary>
    public void KeyBoardController()
    {
        Event e = Event.current;

        if(e.type == EventType.keyDown)
        {

            switch(e.keyCode) 
            {           
                case KeyCode.Backspace:

                    if(!isSelection)
                        Buffer.RemoveText();
                    else
                        Buffer.RemoveRange(GetRangeText(HighLightSelection,Buffer.CurrentLine));


                    SetChanges();

                    isSelection = false;
                    break;

                case KeyCode.Return:
                case KeyCode.KeypadEnter:

                    if(InterpreterView) {
                        ExecuteInterpreter();
                        return;
                    }
                    break;

                case KeyCode.UpArrow:

                    Buffer.GoUp();

                    break;

                case KeyCode.DownArrow:
                    Buffer.GoDown();
                    break;

                case KeyCode.LeftArrow:
                    Buffer.GoLeft();
                    break;

                case KeyCode.RightArrow:
                    Buffer.GoRight();
                    break;

                    //Get any Key.
                case KeyCode.None:
                    char c = Convert.ToChar(e.character.ToString());
                    c = e.shift ? char.ToUpper(c) : c;
                    //Remove Text if has selection text.
                    if(isSelection)
                    {
                        Buffer.RemoveRange(GetRangeText(HighLightSelection,Buffer.CurrentLine));
                        isSelection = false;
                    }
                    if(!InterpreterView)
                        Buffer.InsertText(c);

                    else if(c != '\n')
                        Buffer.InsertTextInterpreter(c);

                    SetChanges();
                    break;

            }

            e.Use();
        }
    }

    /// <summary>
    /// Executes the interpreter.
    /// </summary>
    private void ExecuteInterpreter()
    {
        string line = Buffer.CurrentLine.TrimEnd();
        // Append and return if is a block code
        if(!String.IsNullOrEmpty(line) &&
           line.Substring(line.Length-1, 1) == ":" && !Buffer.BlockInspector) {
            Buffer.InterpreterBlock = line;
            Buffer.AppendInterpreter("");
            Buffer.BlockInspector = true;
            return;
        }
        if(!Buffer.BlockInspector) {
            //Executes a single line
            string output = PythonMachine.Compile(Buffer.CurrentLine);
            Buffer.AppendInterpreter(output);

        }else {
            //Block Code (eg.: if 5 < 10:    )
            if(string.IsNullOrEmpty(line)) {
                //Execute block when enter a blank line
                string output = PythonMachine.Compile(Buffer.InterpreterBlock);
                Buffer.AppendInterpreter(output);
                //Empties current block
                Buffer.InterpreterBlock = string.Empty;
                Buffer.BlockInspector = false;
            }else {
                //stores current block code.
                Buffer.InterpreterBlock += "\n" + line;
                Buffer.AppendInterpreter("");
            }
        }
    }

    /// <summary>
    /// Sets the changes.
    /// </summary>
    private void SetChanges()
    {
        if(!InterpreterView) {
            CurrentTarget.Saved = false;
            CurrentTarget.HasChanges = true;
            Buffer.SaveCodeToBuffer();
        }
    }
    /// <summary>
    /// Gets the range text selected
    /// </summary>
    /// <returns>Array int X Y</returns>
    /// <param name="range">Range of Selection</param>
    /// <param name="text">Text</param>
    private int[] GetRangeText(Rect range, string text)
    {
        //Transform selection rect to coordenadies column number
        Vector2 RangeMin = ToNumberLine(range.xMin, range.y);
        Vector2 RangeMax = ToNumberLine(range.xMax, range.y);

        int begin = Buffer.GetIndexColumn((int)RangeMin.x,text);
        int end   = Buffer.GetIndexColumn((int)RangeMax.x,text);

        return new int[]{ begin, end };
    }

    /// <summary>
    /// Convert mouse position to line number
    /// </summary>
    /// <returns>Return PositionXY with Column Line Number (X) and Number Line (y)</returns>
    public Vector2 ToNumberLine(float column, float line)
    {
        line   = Math.Min((line - LayoutRect.y + Padding.y) / FontSizeXY.y,Buffer.TotalLines-1);

        column = (column - Padding.x) / FontSizeXY.x;

        line = line == 0 ? 1 : line;

        return new Vector2(column,line);
    }

    /// <summary>
    /// Convert Line Number to Pixel for Inspector.
    /// </summary>
    /// <returns>The pixel line.</returns>
    /// <param name="column">Column.</param>
    public Vector2 ToPixelLine(Vector2 PositionLine)
    {
        //Calculate the column position times the font size plus padding spacing;
        int Column = (int)((FontSizeXY.x  * PositionLine.x)   + Padding.x);
        int Line   = (int)((FontSizeXY.y  * PositionLine.y+1) + LayoutRect.y - Padding.y);

        //Limit the column position to padding spacing;
        Column = (int)(Column < Padding.x ? Padding.x : Column);

        //Limit begin of line
        Line = (int)(Line   < LayoutRect.yMin ? LayoutRect.yMin+5 : Line);

        return new Vector2(Column,Line);
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
    /// Repaint inspector
    /// </summary>
    private void Repaint()
    {
        if(RepaintAction != null) 
            RepaintAction();
    }

}
