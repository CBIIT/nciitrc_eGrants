SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[people_positions](
	[position_id] [smallint] IDENTITY(1,1) NOT NULL,
	[position_name] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO

