using System.Net;
using BusinessLayer.Helpers;
using BusinessLayer.Service.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace BusinessLayer.Tests.AI;

public class GeminiClientTests
{
    [Fact]
    public async Task GenerateAsync_WhenApiKeyMissing_ReturnsMockWithoutNetworkCall()
    {
        var handler = new CountingHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new GeminiClient(
            new HttpClient(handler),
            Options.Create(new GeminiSettings { ApiKey = "", UseMockFallback = false }),
            NullLogger<GeminiClient>.Instance);

        var result = await client.GenerateAsync("system", [("user", "hello")]);

        Assert.Equal("", result.Text);
        Assert.Equal("mock", result.Source);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GenerateAsync_WhenRequestTimesOut_ReturnsFallback()
    {
        var client = new GeminiClient(
            new HttpClient(new TimeoutHandler()),
            Options.Create(new GeminiSettings { ApiKey = "test-key", UseMockFallback = false, TimeoutSeconds = 1 }),
            NullLogger<GeminiClient>.Instance);

        var result = await client.GenerateAsync("system", [("user", "hello")]);

        Assert.Equal("", result.Text);
        Assert.Equal("fallback", result.Source);
    }

    private sealed class CountingHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public CountingHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(_response);
        }
    }

    private sealed class TimeoutHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new TaskCanceledException("simulated timeout");
        }
    }
}
