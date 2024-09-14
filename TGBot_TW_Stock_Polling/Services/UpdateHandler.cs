using Telegram.Bot.Examples.WebHook.Services;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using TGBot_TW_Stock_Polling.Dto;
using TGBot_TW_Stock_Polling.Interface;

namespace Telegram.Bot.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IBotService _botService;
    private readonly TradingView _tradingView;
    private readonly Cnyes _cnyes;

    public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger,
                          TradingView tradingView, Cnyes cnyes, IBotService botService)
    {
        _botClient = botClient;
        _logger = logger;
        _tradingView = tradingView;
        _cnyes = cnyes;
        _botService = botService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            { Message: { } message } => BotOnMessageReceived(message, cancellationToken),
            { EditedMessage: { } message } => BotOnMessageReceived(message, cancellationToken),
            //{ CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
            //{ InlineQuery: { } inlineQuery } => BotOnInlineQueryReceived(inlineQuery, cancellationToken),
            //{ ChosenInlineResult: { } chosenInlineResult } => BotOnChosenInlineResultReceived(chosenInlineResult, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("收到消息類型: {MessageType}", message.Type);

        if (message.Text is not { } messageText)
            return;

        var parts = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return;

        var command = parts[0].ToLowerInvariant();

        switch (command)
        {
            case "/start":
            case "hello":
                await _botService.SendHelloMessageAsync(message, cancellationToken);
                break;

            case "/chart":
            case "/range":
            case "/k":
            case "/v":
            case "/p":
            case "/n":
                if (parts.Length < 2 || !int.TryParse(parts[1], out _))
                {
                    await _botService.SendErrorMessageAsync(message, cancellationToken);
                    return;
                }

                await ProcessStockCommand(command, parts, message, cancellationToken);
                break;

            default:
                await _botService.SendErrorMessageAsync(message, cancellationToken);
                break;
        }
    }

    private async Task ProcessStockCommand(string command, string[] parts, Message message, CancellationToken cancellationToken)
    {
        var stockNumber = parts[1];
        Message reply = new Message();

        try
        {
            reply = await _botService.SendWaitMessageAsync(message, cancellationToken);

            switch (command)
            {
                case "/chart":
                    await _tradingView.GetChartAsync(stockNumber, message.Chat.Id, cancellationToken);
                    break;
                case "/range":
                    var range = parts.Length > 2 ? parts[2] : null;
                    await _tradingView.GetRangeAsync(stockNumber, message.Chat.Id, range, cancellationToken);
                    break;
                case "/k":
                    var kRange = parts.Length > 2 ? GetKRange(parts[2]) : "日K";
                    await _cnyes.GetKlineAsync(stockNumber, message.Chat.Id, kRange, cancellationToken);
                    break;
                case "/v":
                    await _cnyes.GetDetialPriceAsync(stockNumber, message.Chat.Id, cancellationToken);
                    break;
                case "/p":
                    await _cnyes.GetPerformanceAsync(stockNumber, message.Chat.Id, cancellationToken);
                    break;
                case "/n":
                    await _cnyes.GetNewsAsync(stockNumber, message.Chat.Id, cancellationToken);
                    break;
            }
        }
        finally
        {
            if (reply != null)
            {
                await _botService.DeleteMessageAsync(new DeleteDto
                {
                    Message = message,
                    Reply = reply,
                    CancellationToken = cancellationToken
                });
            }
        }
    }

    private string GetKRange(string input)
    {
        return input.ToLowerInvariant() switch
        {
            "h" => "分時",
            "d" => "日K",
            "w" => "週K",
            "m" => "月K",
            "5m" => "5分",
            "10m" => "10分",
            "15m" => "15分",
            "30m" => "30分",
            "60m" => "60分",
            _ => "日K"
        };
    }

    // Process Inline Keyboard callback data
    //private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    //{
    //    _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

    //    await _botClient.AnswerCallbackQueryAsync(
    //        callbackQueryId: callbackQuery.Id,
    //        text: $"Received {callbackQuery.Data}",
    //        cancellationToken: cancellationToken);

    //    await _botClient.SendTextMessageAsync(
    //        chatId: callbackQuery.Message!.Chat.Id,
    //        text: $"Received {callbackQuery.Data}",
    //        cancellationToken: cancellationToken);
    //}

    #region Inline Mode

    //private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery, CancellationToken cancellationToken)
    //{
    //    _logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

    //    InlineQueryResult[] results = {
    //        // displayed result
    //        new InlineQueryResultArticle(
    //            id: "1",
    //            title: "TgBots",
    //            inputMessageContent: new InputTextMessageContent("hello"))
    //    };

    //    await _botClient.AnswerInlineQueryAsync(
    //        inlineQueryId: inlineQuery.Id,
    //        results: results,
    //        cacheTime: 0,
    //        isPersonal: true,
    //        cancellationToken: cancellationToken);
    //}

    //private async Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult, CancellationToken cancellationToken)
    //{
    //    _logger.LogInformation("Received inline result: {ChosenInlineResultId}", chosenInlineResult.ResultId);

    //    await _botClient.SendTextMessageAsync(
    //        chatId: chosenInlineResult.From.Id,
    //        text: $"You chose result with Id: {chosenInlineResult.ResultId}",
    //        cancellationToken: cancellationToken);
    //}

    #endregion

#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RCS1163 // Unused parameter.
    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
#pragma warning restore RCS1163 // Unused parameter.
#pragma warning restore IDE0060 // Remove unused parameter
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}
