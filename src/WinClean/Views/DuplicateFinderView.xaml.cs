using System.Windows;
using System.Windows.Controls;
using WinClean.Localization;
using WinClean.ViewModels;

namespace WinClean.Views;

public partial class DuplicateFinderView : UserControl
{
    public DuplicateFinderView()
    {
        InitializeComponent();
    }

    private void DeleteSelected_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DuplicateFinderViewModel vm) return;

        var selectedCount = vm.Groups.SelectMany(g => g.Files.Where(f => f.IsSelected)).Count();
        if (selectedCount == 0)
        {
            MessageBox.Show(LangProvider.S("DF_NoSelection"), LangProvider.S("DF_Hint"),
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirm = MessageBox.Show(
            LangProvider.F("DF_DeleteConfirm", selectedCount),
            LangProvider.S("DF_DeleteConfirmTitle"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        var (deleted, failed, freed, errors) = vm.DoDeleteSelected();

        if (errors.Count > 0)
        {
            var errorList = string.Join("\n", errors.Take(10));
            if (errors.Count > 10)
                errorList += $"\n... +{errors.Count - 10}";

            MessageBox.Show(
                LangProvider.F("DF_PartialFailedMsg", deleted, failed, errorList),
                LangProvider.S("DF_PartialFailedTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}
