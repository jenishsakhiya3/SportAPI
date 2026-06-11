using System;

namespace SportAPI.Data;

public class Sport
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

public class Team
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string City { get; set; }
    public int SportId { get; set; }
    public Sport? Sport { get; set; }
}

public class Player
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Age { get; set; }
    public required string Position { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
}

public class Match
{
    public int Id { get; set; }
    public DateTime MatchDate { get; set; }
    public int SportId { get; set; }
    public Sport? Sport { get; set; }
    public int HomeTeamId { get; set; }
    public Team? HomeTeam { get; set; }
    public int AwayTeamId { get; set; }
    public Team? AwayTeam { get; set; }
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public required string Status { get; set; } // Scheduled, Live, Finished
}

public class MatchStat
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public Match? Match { get; set; }
    public int PossessionHome { get; set; }
    public int PossessionAway { get; set; }
    public int ShotsHome { get; set; }
    public int ShotsAway { get; set; }
}

public class League
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Country { get; set; }
}

public class Standing
{
    public int LeagueId { get; set; }
    public League? League { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public int Played { get; set; }
    public int Won { get; set; }
    public int Drawn { get; set; }
    public int Lost { get; set; }
    public int Points { get; set; }
}

public class Coach
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public int ExperienceYears { get; set; }
}

public class Venue
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string City { get; set; }
    public int Capacity { get; set; }
}

public class Injury
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public Player? Player { get; set; }
    public required string Description { get; set; }
    public DateTime ExpectedReturnDate { get; set; }
}

public class Transfer
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public Player? Player { get; set; }
    public int FromTeamId { get; set; }
    public Team? FromTeam { get; set; }
    public int ToTeamId { get; set; }
    public Team? ToTeam { get; set; }
    public decimal TransferFee { get; set; }
    public DateTime TransferDate { get; set; }
}
