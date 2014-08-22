using System;
using IronPython.Hosting;
using IronPython.Modules;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

/// <summary>
/// Exceptions of ironpython
/// </summary>
public class ErrorHandle : ErrorListener {

    public string Message  { get; set; }
    public int ErrorCode   { get; set; }
    public Severity Sev    { get; set; }
    public SourceSpan Span { get; set; }

    public override void ErrorReported(ScriptSource source, string message, Microsoft.Scripting.SourceSpan span, int errorCode, Microsoft.Scripting.Severity severity)
    {
        Message   = message;
        ErrorCode = errorCode;
        Sev       = severity;
        Span      = span;
    }
}
