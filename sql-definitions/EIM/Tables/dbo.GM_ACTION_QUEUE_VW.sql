SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[GM_ACTION_QUEUE_VW](
	[APPL_ID] [numeric](10, 0) NULL,
	[APPL_TYPE_CODE] [nvarchar](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[SERIAL_NUM] [numeric](6, 0) NOT NULL,
	[SUPPORT_YEAR] [numeric](2, 0) NOT NULL,
	[ACTION_FY] [numeric](4, 0) NOT NULL,
	[SUFFIX_CODE] [nvarchar](4) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[RESP_SPEC_FULL_NAME_CODE] [nvarchar](62) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[RESP_SPEC_EMAIL_ADDRESS] [nvarchar](80) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[PD_EMAIL_ADDRESS] [nvarchar](80) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[PD_FULL_NAME] [nvarchar](62) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[ACTION_TYPE] [varchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[ADMIN_PHS_ORG_CODE] [varchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[RESP_SPEC_NPN_ID] [varchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[to_delete] [nchar](10) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO

