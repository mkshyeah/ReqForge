using System.IO;
using System.Net;
using System.Text.Json;
using CommunityToolkit.Mvvm.Input;
using ReqForge.Models;

namespace ReqForge.ViewModels;

public partial class MainViewModel
{
    [RelayCommand]
    private void ExportCollections()
    {
        if (Collections.Count == 0) return;

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            FileName = "reqforge-export.json"
        };

        if (dialog.ShowDialog() == true)
        {
            var json = JsonSerializer.Serialize(Collections.ToString(), _jsonOptions);
            File.WriteAllText(dialog.FileName, json);
        }
    }

    [RelayCommand]
    private void ImportCollections()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var json = File.ReadAllText(dialog.FileName);
                var imported = JsonSerializer.Deserialize<List<RequestCollection>>(json);
                if (imported != null)
                {
                    foreach (var coll in imported)
                    {
                        Collections.Add(coll);
                    }

                    _storage.SaveAll(Collections.ToList(), CurrentUsername);
                }
            }
            catch
            {
                StatusInfo = "Import failed: invalid JSON file";
            }
        }
    }
}