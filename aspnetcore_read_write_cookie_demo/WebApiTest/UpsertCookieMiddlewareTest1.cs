using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using WebApi;
using Xunit;

namespace WebApiTest;

public class UpsertCookieMiddlewareTest1
{
    private readonly UpsertCookieMiddleware _upsertCookieMiddleware;
    private bool _isNextDelegateCalled;
    private readonly HttpContext _httpContextMock;
    
    public UpsertCookieMiddlewareTest1()
    {
        var responseMock = new Mock<IHttpResponseFeature>();
        _httpContextMock = new DefaultHttpContext();

        // Mock HttpResponse
        Func<object, Task> callbackMethod = null;
        responseMock.Setup(m => m.OnStarting(It.IsAny<Func<object, Task>>(), It.IsAny<object>()))
            .Callback<Func<object, Task>, object>((func, obj) => callbackMethod = func);
        responseMock.Setup(m => m.Headers).Returns(new HeaderDictionary());
        
        _httpContextMock.Features.Set(responseMock.Object);
        
        RequestDelegate next = async context =>
        {
            _isNextDelegateCalled = true;
            if (callbackMethod != null)
            {
                await callbackMethod.Invoke(_httpContextMock);
            }
            else
            {
                await Task.CompletedTask;
            }
        };

        _upsertCookieMiddleware = new UpsertCookieMiddleware(next);
    }

    [Fact]
    public async Task TestUpsertCookieMiddleware_WhenRequestContainsTargetCookie_ShouldResponseContainsUpdatedCookie()
    {
        // Given
        _httpContextMock.Request.Headers["Cookie"] = "cookie11=key1=val1";
        var expectResponseCookie = WebUtility.UrlEncode("key1=val1&newKey=newVal");
        
        // When
        await _upsertCookieMiddleware.Invoke(_httpContextMock);

        // Then
        _isNextDelegateCalled.Should().BeTrue();
        _httpContextMock.Response.GetTypedHeaders().SetCookie.First(c => c.Name.Value == "cookie11").Value.ToString()
            .Should().Contain(expectResponseCookie);

    }
}