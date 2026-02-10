using DAL_Celebrity_MSSQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
//using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using System;
using System.IO;
namespace ANC25_WEBAPI_DLL {
    public static class LifeEventsEndpoints
    {
        public static IEndpointRouteBuilder MapLifeEventsEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/lifeevents");

            group.MapGet("/", (IRepository repo) =>
            {
                var events = repo.GetAllLifeevents();
                return events ?? throw new ArgumentNullException("No events found");
            });

            group.MapGet("/{id:int:min(1)}", (IRepository repo, int id) =>
            {
                var lifeEvent = repo.GetLifeeventById(id);
                return lifeEvent ?? throw new ArgumentException($"Event {id} not found");
            });

            group.MapGet("/celebrities/{celebrityId:int:min(1)}", (IRepository repo, int celebrityId) =>
            {
                var events = repo.GetLifeeventsByCelebrityId(celebrityId);
                return events ?? throw new ArgumentException($"Events for celebrity {celebrityId} not found");
            });

            group.MapDelete("/{id:int:min(1)}", (IRepository repo, int id) =>
            {
                var lifeEvent = repo.GetLifeeventById(id);
                if (lifeEvent == null)
                {
                    throw new ArgumentException($"Event {id} not found");
                }
                repo.DelLifeevent(id);
                return Results.Ok($"Event {id} deleted");
            });

            group.MapPost("/", (IRepository repo, Lifeevent lifeEvent) =>
            {
                repo.AddLifeevent(lifeEvent);
                var events = repo.GetLifeeventsByCelebrityId(lifeEvent.CelebrityId);
                if (events == null)
                {
                    throw new InvalidOperationException("Failed to add event");
                }
                return Results.Created($"/lifeevents/celebrities/{lifeEvent.CelebrityId}", events);
            });

            group.MapPut("/{id:int:min(1)}", (IRepository repo, int id, Lifeevent lifeEvent) =>
            {
                var existingEvent = repo.GetLifeeventById(id);
                if (existingEvent == null)
                {
                    throw new ArgumentException($"Event {id} not found");
                }
                repo.UpdLifeevent(id, lifeEvent);
                return Results.Ok(lifeEvent);
            });

            return builder;
        }
    }
}
