﻿using System;
using System.Threading.Tasks;
using NServiceBus;

class Program
{
    static void Main()
    {
        Start().GetAwaiter().GetResult();
    }

    static async Task Start()
    {
        var endpointConfiguration = ConfigBuilder.Build("Saga");
        var endpoint = await Endpoint.Start(endpointConfiguration);
        Console.WriteLine("Press 'Enter' to start a saga");
        Console.WriteLine("Press any other key to exit");
        try
        {
            while (true)
            {
                var key = Console.ReadKey();
                Console.WriteLine();

                if (key.Key != ConsoleKey.Enter)
                {
                    return;
                }
                var startSagaMessage = new StartSagaMessage
                {
                    MySagaId = Guid.NewGuid()
                };
                await endpoint.SendLocal(startSagaMessage);
            }
        }
        finally
        {
            await endpoint.Stop();
        }

    }
}