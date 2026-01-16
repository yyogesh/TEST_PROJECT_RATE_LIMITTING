-- SQL Script to create the Rate Limit table for SQL Server storage
-- Run this script if you want to use SqlServer storage type for rate limiting

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fpe_RateLimitEntry]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[fpe_RateLimitEntry](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [IpAddress] [nvarchar](50) NOT NULL,
        [WindowStart] [datetime2](7) NOT NULL,
        [RequestCount] [int] NOT NULL,
        [PermitLimit] [int] NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [LastRequestAt] [datetime2](7) NULL,
        CONSTRAINT [PK_fpe_RateLimitEntry] PRIMARY KEY CLUSTERED ([Id] ASC)
    )

    -- Create index on IpAddress and WindowStart for better query performance
    CREATE NONCLUSTERED INDEX [IX_RateLimitEntry_IpAddress_WindowStart] 
    ON [dbo].[fpe_RateLimitEntry] ([IpAddress], [WindowStart])
END
GO
