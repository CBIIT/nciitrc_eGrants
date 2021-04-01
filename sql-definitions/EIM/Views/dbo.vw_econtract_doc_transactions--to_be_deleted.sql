SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE VIEW [dbo].[vw_econtract_doc_transactions--to_be_deleted]
as
Select  et.transaction_id, et.document_id, et.action_type, ea.action_type_description as action, et.transaction_date,  et.operator, 
	case
		when et.action_type in ('error', 'cover') then et.description
		else ''
		end
	as description
from dbo.econtract_transactions et inner join 
	dbo.econtract_action_type ea on et.action_type = ea.action_type_name



GO

