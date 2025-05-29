USE [EIM]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[people_sent_warning](
	[person_id] [int] NOT NULL,
	[email_sent] [int] NULL,
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[people_sent_warning]  WITH CHECK ADD FOREIGN KEY([person_id])
REFERENCES [dbo].[people] ([person_id])
GO

INSERT INTO [EIM].[dbo].[people_sent_warning] (person_id, email_sent)
SELECT person_id, 0 AS email_sent from [EIM].[dbo].[people]