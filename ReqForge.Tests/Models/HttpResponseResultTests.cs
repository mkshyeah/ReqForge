using System.Net;
using ReqForge.Models;

namespace ReqForge.Tests.Models;

public class HttpResponseResultTests
{
    [Theory]
    [InlineData(HttpStatusCode.OK, true)]
    [InlineData(HttpStatusCode.Created, true)]
    [InlineData(HttpStatusCode.NoContent, true)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.Unauthorized, false)]
    [InlineData(HttpStatusCode.NotFound, false)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    [InlineData(HttpStatusCode.Redirect, false)]
    public void IsSuccess_ReturnsCorrectValue(HttpStatusCode code, bool expected)
    {
        var result = new HttpResponseResult { StatusCode = code };

        Assert.Equal(expected, result.IsSuccess);
    }

    [Fact]
    public void FullInfo_ContainsStatusCode()
    {
        var result = new HttpResponseResult
        {
            StatusCode = HttpStatusCode.OK,
            ElapsedTime = TimeSpan.FromMilliseconds(150),
            ContentLength = 1024
        };

        Assert.Contains("200", result.FullInfo);
        Assert.Contains("OK", result.FullInfo);
    }

    [Fact]
    public void FullInfo_ContainsElapsedTime()
    {
        var result = new HttpResponseResult
        {
            StatusCode = HttpStatusCode.OK,
            ElapsedTime = TimeSpan.FromMilliseconds(250),
            ContentLength = 0
        };

        Assert.Contains("250ms", result.FullInfo);
    }

    [Fact]
    public void FullInfo_SmallSize_ShowsBytes()
    {
        var result = new HttpResponseResult
        {
            StatusCode = HttpStatusCode.OK,
            ElapsedTime = TimeSpan.Zero,
            ContentLength = 512
        };

        Assert.Contains("512 B", result.FullInfo);
    }

    [Fact]
    public void FullInfo_KilobyteSize_ShowsKB()
    {
        var result = new HttpResponseResult
        {
            StatusCode = HttpStatusCode.OK,
            ElapsedTime = TimeSpan.Zero,
            ContentLength = 2048
        };

        Assert.Contains("KB", result.FullInfo);
    }

    [Fact]
    public void FullInfo_MegabyteSize_ShowsMB()
    {
        var result = new HttpResponseResult
        {
            StatusCode = HttpStatusCode.OK,
            ElapsedTime = TimeSpan.Zero,
            ContentLength = 2 * 1024 * 1024
        };

        Assert.Contains("MB", result.FullInfo);
    }
}
