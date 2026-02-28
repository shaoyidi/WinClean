using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WinClean.Helpers;
using WinClean.Localization;
using WinClean.Models;
using WinClean.ViewModels;

namespace WinClean.Views;

public partial class LargeFileScannerView : UserControl
{
    public LargeFileScannerView()
    {
        InitializeComponent();
    }

    private LargeFileScannerViewModel? Vm => DataContext as LargeFileScannerViewModel;

    private void FilesGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (FilesGrid.SelectedItem is FileItem file)
            Vm?.OpenInExplorerCommand.Execute(file);
    }

    private void OpenInExplorer_Click(object sender, RoutedEventArgs e)
    {
        if (FilesGrid.SelectedItem is FileItem file)
            Vm?.OpenInExplorerCommand.Execute(file);
    }

    private void DeleteToRecycleBin_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null || FilesGrid.SelectedItem is not FileItem file) return;

        var result = Vm.DoDeleteToRecycleBin(file);
        if (!result.Success)
        {
            MessageBox.Show(
                result.ErrorMessage ?? LangProvider.S("LF_OperationFailed"),
                LangProvider.S("LF_RecycleBinFailed"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void PermanentDelete_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null || FilesGrid.SelectedItem is not FileItem file) return;

        var confirm = MessageBox.Show(
            LangProvider.F("LF_PermanentDeleteConfirm", file.FileName, FileSizeFormatter.Format(file.Size), file.Directory),
            LangProvider.S("LF_PermanentDeleteTitle"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        var result = Vm.DoPermanentDelete(file);
        if (!result.Success)
        {
            MessageBox.Show(
                result.ErrorMessage ?? LangProvider.S("LF_OperationFailed"),
                LangProvider.S("LF_PermanentDeleteFailed"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
