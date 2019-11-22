﻿namespace SCaddins.SpellChecker
{
    using Autodesk.Revit.DB;

    public class CorrectionCandiate : SCaddins.RenameUtilities.RenameCandidate
    {
        public CorrectionCandiate(Parameter parameter) : base(parameter)
        {

        }

        public CorrectionCandiate(TextElement note) : base(note)
        {

        }


    }
}