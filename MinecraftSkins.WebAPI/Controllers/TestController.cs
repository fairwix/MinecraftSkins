using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace MinecraftSkins.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Проверка работы эндпоинта
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new 
        { 
            message = "Test controller works!", 
            timestamp = DateTime.UtcNow,
            endpoints = new[]
            {
                "GET /api/test - информация",
                "GET /api/test/error/{type} - вызвать ошибку",
                "GET /api/test/health/db-failure - симуляция проблемы с БД"
            }
        });
    }

    /// <summary>
    /// Тестовый эндпоинт для проверки обработки ошибок
    /// </summary>
    /// <param name="type">Тип ошибки: badrequest, notfound, conflict, external, unavailable</param>
    [HttpGet("error/{type}")]
    public IActionResult ThrowError(string type)
    {
        _logger.LogInformation("Test endpoint called with error type: {ErrorType}", type);

        switch (type.ToLower())
        {
            case "badrequest":
                throw new ArgumentException("Это тестовая ошибка валидации (400 Bad Request)");
                
            case "notfound":
                throw new KeyNotFoundException("Ресурс не найден (404 Not Found)");
                
            case "conflict":
                throw new InvalidOperationException("Конфликт данных (409 Conflict)");
                
            case "external":
                // Имитация ошибки внешнего сервиса
                throw new HttpRequestException("Ошибка подключения к внешнему API (CoinGecko)");
                
            case "unavailable":
                // Имитация недоступности сервиса
                throw new Exception("Сервис временно недоступен (503 Service Unavailable)");
                
            default:
                return BadRequest(new 
                { 
                    error = "Unknown error type",
                    availableTypes = new[] { "badrequest", "notfound", "conflict", "external", "unavailable" }
                });
        }
    }
    /// <summary>
    /// Тестовый эндпоинт, который вызывает падение контейнера (без middleware)
    /// </summary>
    [HttpGet("fatal")]
    public IActionResult FatalError()
    {
        _logger.LogCritical("🔥 FATAL ERROR! Process will crash now!");
    
        // Завершаем процесс с кодом ошибки
        // Middleware не успевает сработать, потому что процесс умирает
        Environment.FailFast("Принудительное падение контейнера для теста");
    
        return Ok(); // Сюда не дойдем
    }
    
    [HttpGet("die")]
    public IActionResult Die()
    {
        _logger.LogCritical("💀 Killing process...");
    
        // Принудительное завершение
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    
        return Ok();
    }

    [HttpGet("test")]
    public string Test()
    {
        _logger.LogInformation("run test");
        return "fucking test";
    }
    
    
    [HttpGet("exit")]
    public string Exit()
    {
        _logger.LogInformation("run exit");
        
        Environment.Exit(0);
        return "fucking test";
    }


    [HttpGet("rec")]
    public string Rec()
    {
        _logger.LogInformation("rec test");
        DoSmth();
        return "All is Ok";
    }

    private void DoSmth()
    {
        DoSmth();
    }
    
    /// <summary>
    /// Возвращает идентификатор текущего экземпляра (для проверки балансировки)
    /// </summary>
    [HttpGet("instance")]
    public IActionResult GetInstance()
    {
        // Получаем значение переменной окружения INSTANCE_ID, если её нет — "unknown"
        var instanceId = Environment.GetEnvironmentVariable("INSTANCE_ID") ?? "unknown";
        // HOSTNAME — это имя контейнера в Docker, тоже полезно
        var hostname = Environment.GetEnvironmentVariable("HOSTNAME") ?? "unknown";
    
        return Ok(new 
        { 
            instance = instanceId,
            hostname = hostname,
            timestamp = DateTime.UtcNow,
            message = $"Request handled by {instanceId}"
        });
    }
}// Test commit for CI/CD
