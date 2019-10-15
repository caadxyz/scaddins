﻿namespace SCaddins.Common
{
    internal class BasicDialogService : IDialogService
    {
        public bool? ShowColourChooser()
        {
            var colourChooser = new System.Windows.Forms.ColorDialog();
            colourChooser.AnyColor = true;
            var dialogResult = colourChooser.ShowDialog();
            if (dialogResult == System.Windows.Forms.DialogResult.OK) {
                //// var colour = colourChooser.Color;
                return true;
            }
            return false;
        }

        public bool? ShowConfirmationDialog(string message, bool? defaultCheckboxValue, out bool checkboxResult)
        {
            var confirmOverwriteDialog = new ExportManager.ViewModels.ConfirmationDialogViewModel();
            confirmOverwriteDialog.Message = message;
            confirmOverwriteDialog.Value = defaultCheckboxValue;
            bool? result = SCaddinsApp.WindowManager.ShowDialog(confirmOverwriteDialog, null, ExportManager.ViewModels.ConfirmationDialogViewModel.DefaultWindowSettings);
            checkboxResult = confirmOverwriteDialog.ValueAsBool;
            return result.HasValue ? result.Value : false;
        }

        public bool? ShowDirectorySelectionDialog(string defaultDir, out string dirPath)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK) {
                    dirPath = dialog.SelectedPath;
                    return true;
                } else {
                    dirPath = defaultDir;
                    return false;
                }
            }
        }

        public bool? ShowFileSelectionDialog(string defaultFile, out string filePath)
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog()) {
                dialog.Multiselect = false;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK) {
                    filePath = dialog.FileName;
                    return true;
                } else {
                    filePath = defaultFile;
                    return false;
                }
            }
        }

        public void ShowMessageBox(string message)
        {
            System.Windows.MessageBox.Show(message);
        }

        public void ShowMessageBox(string title, string message)
        {
            System.Windows.MessageBox.Show(message, title);
        }

        public void ShowWarningMessageBox(string title, string message)
        {
            System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }

        public void ShowErrorMessageBox(string title, string message)
        {
            System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }

        public bool ShowYesNoDialog(string title, string message, bool defaultValue)
        {
            var dialogResult = System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            return dialogResult == System.Windows.MessageBoxResult.Yes;
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

        public bool? ShowOpenFileDialog(string defaultFileName, out string fileName)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = false;
            bool? result = dlg.ShowDialog();
            fileName = dlg.FileName;
            return result;
        }
    }
}