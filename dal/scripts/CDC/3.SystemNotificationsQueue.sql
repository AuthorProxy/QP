IF dbo.qp_check_existence('SYSTEM_NOTIFICATION_QUEUE', 'IsUserTable') = 1
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SYSTEM_NOTIFICATION_QUEUE]') AND name = 'CdcLastExecutedLsnId')
    BEGIN
      DROP TABLE [dbo].[SYSTEM_NOTIFICATION_QUEUE];
      PRINT 'DROP [dbo].[SYSTEM_NOTIFICATION_QUEUE]';
    END
END
GO

IF dbo.qp_check_existence('SYSTEM_NOTIFICATION_QUEUE', 'IsUserTable') = 0
BEGIN
  CREATE TABLE [dbo].[SYSTEM_NOTIFICATION_QUEUE] (
    [ID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
    [CdcLastExecutedLsnId] [int] NOT NULL,
    [TRANSACTION_LSN] [varchar](22) NOT NULL,
    [TRANSACTION_DATE] [datetime] NOT NULL,
    [URL] [nvarchar](1024) NOT NULL,
    [TRIES] [numeric](18, 0) NOT NULL,
    [JSON] [nvarchar](max) NULL,
    [SENT] [bit] NOT NULL,
    [CREATED] [datetime] NOT NULL,
    [MODIFIED] [datetime] NOT NULL,
    PRIMARY KEY CLUSTERED([ID] ASC)
);

ALTER TABLE [dbo].[SYSTEM_NOTIFICATION_QUEUE] ADD  CONSTRAINT [DF_SYSTEM_NOTIFICATION_QUEUE_TRIES]  DEFAULT ((0)) FOR [TRIES];
ALTER TABLE [dbo].[SYSTEM_NOTIFICATION_QUEUE] ADD  CONSTRAINT [DF_SYSTEM_NOTIFICATION_QUEUE_SENT]  DEFAULT ((0)) FOR [SENT];
ALTER TABLE [dbo].[SYSTEM_NOTIFICATION_QUEUE] ADD  CONSTRAINT [DF_SYSTEM_NOTIFICATION_QUEUE_CREATED]  DEFAULT (getdate()) FOR [CREATED];
ALTER TABLE [dbo].[SYSTEM_NOTIFICATION_QUEUE] ADD  CONSTRAINT [DF_SYSTEM_NOTIFICATION_QUEUE_MODIFIED]  DEFAULT (getdate()) FOR [MODIFIED];
ALTER TABLE [dbo].[SYSTEM_NOTIFICATION_QUEUE]  WITH CHECK ADD  CONSTRAINT [FK_SYSTEM_NOTIFICATION_QUEUE_SYSTEM_NOTIFICATION_QUEUE] FOREIGN KEY([ID]) REFERENCES [dbo].[SYSTEM_NOTIFICATION_QUEUE] ([ID]);
ALTER TABLE [dbo].[SYSTEM_NOTIFICATION_QUEUE] CHECK CONSTRAINT [FK_SYSTEM_NOTIFICATION_QUEUE_SYSTEM_NOTIFICATION_QUEUE];
PRINT 'CREATE [dbo].[SYSTEM_NOTIFICATION_QUEUE]';
END
GO
