﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCaddins.ModelSetupWizard
{
    using System.Collections.Generic;
    using Autodesk.Revit.DB;

    class ModelSetupWizard
    {
        public static void ApplyWorksetModifications
            (Document doc, List<WorksetParameter> worksets)
        {
            // Enable worsharing if required
            if (doc.IsWorkshared == false) {
                doc.EnableWorksharing("Shared Levels and Grids", "Workset1");
            }

            using (Transaction t = new Transaction(doc)) {
                if (t.Start("Add Worksets to Model.") == TransactionStatus.Started) {
                    foreach (var w in worksets) {
                        if (w.IsModified) {
                            if (w.IsExisting && w.Id >= 0)
                            {
                                if (WorksetTable.IsWorksetNameUnique(doc, w.Name))
                                {
                                    WorksetTable.RenameWorkset(doc, new WorksetId(w.Id), w.Name);
                                }
                                var defaultVisibilitySettings = WorksetDefaultVisibilitySettings.GetWorksetDefaultVisibilitySettings(doc);
                                defaultVisibilitySettings.SetWorksetVisibility(new WorksetId(w.Id), w.VisibleInAllViews);
                                continue;
                            }
                            Workset newWorkset = null;
                            if (WorksetTable.IsWorksetNameUnique(doc, w.Name))
                            {
                                newWorkset = Workset.Create(doc, w.Name);
                            }
                            if (newWorkset != null) {
                                var defaultVisibilitySettings = WorksetDefaultVisibilitySettings.GetWorksetDefaultVisibilitySettings(doc);
                                defaultVisibilitySettings.SetWorksetVisibility(newWorkset.Id, w.VisibleInAllViews);
                            }
                        }
                    }
                    t.Commit();
                }
            }
        }

        public static void ApplyProjectInfoModifications
            (Document doc, List<ProjectInformationParameter> projectInformationParameters)
        {
            using (Transaction t = new Transaction(doc)) {
                if (t.Start("Change Project Information Parameter Values") == TransactionStatus.Started) {
                    foreach (var p in projectInformationParameters) {
                        if (p.IsModified) {
                            SetParameterValue(p.GetParameter(), p.Value, doc);
                        }
                    }
                    t.Commit();
                }
            }
        }

        public static void SetParameterValue(Parameter param, string value, Document doc)
        {
            param.Set(value);
        }
    }
}