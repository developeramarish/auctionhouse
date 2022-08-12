USE AuctionhouseDatabase
GO
IF OBJECT_ID(N'[OutboxItems]') IS NULL
BEGIN
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;


BEGIN TRANSACTION;


CREATE TABLE [OutboxItems] (
    [Id] bigint NOT NULL IDENTITY,
    [Event] nvarchar(max) NOT NULL,
    [CommandContext_CommandId_Id] nvarchar(max) NULL,
    [CommandContext_CorrelationId_Value] nvarchar(max) NOT NULL,
    [CommandContext_User] uniqueidentifier NULL,
    [CommandContext_HttpQueued] bit NOT NULL,
    [CommandContext_WSQueued] bit NOT NULL,
    [CommandContext_Name] nvarchar(max) NOT NULL,
    [ReadModelNotifications] int NOT NULL,
    [Timestamp] bigint NOT NULL,
    [Processed] bit NOT NULL,
    CONSTRAINT [PK_OutboxItems] PRIMARY KEY ([Id])
);


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220113231236_Initial', N'6.0.1');


COMMIT;


BEGIN TRANSACTION;


DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OutboxItems]') AND [c].[name] = N'CommandContext_CommandId_Id');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [OutboxItems] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [OutboxItems] DROP COLUMN [CommandContext_CommandId_Id];


EXEC sp_rename N'[OutboxItems].[CommandContext_CorrelationId_Value]', N'CommandContext_ExtraData', N'COLUMN';


ALTER TABLE [OutboxItems] ADD [CommandContext_CommandId] nvarchar(max) NOT NULL DEFAULT N'';


ALTER TABLE [OutboxItems] ADD [CommandContext_CorrelationId] nvarchar(max) NOT NULL DEFAULT N'';


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220811232430_01_Add_ExtraData', N'6.0.1');


COMMIT;


BEGIN TRANSACTION;


DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OutboxItems]') AND [c].[name] = N'CommandContext_ExtraData');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [OutboxItems] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [OutboxItems] ALTER COLUMN [CommandContext_ExtraData] nvarchar(max) NULL;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220822035544_02_Null_ExtraData', N'6.0.1');


COMMIT;



END
