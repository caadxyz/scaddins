﻿// (C) Copyright 2013-2018 by Andrew Nicholas
//
// This file is part of SCaddins.
//
// SCaddins is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SCaddins is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SCaddins.  If not, see <http://www.gnu.org/licenses/>.

namespace SCaddins.ExportManager
{
    using System;
    using System.Collections.Generic;
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    //using Caliburn.Micro;
    using System.Dynamic;

    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
   [Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
   [Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class OpenSheet : IExternalCommand
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public Autodesk.Revit.UI.Result Execute(
            ExternalCommandData commandData,
            ref string message,
            Autodesk.Revit.DB.ElementSet elements)
        {
            if (commandData == null) {
                return Autodesk.Revit.UI.Result.Failed;
            }

            Document doc = commandData.Application.ActiveUIDocument.Document;

            dynamic settings = new ExpandoObject();
            settings.Width = 640;
            settings.MaxHeight = 480;
            settings.WindowStyle = System.Windows.WindowStyle.None;
            settings.ShowInTaskbar = false;
            settings.SizeToContent = System.Windows.SizeToContent.Height;
            settings.Background = System.Windows.Media.Brushes.Transparent;
            var bs = SCaddinsApp.Bootstrapper;
            var windowManager = SCaddinsApp.WindowManager;
            var vm = new ViewModels.OpenSheetViewModel(doc);
            windowManager.ShowDialog(vm, null, settings);
            return Autodesk.Revit.UI.Result.Succeeded;

        }

        public static void OpenViews(System.Collections.IList views)
        {
            foreach (var item in views) {
                var sheet = item as ExportSheet;
                if (sheet.Sheet != null) {
                    UIApplication uiapp = new UIApplication(sheet.Sheet.Document.Application);
                    uiapp.ActiveUIDocument.ActiveView = sheet.Sheet;
                }
            }
        }

        public static List<OpenableView> ViewsInModel(Document doc)
        {
            var result = new List<OpenableView>();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Sheets);
            foreach (ViewSheet view in collector) {
                result.Add(new OpenableView(view.ViewName, view.SheetNumber, view));
            }
            FilteredElementCollector collector2 = new FilteredElementCollector(doc);
            collector2.OfCategory(BuiltInCategory.OST_Views);
            foreach (View view in collector2) {
                result.Add(new OpenableView(view.Name, string.Empty, view));
            }
            return result;
        }
    }
}

/* vim: set ts=4 sw=4 nu expandtab: */
