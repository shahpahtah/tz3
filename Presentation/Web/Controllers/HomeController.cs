using System.Diagnostics;
using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using exchangesms.data;
using Web.Models;
using exchangesms;

namespace Web.Controllers;

public class HomeController : Controller
{
    private readonly IMessageRepository _messageRepository;
    private readonly ILogger<HomeController> _logger;
    private readonly IWebSocketManager _webSocketManager;


    public HomeController(ILogger<HomeController> logger, IMessageRepository messageRepository, IWebSocketManager webSocketManager)
    {
        _logger = logger;
        _messageRepository = messageRepository;
        _webSocketManager = webSocketManager;
    }

    // Путь для отображения страницы
    public IActionResult Client1()
    {
        return View();
    }

    // Action для отображения представления Client2 (Отображение сообщений в реальном времени)
    [HttpGet("/Client2")] // Путь для отображения страницы
    public IActionResult Client2()
    {
        return View();
    }

    // Action для отображения представления Client3 (История сообщений)
    [HttpGet("/Client3")] // Путь для отображения страницы
    public IActionResult Client3()
    {
        return View();
    }

    [HttpPost("api/send")]
    public IActionResult SendMessage([FromBody] MessageModel message)
    {
        try
        {
            if (string.IsNullOrEmpty(message.Text) || message.Text.Length > 128)
            {
                _logger.LogWarning("Invalid message text: {Text}", message.Text);
                return BadRequest("Invalid message text. Must be between 1 and 128 characters.");
            }
            Message ms = Message.Mapper.Map(Message.DtoFactory.Create(message.Text, DateTime.Now, message.SequenceNumber));
            _messageRepository.AddMessage(ms);
            _logger.LogInformation("Received and saved message: SequenceNumber = {SequenceNumber}, Id = {Id}", message.SequenceNumber, ms.Id);
            _webSocketManager.BroadcastMessage(ms);

            return Ok();


        }
        catch (Exception ex)
        {
            {
                _logger.LogError(ex, "Error processing message.");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
    [HttpGet("api/range")]
    public IActionResult GetMessages(DateTime start, DateTime end)
    {
        try
        {
            if (start > end)
            {
                return BadRequest("Start time must be more than end");
            }
            var messages = _messageRepository.GetMessages(start, end);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving messages from database.");
            return StatusCode(500, "Internal Server Error");
        }
    }
    [HttpGet("ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _webSocketManager.AddWebSocket(webSocket);
            _logger.LogInformation("WebSocket connection established.");
            await HandleWebSocket(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private async Task HandleWebSocket(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            // Просто держим соединение открытым.  Все сообщения отправляются сервером.
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        _logger.LogInformation("WebSocket connection closed.");
        _webSocketManager.RemoveWebSocket(webSocket);
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }


}
public interface IWebSocketManager
{
    void AddWebSocket(WebSocket webSocket);
    void RemoveWebSocket(WebSocket webSocket);
    Task BroadcastMessage(Message message);
}
public class WebSocketManager : IWebSocketManager
{
    private readonly List<WebSocket> _sockets = new List<WebSocket>();
    private readonly ILogger<WebSocketManager> _logger;
    public WebSocketManager(ILogger<WebSocketManager> logger)
    {
        _logger = logger;
    }
    public void AddWebSocket(WebSocket webSocket)
    {
        _sockets.Add(webSocket);
    }


    public async Task BroadcastMessage(Message message)
    {
        string jsonMessage = JsonSerializer.Serialize(message);
        byte[] bytes = Encoding.UTF8.GetBytes(jsonMessage);

        foreach (var socket in _sockets)
        {
            if (socket.State == WebSocketState.Open)
            {
                try
                {
                    await socket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending message to WebSocket client.");
                    RemoveWebSocket(socket);
                }
            }
        }
    }

    public void RemoveWebSocket(WebSocket webSocket)
    {
        _sockets.Remove(webSocket);
    }
}
