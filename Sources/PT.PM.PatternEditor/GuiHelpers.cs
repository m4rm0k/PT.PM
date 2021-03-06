﻿using Avalonia.Controls;
using PT.PM.Common.Exceptions;

namespace PT.PM.PatternEditor
{
    internal class GuiHelpers
    {
        internal static void ProcessErrorOnDoubleClick(ListBox errorsListBox, TextBox inputTextBox)
        {
            errorsListBox.Focus();
            int selectionStart = -1, selectionEnd = -1;

            if (errorsListBox.SelectedItem is ParsingException parsingException)
            {
                selectionStart = parsingException.TextSpan.Start;
                selectionEnd = parsingException.TextSpan.End;
            }
            else if (errorsListBox.SelectedItem is ConversionException conversionException)
            {
                var textSpan = conversionException.TextSpan;
                if (textSpan != null)
                {
                    selectionStart = textSpan.Start;
                    selectionEnd = textSpan.End;
                }
            }
            else if (errorsListBox.SelectedItem is MatchingException matchException)
            {
                var textSpan = matchException.TextSpan;
                if (textSpan != null)
                {
                    selectionStart = textSpan.Start;
                    selectionEnd = textSpan.End;
                }
            }

            if (selectionStart != -1)
            {
                inputTextBox.SelectionStart = selectionStart;
                inputTextBox.SelectionEnd = selectionEnd;
                inputTextBox.CaretIndex = inputTextBox.SelectionEnd + 1;
            }
        }
    }
}
