SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[institutions_index](
	[index_id] [int] IDENTITY(1,1) NOT NULL,
	[ins_index] [nchar](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[index_seq] [int] NOT NULL
) ON [PRIMARY]

GO

