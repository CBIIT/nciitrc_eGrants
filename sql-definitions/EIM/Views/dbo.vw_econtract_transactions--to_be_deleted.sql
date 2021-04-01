SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE VIEW [dbo].[vw_econtract_transactions--to_be_deleted]
as
Select  sel.TR_ID, sel.document_id,  sel.action_type, et.transaction_date,  et.operator, et.description
from dbo.econtract_transactions et inner join 
(SELECT action_type, document_id ,max(transaction_id) TR_ID
from dbo.econtract_transactions 
group by document_id, action_type) sel on et.transaction_id = sel.TR_ID 
		

GO

