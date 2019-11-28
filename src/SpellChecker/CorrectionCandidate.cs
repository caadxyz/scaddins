﻿namespace SCaddins.SpellChecker
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Autodesk.Revit.DB;
    using NHunspell;

    public class CorrectionCandidate : IEnumerator
    {
        private Dictionary<string, string> autoReplacementList;
        private int currentIndex;
        private Hunspell hunspell;
        private string[] originalWords;
        private Parameter parameter;
        private Regex rgx;

        public CorrectionCandidate(Parameter parameter, Hunspell hunspell, ref Dictionary<string, string> autoReplacementList)
        {
            this.parameter = parameter;
            this.hunspell = hunspell;
            this.autoReplacementList = autoReplacementList;
            OriginalText = parameter.AsString();
            char[] delimiterChars = { ' ', ',', '.', ':', '\t', '\\', '/', '(', ')', '<', '>' };
            if (!string.IsNullOrEmpty(OriginalText)) {
                originalWords = OriginalText.Split(delimiterChars);
                NewText = OriginalText;
            } else {
                originalWords = null;
                NewText = OriginalText;
            }
            currentIndex = -1;
            rgx = new Regex(@"^.*[\d]+.*$");
        }

        public object Current => originalWords[currentIndex].Trim();

        public bool IsModified => !string.Equals(this.OriginalText, this.NewText, System.StringComparison.CurrentCulture);

        public string NewText {
            get; private set;
        }

        public string OriginalText {
            get; private set;
        }

        public string TypeString => parameter.Element.GetType().ToString();

        private string CurrentAsString => (string)Current;

        public bool MoveNext()
        {
            while (originalWords != null && currentIndex < (originalWords.Length - 1)) {
                currentIndex++;

                // continue if a number is found...
                if (rgx.IsMatch(CurrentAsString)) {
                    continue;
                }

                if (autoReplacementList.ContainsKey(CurrentAsString)) {
                    string replacement;
                    if (autoReplacementList.TryGetValue(CurrentAsString, out replacement)) {
                        ReplaceCurrent(replacement);
                        currentIndex++;
                        continue;
                    }
                }

                if (!hunspell.Spell(originalWords[currentIndex].Trim())) {
                    return true;
                }
            }
            return false;
        }

        public bool Rename()
        {
            if (IsModified) {
                if (!parameter.IsReadOnly) {
                    return parameter.Set(NewText);
                }
            }
            return false;
        }

        public void ReplaceCurrent(string word)
        {
            NewText = ReplaceFirst(NewText, (string)Current, word);
        }

        public void Reset()
        {
            currentIndex = -1;
        }

        private string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0) {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}
