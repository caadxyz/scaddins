// (C) Copyright 2012-2013 by Andrew Nicholas (andrewnicholas@iinet.net.au)
//
// This file is part of SCexport.
//
// SCexport is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SCexport is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SCexport.  If not, see <http://www.gnu.org/licenses/>.

namespace SCaddins.SCexport
{
    using System;
    using System.Windows.Forms;
    using Autodesk.Revit.DB;

    /// <summary>
    /// Class to hold view sheet information.
    /// </summary>
    public class SCexportSheet
    {
        #region Variables
        private DateTime sheetRevisionDateTime;
        private Document doc;
        private ElementId id;
        private PrintSetting printSetting;
        private SheetName segmentedFileName;
        private ViewSheet sheet;
        private bool forceDate;
        private bool verified;
        private double height;
        private double width;
        private string displineCode;
        private string fullExportName;
        private string pageSize;
        private string projectName;
        private string projectNumber;
        private string scale;
        private string scaleBarScale;
        private string sheetDescription;
        private string sheetNumber;
        private string sheetRevision;
        private string sheetRevisionDate;
        private string sheetRevisionDescription;

        /// <summary>
        /// Initializes a new instance of the SCexportSheet class.
        /// </summary>
        /// <param name="sheet">The Revit ViewSheet.</param>
        /// <param name="doc">The Active Revit Document.</param>
        /// <param name="filenameTemplate">Naming template.</param>
        /// <param name="scx">SCexport instance.</param>
        public SCexportSheet(
                ViewSheet sheet,
                Document doc,
                SheetName filenameTemplate,
                SCexport scx)
        {
            this.Init(sheet, doc, filenameTemplate, scx);
        }

        /// <summary>
        /// Gets the size of the page.
        /// </summary>
        /// <value>The size of the page.</value>
        public string PageSize
        {
            get { return this.pageSize; }
        }

