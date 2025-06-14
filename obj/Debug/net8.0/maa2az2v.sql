IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250606031700_InitialCreate'
)
BEGIN
    CREATE TABLE [WhitelistEntries] (
        [Id] int NOT NULL IDENTITY,
        [TweetUrl] nvarchar(max) NOT NULL,
        [ReferralCode] nvarchar(max) NULL,
        [ReferredBy] nvarchar(max) NULL,
        [ReferralCodeInput] nvarchar(max) NULL,
        CONSTRAINT [PK_WhitelistEntries] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250606031700_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250606031700_InitialCreate', N'9.0.5');
END;

COMMIT;
GO

