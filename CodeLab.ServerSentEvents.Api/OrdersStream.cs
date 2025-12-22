using System.Net.ServerSentEvents;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;

namespace CodeLab.ServerSentEvents.Api;

public static class OrdersStream
{
    public static void MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("orders/realtime", async (
            ChannelReader<OrderPlacement> channelReader,
            CancellationToken cancellationToken
            ) =>
        {
            // Here we are using one of the examples of the Server-Sent Events (SSE) support in .NET 10
            // Return a stream of server-sent events (SSE) for real-time order placements
            // The "orders" event type is specified for the SSE
            // The channelReader reads all incoming OrderPlacement messages
            return Results.ServerSentEvents(channelReader.ReadAllAsync(cancellationToken), "orders");
        })
            .WithTags("Orders");

        app.MapGet("orders/realtime/with-events", async (
            ChannelReader<OrderPlacement> channelReader,
            OrderEventBuffer eventBuffer,
            [FromHeader(Name = "Last-Event-ID")] string? lastEventId,
            CancellationToken cancellationToken
            ) =>
        {
            async IAsyncEnumerable<SseItem<OrderPlacement>> StreamEvents()
            {
                if (!string.IsNullOrWhiteSpace(lastEventId))
                {
                    var missedEvent = eventBuffer.GetEventsAfter(lastEventId);

                    foreach (var sseItem in missedEvent)
                    {
                        yield return sseItem;
                    }
                }

                await foreach (var order in channelReader.ReadAllAsync(cancellationToken))
                {
                    var sseItem = eventBuffer.Add(order);
                    yield return sseItem;
                }
            }

            return TypedResults.ServerSentEvents(StreamEvents(), "orders");
        });
    }
}