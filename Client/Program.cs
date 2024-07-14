using Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using GrainInterfaces;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseOrleansClient(client => { client.UseLocalhostClustering(); })
    .ConfigureLogging(logging => logging.AddConsole())
    .UseConsoleLifetime();

using IHost host = builder.Build();
await host.StartAsync();

IClusterClient client = host.Services.GetRequiredService<IClusterClient>();

var grainFactory = host.Services.GetRequiredService<IGrainFactory>();
var gameController = new GameController(grainFactory);

Console.WriteLine("Find game...");
var gameId = await gameController.FindGame();

int value;
do
{
    Console.Write("Enter a number between 0 (inclusive) and 100 (exclusive): ");
    value = Random.Shared.Next(100);
    Console.WriteLine(value);
}
while (value < 0 && value >= 100);

Console.WriteLine("Make a move...");
var result = await gameController.MakeMove(gameId, value);
switch (result)
{
    case GameOutcome.Win:
        Console.WriteLine("You win!");
        break;
    case GameOutcome.Lose:
        Console.WriteLine("You lose!");
        break;
    case GameOutcome.Draw:
        Console.WriteLine("Draw!");
        break;
    default: 
        throw new ArgumentOutOfRangeException(nameof(result), result, "Unknown game result");
}

int score = await gameController.GetScore();
Console.WriteLine($"Your score is {score}");

// // Login (set nickname)
// string nickname;
// do
// {
//     // nickname = Console.ReadLine();
//     nickname = "Player1";
// }
// while (string.IsNullOrEmpty(nickname));
//
// await gameController.SetUser(nickname);
//
// // Read game value
// int value = Random.Shared.Next(0, 101);
//
// // Start game
// int result = await gameController.StartGame(value);
// var player = client.GetGrain<IPlayerGrain>(GetGuid);
//
// IHello friend = client.GetGrain<IHello>(0);
// string response = await friend.SayHello("Hi friend!");
//
// Console.WriteLine($"""
//                    {response}
//
//                    Press any key to exit...
//                    """);
//
// Console.ReadKey();

await host.StopAsync();

Console.ReadLine();