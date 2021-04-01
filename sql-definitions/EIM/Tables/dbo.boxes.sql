SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[boxes](
	[box_id] [int] IDENTITY(5000,1) NOT NULL,
	[accession_id] [int] NOT NULL,
	[box_number] [int] NOT NULL,
	[container_type_id] [tinyint] NOT NULL,
	[box_id_old] [int] NULL
) ON [PRIMARY]

GO

