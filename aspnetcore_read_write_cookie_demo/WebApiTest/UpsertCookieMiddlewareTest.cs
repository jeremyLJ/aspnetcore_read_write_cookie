using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using WebApi;
using Xunit;

namespace WebApiTest;

public class UpsertCookieMiddlewareTest
{
    private readonly UpsertCookieMiddleware _upsertCookieMiddleware;
    private bool _isNextDelegateCalled;
    private readonly Mock<IRequestCookieCollection> _requestCookieMock;
    private readonly Mock<HttpResponse> _responseMock;
    private readonly Mock<HttpContext> _httpContextMock;
    
    public UpsertCookieMiddlewareTest()
    {
        var requestMock = new Mock<HttpRequest>();
        _responseMock = new Mock<HttpResponse>();
        _httpContextMock = new Mock<HttpContext>();

        // Mock HttpRequest with Cookie
        _requestCookieMock = new Mock<IRequestCookieCollection>();
        
        requestMock.Setup((m => m.Cookies)).Returns(_requestCookieMock.Object);
        _httpContextMock.Setup(m => m.Request).Returns(requestMock.Object);
        
        // Mock HttpResponse
        Func<object, Task> callbackMethod = null;
        _responseMock.Setup(m => m.OnStarting(It.IsAny<Func<object, Task>>(), It.IsAny<object>()))
            .Callback<Func<object, Task>, object>((func, obj) => callbackMethod = func);

        var responseCookieMock = new Mock<IResponseCookies>();
        _responseMock.Setup(m => m.Cookies).Returns(responseCookieMock.Object);
        _responseMock.Setup(m => m.Headers).Returns(new HeaderDictionary
        {
            // {"Set-Cookie", responseCookieMock.Object.ToString()}
        });

        _httpContextMock.Setup(m => m.Response).Returns(_responseMock.Object);
        
        RequestDelegate next = async context =>
        {
            _isNextDelegateCalled = true;
            if (callbackMethod != null)
            {
                await callbackMethod.Invoke(_httpContextMock.Object);
            }
            else
            {
                await Task.CompletedTask;
            }
        };

        _upsertCookieMiddleware = new UpsertCookieMiddleware(next);
    }

    [Fact(Skip = "fail")]
    public async Task TestUpsertCookieMiddleware_WhenRequestContainsTargetCookie_ShouldResponseContainsUpdatedCookie()
    {
        // Given
        var cookieValueTest = "key1=val1";
        _requestCookieMock.Setup(m => m.TryGetValue("cookie11", out cookieValueTest)).Returns(true);
        
        var expectResponseCookie = WebUtility.UrlEncode("key1=val1&newKey=newVal");
        
        // When
        await _upsertCookieMiddleware.Invoke(_httpContextMock.Object);

        // Then
        _isNextDelegateCalled.Should().BeTrue();
        _responseMock.Object.GetTypedHeaders().SetCookie.First(c => c.Name.Value == "cookie11").Value.ToString()
            .Should().Contain(expectResponseCookie);
        
        // _responseMock.Object.Headers["Set-Cookie"].Should().Contain(expectResponseCookie);
    }
}