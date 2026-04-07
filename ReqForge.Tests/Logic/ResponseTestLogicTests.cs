using System.Net;
using ReqForge.Models;

namespace ReqForge.Tests.Logic;

public class ResponseTestLogicTests
{
    /// <summary>
    /// Mirrors the RunTests logic from MainViewModel.Tests.cs so we can unit-test it
    /// without instantiating the full ViewModel.
    /// </summary>
    private static List<TestResult> RunTests(IEnumerable<ResponseTest> tests, HttpResponseResult result)
    {
        var results = new List<TestResult>();
        foreach (var test in tests)
        {
            bool passed = test.TestType switch
            {
                "StatusEquals" => ((int)result.StatusCode).ToString() == test.ExpectedValue,
                "BodyContains" => result.Content.Contains(test.ExpectedValue, StringComparison.OrdinalIgnoreCase),
                "TimeLessThanMs" => double.TryParse(test.ExpectedValue, out var ms) &&
                                    result.ElapsedTime.TotalMilliseconds < ms,
                _ => false
            };
            results.Add(new TestResult
            {
                Description = $"{test.TestType}: {test.ExpectedValue}",
                Passed = passed
            });
        }
        return results;
    }

    private static HttpResponseResult MakeResponse(
        HttpStatusCode code = HttpStatusCode.OK,
        string content = "",
        double elapsedMs = 100)
    {
        return new HttpResponseResult
        {
            StatusCode = code,
            Content = content,
            ElapsedTime = TimeSpan.FromMilliseconds(elapsedMs)
        };
    }

    // === StatusEquals ===

    [Fact]
    public void StatusEquals_200_Passes()
    {
        var tests = new[] { new ResponseTest { TestType = "StatusEquals", ExpectedValue = "200" } };
        var response = MakeResponse(HttpStatusCode.OK);

        var results = RunTests(tests, response);

        Assert.Single(results);
        Assert.True(results[0].Passed);
    }

    [Fact]
    public void StatusEquals_404_Passes()
    {
        var tests = new[] { new ResponseTest { TestType = "StatusEquals", ExpectedValue = "404" } };
        var response = MakeResponse(HttpStatusCode.NotFound);

        var results = RunTests(tests, response);

        Assert.True(results[0].Passed);
    }

