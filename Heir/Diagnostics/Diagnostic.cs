﻿using Heir.Syntax;
using Spectre.Console;

namespace Heir.Diagnostics;

// this code is so shit
public sealed class Diagnostic(SourceFile sourceFile, DiagnosticCode code, string message, Location startLocation, Location endLocation, DiagnosticLevel level)
{
    public DiagnosticCode Code { get; } = code;
    public string Message { get; } = message;
    public Location StartLocation { get; } = startLocation;
    public Location EndLocation { get; } = endLocation;
    public DiagnosticLevel Level { get; } = level;

    public string ToString(bool colors)
    {
        var levelDisplay = Level.ToString().ToLower();
        var errorCodeDisplay = Code.ToString();
        var messageDisplay = colors ? Markup.Escape(Message) : Message;
        var locationDisplay = StartLocation.ToString();

        var erroneousCodeColumnLength = EndLocation.Column - StartLocation.Column;
        var erroneousCodeDistance = EndLocation.Column - erroneousCodeColumnLength - 1;
        var erroneousCodeLineLength = EndLocation.Line - (StartLocation.Line - 1);
        var lines = sourceFile.Source
            .Split('\n')
            .Skip(StartLocation.Line - 1)
            .Take(erroneousCodeLineLength)
            .ToList();
            
        var padding = string.Join("", Enumerable.Repeat(' ', EndLocation.Line.ToString().Length));
        var codeDisplay = "";
        var lineNumber = 0;
        foreach (var line in lines)
        {
            var offset = lineNumber++;
            var trimmed = line.Trim();
            codeDisplay += $"{StartLocation.Line + offset}{padding} | {(line != trimmed ? " " + trimmed : trimmed)}\n";
        }

        codeDisplay += $" {padding}    {(erroneousCodeDistance <= 0 ? "" : string.Join("", Enumerable.Repeat(' ', erroneousCodeDistance)))}{(erroneousCodeColumnLength <= 0 ? "" : string.Join("", Enumerable.Repeat('~', erroneousCodeColumnLength)))}\n";

        if (colors)
        {
            errorCodeDisplay = $"[grey]{errorCodeDisplay}[/]";
            messageDisplay = $"[silver]{messageDisplay}[/]";
            locationDisplay = $"[silver]{locationDisplay}[/]";
            codeDisplay = "";
            lineNumber = 0;
            foreach (var line in lines)
            {
                var offset = lineNumber++;
                var trimmed = line.Trim();
                codeDisplay += $"[invert white]{StartLocation.Line + offset}{padding}[/] [grey]|[/] {(offset == 0 ? "[white]" : "[grey58]")}{Markup.Escape(line != trimmed ? " " + trimmed : trimmed)}[/]\n";
            }

            codeDisplay += $" {padding}    [red]{(erroneousCodeDistance <= 0 ? "" : string.Join("", Enumerable.Repeat(' ', erroneousCodeDistance)))}{(erroneousCodeColumnLength <= 0 ? "" : string.Join("", Enumerable.Repeat('~', erroneousCodeColumnLength)))}[/]\n";

            switch (Level)
            {
                case DiagnosticLevel.Error:
                    levelDisplay = $"[bold red]{levelDisplay}[/]";
                    break;
                case DiagnosticLevel.Warn:
                    levelDisplay = $"[bold yellow]{levelDisplay}[/]";
                    break;
            }
        }

        return $"{locationDisplay} - {levelDisplay} {errorCodeDisplay}: {messageDisplay}\n{codeDisplay}";
    }
    
    private static int CountSubstrings(string str, string substring)
    {
        var count = 0;
        var index = 0;
    
        while ((index = str.IndexOf(substring, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += substring.Length;
        }
    
        return count;
    }
}
    
