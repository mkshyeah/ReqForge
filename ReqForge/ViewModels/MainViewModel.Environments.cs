using CommunityToolkit.Mvvm.Input;
using ReqForge.Models;

namespace ReqForge.ViewModels;

public partial class MainViewModel
{
    [RelayCommand]
    private void CreateEnvironment()
    {
        var newEnv = new RequestEnvironment { Name = $"Env {Environments.Count+1}"};
        newEnv.Variables.Add(new EnvironmentVariable("base_url","https://api.example.com"));
        Environments.Add(newEnv);
        SelectedEnvironment = newEnv;
        SaveEnvironments();
    }

    [RelayCommand]
    private void AddVariable()
    {
        SelectedEnvironment?.Variables.Add(new EnvironmentVariable("", ""));
        SaveEnvironments();
    }

    [RelayCommand]
    private void RemoveVariable(EnvironmentVariable variable)
    {
        if (variable != null)
        {
            SelectedEnvironment?.Variables.Remove(variable);
            SaveEnvironments();
        }
    }

    [RelayCommand]
    private void SaveEnvironments()
    {
        var dtos = Environments.Select(env => new RequestEnvironmentDto
        {
            Name = env.Name,
            Variables = env.Variables.Select(v => new EnvironmentVariableDto
                    { Key = v.Key, Value = v.Value })
                .ToList()
        }).ToList();
        
        _envStorage.SaveAll(dtos);
    }
    
    private string ResolveVariables(string input)
    {
        // Если строка пустая или окружение не выбрано — возвращаем как есть
        if (string.IsNullOrEmpty(input) || SelectedEnvironment == null)
            return input;

        var result = input;

        foreach (var variable in SelectedEnvironment.Variables)
        {
            // Проверяем, что ключ переменной не пустой
            if (!string.IsNullOrWhiteSpace(variable.Key))
            {
                // Ищем {{key}} и заменяем на value
                // Используем конструкция $$$$"{{{{{variable.Key}}}}}" для экранирования фигурных скобок в строке
                result = result.Replace($"{{{{{variable.Key}}}}}", variable.Value ?? string.Empty);
            }
        }

        return result;
    }
}