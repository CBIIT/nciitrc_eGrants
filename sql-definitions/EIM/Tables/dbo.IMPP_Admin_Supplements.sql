SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[IMPP_Admin_Supplements](
	[Supp_appl_id] [int] NOT NULL,
	[Full_grant_num] [varchar](19) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Former_Num] [varchar](19) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[serial_num] [int] NULL,
	[Former_appl_id] [int] NULL,
	[Action_date] [smalldatetime] NULL,
	[admin_supp_action_code] [varchar](5) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[APPL_TYPE_CODE] [char](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[ACTIVITY_CODE] [varchar](3) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[ADMIN_PHS_ORG_CODE] [varchar](2) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[SUPPORT_YEAR] [tinyint] NULL,
	[SUFFIX_CODE] [varchar](4) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[ACCESSION_NUMBER] [int] NULL,
	[eRa_TS] [datetime] NULL
) ON [PRIMARY]

GO

