// Данная работа предназначена показать освоенные знания по Orleans.
// Работа выполнена в течение одного рабочего дня (точнее сказать сложнее, т.к. делалась с перерывами)
// Т.к. это моя первая работа с Orleans, то я не стал усложнять ее, а сделал простой пример с минимальным функционалом.
// В данном коде не будет какой либо архитектуры, проектирования, обработки ошибок и т.д.
// Также в коде практически нет комментариев в связи с его простотой.
// Исходное задание:
//
//
// Игра:
// - когда новый игрок подключается к серверу он попадает в очередь
// - когда в очереди есть два игрока они попадают в комнату, в комнате сервер загадывает число от 0 до 100
// - когда игрок попадает в комнату он загадывает число от 0 до 100 (вводит из консоли) и передает его на сервер
// - игрок который назвал число ближе к тому которое загадает сервер - получает очко
// - число очков должно сохраняться для каждого игрока в бд (persistence которая встроена в orleans)
//
//
// PS Загадываемые числа должны быть от 0 включительно до 100 НЕ включительно.


using Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseOrleansClient((ctx, client) =>
    {
        int instanceId = ctx.Configuration.GetValue<int>("InstanceId");
        client.UseLocalhostClustering(30000 + instanceId);
    })
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.None);
        logging.AddFilter("Orleans", LogLevel.Critical);
        logging.AddFilter("Microsoft.Orleans", LogLevel.Critical);
    })
    .UseConsoleLifetime();

using IHost host = builder.Build();
await host.StartAsync();

var grainFactory = host.Services.GetRequiredService<IGrainFactory>();

// Guid example: "9D2B0228-4D0D-4C23-8B49-01A698857709";
var gameController = new GameController(grainFactory);

bool exit = false;
while (!exit)
{
    var playerId = gameController.GetPlayerId();
    Console.Clear();
    Console.Write($"Player ID: {playerId}\n" +
        "1. Start Game\n" +
        "2. Set Player ID\n" +
        "3. Get Score\n" +
        "0. Exit" +
        "\nChoose item: ");

    ConsoleKeyInfo key = Console.ReadKey(false);
    Console.Clear();
    switch (key.Key)
    {
        case ConsoleKey.D1:
            await StartGame();
            break;
        case ConsoleKey.D2:
            SetPlayerId();
            break;
        case ConsoleKey.D3:
            await GetScore();
            break;
        case ConsoleKey.D0:
            exit = true;
            break;
    }
}

await host.StopAsync();

async Task StartGame()
{
    Console.WriteLine("Find a game...");
    var roomId = await gameController.FindGame();

    int value;
    do
    {
        Console.Clear();
        Console.Write($"Game started. Room ID: {roomId}\n" +
            "Enter a number between 0 (inclusive) and 100 (exclusive): ");

        string input = Console.ReadLine()?.Trim();
        int.TryParse(input, out value);
    }
    while (value < 0 || value >= 100);

    Console.Clear();
    Console.WriteLine($"Send number '{value}'...");
    var result = await gameController.SendNumber(roomId, value);

    Console.Clear();
    var playerId = gameController.GetPlayerId();
    if (playerId == result.Item1)
    {
        Console.WriteLine($"You win! Server number is {result.Item2}");
    }
    else
    {
        Console.WriteLine($"You lose! Server number is {result.Item2}");
    }
    
    Console.WriteLine("Press enter to continue...");
    Console.ReadLine();
}

async Task GetScore()
{
    int score = await gameController.GetScore();
    Console.WriteLine($"Your score is {score}");
    Console.WriteLine("Press enter to continue...");
    Console.ReadLine();
}

void SetPlayerId()
{
    string text;
    do
    {
        Console.Clear();
        Console.Write("Write text (not a guid): ");
        text = Console.ReadLine();
    }
    while (string.IsNullOrWhiteSpace(text));
    
    gameController.SetPlayerId(text);
}