    [Fact]
    public void StatusEquals_WrongCode_Fails()
    {
        var tests = new[] { new ResponseTest { TestType = "StatusEquals", ExpectedValue = "200" } };
        var response = MakeResponse(HttpStatusCode.InternalServerError);

        var results = RunTests(tests, response);

        Assert.False(results[0].Passed);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, "200", true)]
    [InlineData(HttpStatusCode.Created, "201", true)]
    [InlineData(HttpStatusCode.BadRequest, "400", true)]
    [InlineData(HttpStatusCode.OK, "201", false)]
    [InlineData(HttpStatusCode.NotFound, "200", false)]
    public void StatusEquals_VariousCodes(HttpStatusCode actual, string expected, bool shouldPass)
    {
        var tests = new[] { new ResponseTest { TestType = "StatusEquals", ExpectedValue = expected } };
        var response = MakeResponse(actual);

        var results = RunTests(tests, response);

        Assert.Equal(shouldPass, results[0].Passed);
    }

    // === BodyContains ===

    [Fact]
    public void BodyContains_ExactMatch_Passes()
    {
        var tests = new[] { new ResponseTest { TestType = "BodyContains", ExpectedValue = "success" } };
        var response = MakeResponse(content: "{\"status\":\"success\"}");

        var results = RunTests(tests, response);

        Assert.True(results[0].Passed);
    }

    [Fact]
    public void BodyContains_CaseInsensitive_Passes()
    {
        var tests = new[] { new ResponseTest { TestType = "BodyContains", ExpectedValue = "SUCCESS" } };
        var response = MakeResponse(content: "{\"status\":\"success\"}");

        var results = RunTests(tests, response);

        Assert.True(results[0].Passed);
    }

    [Fact]
    public void BodyContains_NotFound_Fails()
    {
        var tests = new[] { new ResponseTest { TestType = "BodyContains", ExpectedValue = "error" } };
        var response = MakeResponse(content: "{\"status\":\"success\"}");

        var results = RunTests(tests, response);

        Assert.False(results[0].Passed);
    }

    [Fact]
    public void BodyContains_EmptyExpected_AlwaysPasses()
    {
        var tests = new[] { new ResponseTest { TestType = "BodyContains", ExpectedValue = "" } };
        var response = MakeResponse(content: "anything");

        var results = RunTests(tests, response);

        Assert.True(results[0].Passed);
    }

    // === TimeLessThanMs ===

    [Fact]
    public void TimeLessThanMs_FastResponse_Passes()
    {
        var tests = new[] { new ResponseTest { TestType = "TimeLessThanMs", ExpectedValue = "500" } };
        var response = MakeResponse(elapsedMs: 200);

        var results = RunTests(tests, response);

        Assert.True(results[0].Passed);
    }

    [Fact]
    public void TimeLessThanMs_SlowResponse_Fails()
    {
        var tests = new[] { new ResponseTest { TestType = "TimeLessThanMs", ExpectedValue = "500" } };
        var response = MakeResponse(elapsedMs: 800);

        var results = RunTests(tests, response);

        Assert.False(results[0].Passed);
    }

    [Fact]
    public void TimeLessThanMs_ExactBoundary_Fails()
    {
        var tests = new[] { new ResponseTest { TestType = "TimeLessThanMs", ExpectedValue = "500" } };
        var response = MakeResponse(elapsedMs: 500);

        var results = RunTests(tests, response);

        Assert.False(results[0].Passed);
    }

    [Fact]
    public void TimeLessThanMs_InvalidNumber_Fails()
    {
        var tests = new[] { new ResponseTest { TestType = "TimeLessThanMs", ExpectedValue = "not_a_number" } };
        var response = MakeResponse(elapsedMs: 100);

        var results = RunTests(tests, response);

        Assert.False(results[0].Passed);
    }

    // === Unknown test type ===

    [Fact]
    public void UnknownTestType_AlwaysFails()
    {
        var tests = new[] { new ResponseTest { TestType = "SomethingElse", ExpectedValue = "200" } };
        var response = MakeResponse();

        var results = RunTests(tests, response);

        Assert.False(results[0].Passed);
    }

    // === Multiple tests ===

    [Fact]
    public void MultipleTests_AllEvaluated()
    {
        var tests = new[]
        {
            new ResponseTest { TestType = "StatusEquals", ExpectedValue = "200" },
            new ResponseTest { TestType = "BodyContains", ExpectedValue = "hello" },
            new ResponseTest { TestType = "TimeLessThanMs", ExpectedValue = "1000" }
        };
        var response = MakeResponse(HttpStatusCode.OK, "hello world", 300);

        var results = RunTests(tests, response);

        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.True(r.Passed));
    }

    [Fact]
    public void MultipleTests_MixedResults()
    {
        var tests = new[]
        {
            new ResponseTest { TestType = "StatusEquals", ExpectedValue = "200" },
            new ResponseTest { TestType = "BodyContains", ExpectedValue = "missing_text" },
        };
        var response = MakeResponse(HttpStatusCode.OK, "actual content");

        var results = RunTests(tests, response);

        Assert.True(results[0].Passed);
        Assert.False(results[1].Passed);
    }

    [Fact]
    public void EmptyTests_ReturnsEmptyResults()
    {
        var results = RunTests(Array.Empty<ResponseTest>(), MakeResponse());

        Assert.Empty(results);
    }

    // === Description format ===

    [Fact]
    public void TestResult_DescriptionFormat_Correct()
    {
        var tests = new[] { new ResponseTest { TestType = "StatusEquals", ExpectedValue = "200" } };
        var response = MakeResponse();

        var results = RunTests(tests, response);

        Assert.Equal("StatusEquals: 200", results[0].Description);
    }
}
