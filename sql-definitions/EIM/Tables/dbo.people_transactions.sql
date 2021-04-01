SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[people_transactions](
	[Transaction_Id] [int] IDENTITY(1,1) NOT NULL,
	[Created_by] [int] NOT NULL,
	[Created_date] [date] NOT NULL,
	[User_person_id] [int] NOT NULL,
	[Transaction_type] [varchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[History_people_id] [int] NULL
) ON [PRIMARY]

GO

