using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqForge.Models;

namespace ReqForge.ViewModels;

public partial class MainViewModel
{
    public ObservableCollection<ResponseTest> Tests { get; } = new();
    [ObservableProperty]
    private ObservableCollection<TestResult> _testResults = new();
    public List<string> TestTypes { get; } = new() { "StatusEquals", "BodyContains", "TimeLessThanMs" };
    [RelayCommand]
    private void AddTest()
    {
        Tests.Add(new ResponseTest());
    }

    [RelayCommand]
    private void RemoveTest(ResponseTest test)
    {
        if (test != null)
        {
            Tests.Remove(test);
        }
    }

    private void RunTests(HttpResponseResult result)
    {
        TestResults.Clear();
        foreach (var test in Tests)
        {
            bool passed = test.TestType switch
            {
                "StatusEquals" => ((int)result.StatusCode).ToString() == test.ExpectedValue,
                "BodyContains" => result.Content.Contains(test.ExpectedValue, StringComparison.OrdinalIgnoreCase),
                "TimeLessThanMs" => double.TryParse(test.ExpectedValue, out var ms) &&
                                    result.ElapsedTime.TotalMilliseconds < ms,
                _ => false
            };
            
            TestResults.Add(new TestResult
            {
                Description = $"{test.TestType}: {test.ExpectedValue}",
                Passed = passed,
            });
        }
        
    }
}