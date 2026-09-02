using Microsoft.EntityFrameworkCore;
using SportAPI.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using System.Diagnostics;

namespace SportAPI.Endpoints;

public static class SportEndpoints
{
    public static void MapSportEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").RequireAuthorization();

        // 1. GET /api/sports (Delay: 50ms)
        api.MapGet("/sports", async (SportDbContext db) =>
        {
            await Task.Delay(5000);
            return Results.Ok(await db.Sports.ToListAsync());
        }).WithName("GetSports");

        // 2. POST /api/sports (Delay: 100ms)
        api.MapPost("/sports", async (SportDbContext db, Sport sport) =>
        {
            await Task.Delay(1000);
            db.Sports.Add(sport);
            await db.SaveChangesAsync();
            return Results.Created($"/api/sports/{sport.Id}", sport);
        }).WithName("CreateSport");

        // 3. GET /api/teams (Delay: 150ms)
        api.MapGet("/teams", async (SportDbContext db, int? sportId) =>
        {
            await Task.Delay(1500);
            var query = db.Teams.AsQueryable();
            if (sportId.HasValue)
            {
                query = query.Where(t => t.SportId == sportId.Value);
            }
            return Results.Ok(await query.ToListAsync());
        }).WithName("GetTeams");

        // 4. POST /api/teams (Delay: 200ms)
        api.MapPost("/teams", async (SportDbContext db, Team team) =>
        {
            await Task.Delay(2000);
            db.Teams.Add(team);
            await db.SaveChangesAsync();
            return Results.Created($"/api/teams/{team.Id}", team);
        }).WithName("CreateTeam");

        // 5. GET /api/players (Delay: 250ms)
        api.MapGet("/players", async (SportDbContext db, int? teamId) =>
        {
            await Task.Delay(2500);
            var query = db.Players.AsQueryable();
            if (teamId.HasValue)
            {
                query = query.Where(p => p.TeamId == teamId.Value);
            }
            return Results.Ok(await query.ToListAsync());
        }).WithName("GetPlayers");

        // 6. POST /api/players (Delay: 300ms)
        api.MapPost("/players", async (SportDbContext db, Player player) =>
        {
            await Task.Delay(3000);
            db.Players.Add(player);
            await db.SaveChangesAsync();
            return Results.Created($"/api/players/{player.Id}", player);
        }).WithName("CreatePlayer");

