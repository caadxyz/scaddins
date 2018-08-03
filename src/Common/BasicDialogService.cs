﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCaddins.Common
{
    class BasicDialogService : IDialogService
    {
        public BasicDialogService()
        {

        }

        public bool? ShowMessageBox(string message)
        {
            System.Windows.MessageBox.Show(message);
            return true;
        }

        public bool? ShowConfirmationDialog(string message, bool? defaultCheckboxValue, out bool checkboxResult)
        {
            var confirmOverwriteDialog = new ExportManager.ViewModels.ConfirmationDialogViewModel();
            confirmOverwriteDialog.Message = message;
            confirmOverwriteDialog.Value = defaultCheckboxValue;
            bool? result = SCaddinsApp.WindowManager.ShowDialog(confirmOverwriteDialog, null, ExportManager.ViewModels.ConfirmationDialogViewModel.DefaultWindowSettings);
            bool newBool = result.HasValue ? result.Value : false;
            if (newBool) {
                checkboxResult = confirmOverwriteDialog.ValueAsBool;
                return confirmOverwriteDialog.ValueAsBool;
            }
            checkboxResult = confirmOverwriteDialog.ValueAsBool;
            return false;
        }

        public bool? ShowSaveAsDialog(string defaultFileName, string defaultExtension, string filter, out string savePath)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = defaultFileName; // Default file name
            dlg.DefaultExt = defaultExtension; // Default file extension
            dlg.Filter = filter; // Filter files by extension
            bool? result = dlg.ShowDialog();
            savePath = dlg.FileName;
            return result;
        }

        public bool? ShowDirectorySelectionDialog(string defaultDir, out string dirPath)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK) {
                    dirPath = dialog.SelectedPath;
                    return true;
                } else
                {
                    dirPath = defaultDir;
                    return false;
                }
            } 
        }
    }
}