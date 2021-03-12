SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[IMPP_Admin_Supplements_WIP](
	[Serial_num] [int] NULL,
	[Supp_appl_id] [int] NULL,
	[Full_grant_num] [varchar](19) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Former_num] [varchar](19) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Former_appl_id] [int] NULL,
	[Submitted_date] [smalldatetime] NULL,
	[movedto_appl_id] [int] NULL,
	[Support_year] [tinyint] NULL,
	[Suffix_code] [varchar](4) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[file_type] [varchar](4) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[category_id] [int] NULL,
	[url] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Created_date] [smalldatetime] NULL,
	[moved_by] [int] NULL,
	[moved_date] [smalldatetime] NULL,
	[adm_supp_wip_id] [int] IDENTITY(1,1) NOT NULL,
	[movedto_document_id] [varchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[sub_category_name] [varchar](35) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[doc_url] [nvarchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO

