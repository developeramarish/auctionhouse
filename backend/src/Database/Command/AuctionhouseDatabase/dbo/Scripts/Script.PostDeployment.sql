--:r .\Job_ResetPasswordCode.sql
:r .\Generated\EventOutbox.sql
:r .\Generated\SagaNotifications.sql
ALTER DATABASE AuctionhouseDatabase
    SET READ_COMMITTED_SNAPSHOT ON
    GO
    ALTER DATABASE AuctionhouseDatabase
    SET ALLOW_SNAPSHOT_ISOLATION ON
    GO
