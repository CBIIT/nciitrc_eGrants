SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[impp_grant_images_t](
	[appl_id] [int] NOT NULL,
	[accession_num] [int] NOT NULL,
	[total_pages_num] [smallint] NULL,
	[created_date] [smalldatetime] NULL,
	[last_upd_date] [smalldatetime] NULL
) ON [PRIMARY]

GO