        // 7. PUT /api/players/{id} (Delay: 350ms)
        api.MapPut("/players/{id:int}", async (SportDbContext db, int id, Player playerInput) =>
        {
            await Task.Delay(3500);
            var player = await db.Players.FindAsync(id);
            if (player == null) return Results.NotFound();

            player.Name = playerInput.Name;
            player.Age = playerInput.Age;
            player.Position = playerInput.Position;
            player.TeamId = playerInput.TeamId;

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("UpdatePlayer");

        // 8. DELETE /api/players/{id} (Delay: 400ms)
        api.MapDelete("/players/{id:int}", async (SportDbContext db, int id) =>
        {
            await Task.Delay(4000);
            var player = await db.Players.FindAsync(id);
            if (player == null) return Results.NotFound();

            db.Players.Remove(player);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("DeletePlayer");

        // 9. GET /api/matches (Delay: 450ms)
        api.MapGet("/matches", async (SportDbContext db) =>
        {
            await Task.Delay(4500);
            return Results.Ok(await db.Matches.ToListAsync());
        }).WithName("GetMatches");

        // 10. POST /api/matches (Delay: 500ms)
        api.MapPost("/matches", async (SportDbContext db, Match match) =>
        {
            await Task.Delay(500);
            db.Matches.Add(match);
            await db.SaveChangesAsync();
            return Results.Created($"/api/matches/{match.Id}", match);
        }).WithName("CreateMatch");

        // 11. PUT /api/matches/{id}/score (Delay: 550ms)
        api.MapPut("/matches/{id:int}/score", async (SportDbContext db, int id, MatchScoreUpdate scoreUpdate) =>
        {
            await Task.Delay(5500);
            var match = await db.Matches.FindAsync(id);
            if (match == null) return Results.NotFound();

            match.HomeScore = scoreUpdate.HomeScore;
            match.AwayScore = scoreUpdate.AwayScore;
            match.Status = scoreUpdate.Status;

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("UpdateMatchScore");

        // 12. GET /api/matches/{id}/stats (Delay: 600ms)
        api.MapGet("/matches/{id:int}/stats", async (SportDbContext db, int id) =>
        {
            await Task.Delay(6000);
            var stats = await db.MatchStats.FirstOrDefaultAsync(s => s.MatchId == id);
            if (stats == null) return Results.NotFound();
            return Results.Ok(stats);
        }).WithName("GetMatchStats");

        // 13. POST /api/matches/{id}/stats (Delay: 650ms)
        api.MapPost("/matches/{id:int}/stats", async (SportDbContext db, int id, MatchStat statInput) =>
        {
            await Task.Delay(6500);
            var matchExists = await db.Matches.AnyAsync(m => m.Id == id);
            if (!matchExists) return Results.NotFound("Match not found");

            var stats = await db.MatchStats.FirstOrDefaultAsync(s => s.MatchId == id);
            if (stats == null)
            {
                statInput.MatchId = id;
                db.MatchStats.Add(statInput);
            }
            else
            {
                stats.PossessionHome = statInput.PossessionHome;
                stats.PossessionAway = statInput.PossessionAway;
                stats.ShotsHome = statInput.ShotsHome;
                stats.ShotsAway = statInput.ShotsAway;
            }

            await db.SaveChangesAsync();
            return Results.Ok(stats ?? statInput);
        }).WithName("UpsertMatchStats");

        // 14. GET /api/leagues (Delay: 700ms)
        api.MapGet("/leagues", async (SportDbContext db) =>
        {
            await Task.Delay(700);
            return Results.Ok(await db.Leagues.ToListAsync());
        }).WithName("GetLeagues");

        // 15. POST /api/leagues (Delay: 750ms)
        api.MapPost("/leagues", async (SportDbContext db, League league) =>
        {
            await Task.Delay(7500);
            db.Leagues.Add(league);
            await db.SaveChangesAsync();
            return Results.Created($"/api/leagues/{league.Id}", league);
        }).WithName("CreateLeague");

        // 16. GET /api/standings/{leagueId} (Delay: 800ms)
        api.MapGet("/standings/{leagueId:int}", async (SportDbContext db, int leagueId) =>
        {
            await Task.Delay(8000);
            var standings = await db.Standings
                .Where(s => s.LeagueId == leagueId)
                .OrderByDescending(s => s.Points)
                .ToListAsync();
            return Results.Ok(standings);
        }).WithName("GetStandings");

        // 17. GET /api/coaches (Delay: 850ms)
        api.MapGet("/coaches", async (SportDbContext db) =>
        {
            await Task.Delay(8050);
            return Results.Ok(await db.Coaches.ToListAsync());
        }).WithName("GetCoaches");

        // 18. POST /api/coaches (Delay: 900ms)
        api.MapPost("/coaches", async (SportDbContext db, Coach coach ) =>
        {
            await Task.Delay(900);
            db.Coaches.Add(coach);
            await db.SaveChangesAsync();
            return Results.Created($"/api/coaches/{coach.Id}", coach);
        }).WithName("CreateCoach");

        // 19. POST /api/players/{id}/transfer (Delay: 950ms)
        api.MapPost("/players/{id:int}/transfer", async (SportDbContext db, int id, TransferRequest transferReq) =>
        {
            await Task.Delay(9500);
            var player = await db.Players.FindAsync(id);
            if (player == null) return Results.NotFound("Player not found");

            var fromTeamId = player.TeamId;

            // Update player's team
            player.TeamId = transferReq.ToTeamId;

            // Create transfer record
            var transfer = new Transfer
            {
                PlayerId = id,
                FromTeamId = fromTeamId,
                ToTeamId = transferReq.ToTeamId,
                TransferFee = transferReq.TransferFee,
                TransferDate = DateTime.UtcNow
            };
            db.Transfers.Add(transfer);

            await db.SaveChangesAsync();
            return Results.Ok(transfer);
        }).WithName("TransferPlayer");

        // 20. GET /api/dashboard/summary (Delay: 1000ms)
        api.MapGet("/dashboard/summary", async (SportDbContext db) =>
        {
            await Task.Delay(10000);
            var sportsCount = await db.Sports.CountAsync();
            var teamsCount = await db.Teams.CountAsync();
            var playersCount = await db.Players.CountAsync();
            var matchesCount = await db.Matches.CountAsync();

            return Results.Ok(new
            {
                SportsCount = sportsCount,
                TeamsCount = teamsCount,
                PlayersCount = playersCount,
                MatchesCount = matchesCount
            });
        }).WithName("GetDashboardSummary");

        // 20b. GET /api/diagnostics/db-check (Tests database connection and queries table)
        api.MapGet("/diagnostics/db-check", async (SportDbContext db) =>
        {
            try
            {
                var canConnect = await db.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return Results.Ok(new { Success = false, Message = "Connection test failed. The database is unreachable." });
                }

                // Query Sports table to verify schema exists
                var sportsCount = await db.Sports.CountAsync();
                return Results.Ok(new { Success = true, Message = "Connection and query successful!", SportsCount = sportsCount });
            }
            catch (Exception ex)
            {
                return Results.Json(new
                {
                    Success = false,
                    ErrorType = ex.GetType().Name,
                    Message = ex.Message,
                    Details = ex.ToString()
                }, statusCode: 500);
            }
        }).WithName("DbCheck");

        // 21. GET /api/diagnostics/cpu-stress (Stresses CPU to trigger Autoscale)
        api.MapGet("/diagnostics/cpu-stress", (int? seconds) =>
        {
            var secs = seconds ?? 30;
            var endTime = DateTime.UtcNow.AddSeconds(secs);
            var processorCount = Environment.ProcessorCount;

            for (int i = 0; i < processorCount; i++)
            {
                _ = Task.Run(() =>
                {
                    while (DateTime.UtcNow < endTime)
                    {
                        // Spin CPU core
                        _ = Math.Sqrt(Random.Shared.NextDouble());
                    }
                });
            }

            return Results.Ok(new
            {
                Message = $"Stressing {processorCount} CPU cores at 100% load for {secs} seconds.",
                TargetTimeUtc = endTime
            });
        }).WithName("CpuStress");

        api.MapGet("/storage/check/{containerName}", async (string containerName, BlobServiceClient blobServiceClient) =>
        {
            try
            {
                // 1. Get a reference to the container
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // 2. Check if it exists and is accessible using the current credentials
                bool exists = await containerClient.ExistsAsync();

                if (exists)
                {
                    return Results.Ok(new 
                    { 
                        IsAccessible = true, 
                        Message = $"Successfully accessed container: {containerName}" 
                    });
                }

                return Results.NotFound(new 
                { 
                    IsAccessible = false, 
                    Message = $"Container '{containerName}' does not exist or is not accessible." 
                });
            }
            catch (Exception ex)
            {
                // Catch authentication, network, or permission errors
                return Results.Problem(
                    detail: ex.Message, 
                    statusCode: 503, // Service Unavailable
                    title: "Azure Blob Storage Connection Failed"
                );
            }
        }).WithName("CheckStorageAccess");
    }
}

public record MatchScoreUpdate(int HomeScore, int AwayScore, string Status);
public record TransferRequest(int ToTeamId, decimal TransferFee);
