SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Detail](
	[appl_id] [int] NULL,
	[direct_cost_amt] [int] NULL,
	[indirect_cost_amt] [int] NULL,
	[appl_period_start_date] [datetime] NULL,
	[appl_period_end_date] [datetime] NULL,
	[appl_period_num] [int] NULL,
	[period_type_code] [nvarchar](3) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[total_period_amt] [int] NULL,
	[total_oblgtd_amt] [int] NULL,
	[fy] [int] NULL,
	[account_descrip] [nvarchar](30) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[account_code] [int] NULL,
	[budget_item_amt] [int] NULL,
	[cmnt] [int] NULL,
	[budget_item_type_code] [nvarchar](10) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[budget_item_cnt] [int] NULL,
	[can] [int] NULL,
	[display_order_num] [int] NULL,
	[appl_budget_item_id] [int] NULL
) ON [PRIMARY]

GO

