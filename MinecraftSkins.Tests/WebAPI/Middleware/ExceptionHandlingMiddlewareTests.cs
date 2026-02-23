using System.Text;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using MinecraftSkins.Application.Exceptions;
using MinecraftSkins.WebAPI.Middleware;

namespace MinecraftSkins.Tests.WebAPI.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;
    private readonly Mock<IHostEnvironment> _envMock;
    private readonly ExceptionHandlingMiddleware _middleware;

    public ExceptionHandlingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        _envMock = new Mock<IHostEnvironment>();
        _envMock.Setup(e => e.EnvironmentName).Returns("Production");
        _middleware = new ExceptionHandlingMiddleware(_ => throw new Exception("Test"), _loggerMock.Object, _envMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturn500_WhenUnhandledException()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await _middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(500);
        context.Response.ContentType.Should().Contain("application/problem+json");
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().Contain("Internal server error");
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturn400_WhenValidationException()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new ValidationException("error"), _loggerMock.Object, _envMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().Contain("Validation failed");
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturn409_WhenSkinUnavailableException()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new SkinUnavailableException(Guid.NewGuid()), _loggerMock.Object, _envMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturn503_WhenExternalServiceUnavailableException()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new ExternalServiceUnavailableException("Service", "msg"), _loggerMock.Object, _envMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(503);
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturn499_WhenOperationCanceledException()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new OperationCanceledException(), _loggerMock.Object, _envMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(499);
    }
}