        /// <summary>
        /// Gets the name of the print setting.
        /// </summary>
        /// <value>The name of the print setting.</value>
        public string PrintSettingName
        {
            get
            {
                return this.printSetting != null ? this.printSetting.Name : string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the name of the segmented file.
        /// </summary>
        /// <value>The name of the segmented file.</value>
        public SheetName SegmentedFileName {
            get {
                return this.segmentedFileName;
            }
            
            set {
                this.segmentedFileName = value;
                this.SetExportName();
            }
        }

        /// <summary>
        /// Gets the sheet description.
        /// </summary>
        /// <value>The sheet description.</value>
        public string SheetDescription
        {
            get { return this.sheetDescription; }
        }

        /// <summary>
        /// Gets the sheet number.
        /// </summary>
        /// <value>The sheet number.</value>
        public string SheetNumber
        {
            get { return this.sheetNumber; }
        }

        /// <summary>
        /// Gets the sheet revision.
        /// </summary>
        /// <value>The sheet revision.</value>
        public string SheetRevision
        {
            get {
                return this.sheetRevision ?? "-";
            }
        }

        /// <summary>
        /// Gets the sheet revision description.
        /// </summary>
        /// <value>The sheet revision description.</value>
        public string SheetRevisionDescription
        {
            get {
                return this.sheetRevisionDescription ?? "-";
            }
        }

        /// <summary>
        /// Gets the sheet revision date.
        /// </summary>
        /// <value>The sheet revision date.</value>
        public string SheetRevisionDate
        {
            get {
                return this.sheetRevisionDate ?? "-";
            }
        }

        /// <summary>
        /// Gets the sheet revision date time.
        /// </summary>
        /// <value>The sheet revision date time.</value>
        public DateTime SheetRevisionDateTime
        {
            get { return this.sheetRevisionDateTime; }
        }

        /// <summary>
        /// Gets the scale.
        /// </summary>
        /// <value>The scale.</value>
        public string Scale
        {
            get {
                string result = this.scale.Trim();
                int i = 0;
                if (result.Contains(":")) {
                    i = result.IndexOf(':');
                }
                bool flag = false;
                if (result.Trim() == string.Empty) {
                    result = "0";
                }
                if (this.scaleBarScale != string.Empty) {
                    flag |= i > 0 && !result.Substring(i + 2).Equals(this.scaleBarScale.Trim());
                }
                if (this.scaleBarScale.Trim() != string.Empty && flag) {
                    result += " [**" + this.scaleBarScale + "]";
                }
                return result;
            }
        }

        /// <summary>
        /// Gets or sets the export dir.
        /// </summary>
        /// <value>The export dir.</value>
        public string ExportDir
        {
            get; set;
        }

        /// <summary>
        /// Gets the sheet.
        /// </summary>
        /// <value>The sheet.</value>
        public ViewSheet Sheet
        {
            get { return this.sheet; }
        }

        /// <summary>
        /// Gets the full name of the export.
        /// </summary>
        /// <value>The full name of the export.</value>
        public string FullExportName
        {
            get { return this.fullExportName; }
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public ElementId Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>The width.</value>
        public double Width
        {
            get { return this.width * 304.8; }
        }

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>The height.</value>
        public double Height
        {
            get { return this.height * 304.8; }
        }

        /// <summary>
        /// Gets the SC print setting.
        /// </summary>
        /// <value>The SC print setting.</value>
        public PrintSetting SCPrintSetting
        {
            get { return this.printSetting; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="SCexportSheet"/> is verified.
        /// </summary>
        /// <value><c>true</c> if verified; otherwise, <c>false</c>.</value>
        public bool Verified
        {
            get { return this.verified; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SCexportSheet"/> force date.
        /// </summary>
        /// <value><c>true</c> if force date; otherwise, <c>false</c>.</value>
        public bool ForceDate
        {
            get {
                return this.forceDate;
            }
            
            set {
                this.forceDate = value;
                this.SetExportName();
            }
        }
        #endregion

        /// <summary>
        /// Fulls the export path.
        /// </summary>
        /// <returns>The export path.</returns>
        /// <param name="extension">The file extension.</param>
        public string FullExportPath(string extension)
        {
            return this.ExportDir + "\\" + this.fullExportName + extension;
        }

        /// <summary>
        /// Updates some of the sheet info(scale, pagesize).
        /// This could be done at startup, but in some cases
        /// it can take a while.
        /// </summary>
        public void UpdateSheetInfo()
        {
            var titleBlock = SCexport.GetTitleBlockFamily(
                this.sheetNumber, this.doc);
            if (titleBlock != null) {
                this.scale = titleBlock.get_Parameter(
                    BuiltInParameter.SHEET_SCALE).AsString();
                try {
                    #if REVIT2015
                    var p = titleBlock.GetParameters(Constants.TitleScale);
                    var s = p[0].AsValueString();
                    #else
                    var s = titleBlock.get_Parameter(Constants.TitleScale).AsValueString();
                    #endif
                    var d = Convert.ToDouble(s);
                    var i = Convert.ToInt32(d);
                    this.scaleBarScale = d.ToString();
                } catch {
                    this.scaleBarScale = string.Empty;
                }
                this.width = titleBlock.get_Parameter(
                        BuiltInParameter.SHEET_WIDTH).AsDouble();
                this.height = titleBlock.get_Parameter(
                        BuiltInParameter.SHEET_HEIGHT).AsDouble();
            }
            this.pageSize = PrintSettings.GetSheetSizeAsString(this);
            this.printSetting = PrintSettings.AssignPrintSetting(
                    ref this.doc, this.pageSize);
            this.verified = true;
        }

        public void UpdateNumber()
        {
            this.sheetNumber = this.sheet.get_Parameter(
                    BuiltInParameter.SHEET_NUMBER).AsString();   
        }

        public void UpdateRevision()
        {
            this.sheetRevision = this.sheet.get_Parameter(
                    BuiltInParameter.SHEET_CURRENT_REVISION).AsString();
            this.sheetRevisionDescription = this.sheet.get_Parameter(
                    BuiltInParameter.SHEET_CURRENT_REVISION_DESCRIPTION).AsString();
            this.sheetRevisionDate = this.sheet.get_Parameter(
                    BuiltInParameter.SHEET_CURRENT_REVISION_DATE).AsString();
            this.sheetRevisionDateTime = SCexport.ToDateTime(this.sheetRevisionDate);
        }

        /// <summary>
        /// Initializes initial values.
        /// </summary>
        /// <param name="viewSheet">The Revit ViewSheet.</param>
        /// <param name="document">The Active Revit Document.</param>
        /// <param name="sheetName">Naming template.</param>
        /// <param name="scx">SCexport instance.</param>
        private void Init(
                ViewSheet viewSheet,
                Document document,
                SheetName sheetName,
                SCexport scx)
        {
            this.doc = document;
            this.sheet = viewSheet;
            this.segmentedFileName = sheetName;
            this.verified = false;
            this.displineCode = "AD";
            this.ExportDir = scx.ExportDir;
            this.sheetNumber = viewSheet.get_Parameter(
                    BuiltInParameter.SHEET_NUMBER).AsString();
            this.UpdateRevision();
            this.sheetDescription = viewSheet.get_Parameter(
                    BuiltInParameter.SHEET_NAME).AsString();
            this.projectNumber = document.ProjectInformation.Number;
            this.projectName = document.ProjectInformation.Name;
            this.width = 841;
            this.height = 594;
            this.scale = string.Empty;
            this.scaleBarScale = string.Empty;
            this.pageSize = string.Empty;
            this.id = viewSheet.Id;
            this.SetExportName();
        }

        private void PopulateSegmentedFileName()
        {
            for (int i = 0; i < this.segmentedFileName.Count; i++) {
                switch (this.segmentedFileName[i].Type) {
                case SheetNameSegment.SegmentType.SheetNumber:
                    this.segmentedFileName[i].Text = this.sheetNumber;
                    break;
                case SheetNameSegment.SegmentType.SheetName:
                    this.segmentedFileName[i].Text = this.sheetDescription;
                    break;
                case SheetNameSegment.SegmentType.ProjectNumber:
                    this.segmentedFileName[i].Text = this.projectNumber;
                    break;
                case SheetNameSegment.SegmentType.Discipline:
                    if (this.segmentedFileName[i].Text.Trim() == string.Empty) {
                        this.segmentedFileName[i].Text = this.displineCode;
                    }
                    break;
                case SheetNameSegment.SegmentType.Revision:
                    this.segmentedFileName[i].Text = this.sheetRevision;
                    break;
                case SheetNameSegment.SegmentType.RevisionDescription:
                    this.segmentedFileName[i].Text =
                        this.sheetRevisionDescription;
                    break;
                case SheetNameSegment.SegmentType.Hyphen:
                    this.segmentedFileName[i].Text = "-";
                    break;
                case SheetNameSegment.SegmentType.Underscore:
                    this.segmentedFileName[i].Text = "_";
                    break;
                }
            }
        }

        /// <summary>
        /// Set the export name for the sheet.
        /// </summary>
        private void SetExportName()
        {
            if (this.forceDate) {
                this.sheetRevision = SCexport.GetDateString();
            } else {
                this.sheetRevision = this.sheet.get_Parameter(
                        BuiltInParameter.SHEET_CURRENT_REVISION).AsString();
            }

            if (this.sheetRevision.Length < 1) {
                this.sheetRevision = SCexport.GetDateString();
            }

            this.PopulateSegmentedFileName();
            this.fullExportName = null;
            foreach (SheetNameSegment s in this.segmentedFileName) {
                this.fullExportName += s.Text;
            }
        }
    }
}

/* vim: set ts=4 sw=4 nu expandtab: */
