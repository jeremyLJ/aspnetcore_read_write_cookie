using System.Net;
using System.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using WebApi;
using Xunit;

namespace WebApiTest;

public class UpsertCookieMiddlewareTest2
{
    private readonly UpsertCookieMiddleware _upsertCookieMiddleware;
    private bool _isNextDelegateCalled;
    private readonly Mock<IRequestCookieCollection> _requestCookieCollectionMock;
    private readonly HttpContext _httpContextMock;

    public UpsertCookieMiddlewareTest2()
    {
        _httpContextMock = new DefaultHttpContext();
        var requestCookieFeatureMock = new Mock<IRequestCookiesFeature>();
        var responseMock = new Mock<IHttpResponseFeature>();

        _requestCookieCollectionMock = new Mock<IRequestCookieCollection>();
        requestCookieFeatureMock.Setup(m => m.Cookies).Returns(_requestCookieCollectionMock.Object);
        
        // Mock HttpResponse
        Func<object, Task> callbackMethod = null;
        responseMock.Setup(m => m.OnStarting(It.IsAny<Func<object, Task>>(), It.IsAny<object>()))
            .Callback<Func<object, Task>, object>((func, obj) => callbackMethod = func);
        responseMock.Setup(m => m.Headers).Returns(new HeaderDictionary());
        
        _httpContextMock.Features.Set(requestCookieFeatureMock.Object);
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
        var cookieKeyTest = "cookie11"; 
        var cookieValueTest = "key1=val1";
        
        _requestCookieCollectionMock.Setup(m => m.TryGetValue(cookieKeyTest, out cookieValueTest)).Returns(true);
        
        var expectResponseCookie = HttpUtility.UrlEncode("key1=val1&newKey=newVal");
        
        // When
        await _upsertCookieMiddleware.Invoke(_httpContextMock);

        // Then
        _isNextDelegateCalled.Should().BeTrue();
        _httpContextMock.Response.GetTypedHeaders().SetCookie.First(c => c.Name.Value == cookieKeyTest).Value.ToString()
            .Should().Contain(expectResponseCookie);
    }
}