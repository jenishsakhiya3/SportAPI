-- =========================================================================
-- SportAPI Database Schema Script (Azure SQL Database / SQL Server)
-- Run this script in your Azure SQL Database Query Editor or SSMS
-- =========================================================================

-- Disable foreign key check if re-creating
-- (Azure SQL supports standard table creation order instead)

-- 1. Create Sports Table
CREATE TABLE [Sports] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT [PK_Sports] PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- 2. Create Teams Table
CREATE TABLE [Teams] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(MAX) NOT NULL,
    [City] NVARCHAR(MAX) NOT NULL,
    [SportId] INT NOT NULL,
    CONSTRAINT [PK_Teams] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Teams_Sports_SportId] FOREIGN KEY ([SportId]) REFERENCES [Sports] ([Id]) ON DELETE CASCADE
);

-- 3. Create Players Table
CREATE TABLE [Players] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(MAX) NOT NULL,
    [Age] INT NOT NULL,
    [Position] NVARCHAR(MAX) NOT NULL,
    [TeamId] INT NOT NULL,
    CONSTRAINT [PK_Players] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Players_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [Teams] ([Id]) ON DELETE CASCADE
);

-- 4. Create Matches Table
CREATE TABLE [Matches] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [MatchDate] DATETIME2(7) NOT NULL,
    [SportId] INT NOT NULL,
    [HomeTeamId] INT NOT NULL,
    [AwayTeamId] INT NOT NULL,
    [HomeScore] INT NOT NULL,
    [AwayScore] INT NOT NULL,
    [Status] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT [PK_Matches] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Matches_Sports_SportId] FOREIGN KEY ([SportId]) REFERENCES [Sports] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Matches_Teams_HomeTeamId] FOREIGN KEY ([HomeTeamId]) REFERENCES [Teams] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Matches_Teams_AwayTeamId] FOREIGN KEY ([AwayTeamId]) REFERENCES [Teams] ([Id]) ON DELETE NO ACTION
);

-- 5. Create MatchStats Table
CREATE TABLE [MatchStats] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [MatchId] INT NOT NULL,
    [PossessionHome] INT NOT NULL,
    [PossessionAway] INT NOT NULL,
    [ShotsHome] INT NOT NULL,
    [ShotsAway] INT NOT NULL,
    CONSTRAINT [PK_MatchStats] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MatchStats_Matches_MatchId] FOREIGN KEY ([MatchId]) REFERENCES [Matches] ([Id]) ON DELETE CASCADE
);

-- 6. Create Leagues Table
CREATE TABLE [Leagues] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(MAX) NOT NULL,
    [Country] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT [PK_Leagues] PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- 7. Create Standings Table
CREATE TABLE [Standings] (
    [LeagueId] INT NOT NULL,
    [TeamId] INT NOT NULL,
    [Played] INT NOT NULL,
    [Won] INT NOT NULL,
    [Drawn] INT NOT NULL,
    [Lost] INT NOT NULL,
    [Points] INT NOT NULL,
    CONSTRAINT [PK_Standings] PRIMARY KEY CLUSTERED ([LeagueId] ASC, [TeamId] ASC),
    CONSTRAINT [FK_Standings_Leagues_LeagueId] FOREIGN KEY ([LeagueId]) REFERENCES [Leagues] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Standings_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [Teams] ([Id]) ON DELETE CASCADE
);

-- 8. Create Coaches Table
CREATE TABLE [Coaches] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(MAX) NOT NULL,
    [TeamId] INT NOT NULL,
    [ExperienceYears] INT NOT NULL,
    CONSTRAINT [PK_Coaches] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Coaches_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [Teams] ([Id]) ON DELETE CASCADE
);

-- 9. Create Venues Table
CREATE TABLE [Venues] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(MAX) NOT NULL,
    [City] NVARCHAR(MAX) NOT NULL,
    [Capacity] INT NOT NULL,
    CONSTRAINT [PK_Venues] PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- 10. Create Injuries Table
CREATE TABLE [Injuries] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [PlayerId] INT NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    [ExpectedReturnDate] DATETIME2(7) NOT NULL,
    CONSTRAINT [PK_Injuries] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Injuries_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE CASCADE
);

-- 11. Create Transfers Table
CREATE TABLE [Transfers] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [PlayerId] INT NOT NULL,
    [FromTeamId] INT NOT NULL,
    [ToTeamId] INT NOT NULL,
    [TransferFee] DECIMAL(18,2) NOT NULL,
    [TransferDate] DATETIME2(7) NOT NULL,
    CONSTRAINT [PK_Transfers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Transfers_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Transfers_Teams_FromTeamId] FOREIGN KEY ([FromTeamId]) REFERENCES [Teams] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Transfers_Teams_ToTeamId] FOREIGN KEY ([ToTeamId]) REFERENCES [Teams] ([Id]) ON DELETE NO ACTION
);
