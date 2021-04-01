SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[impp_nga](
	[appl_id] [int] NULL,
	[nga_create_date] [smalldatetime] NOT NULL,
	[nga_id] [int] NOT NULL,
	[rpt_seq_num] [int] NULL,
	[loaded] [bit] NULL,
	[noa] [bit] NULL
) ON [PRIMARY]

GO

