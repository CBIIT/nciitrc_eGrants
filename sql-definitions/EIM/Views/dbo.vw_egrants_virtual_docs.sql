SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

create view vw_egrants_virtual_docs
as

select * from vw_egrants_virtual_docs_snapshots

UNION ALL

select * from vw_egrants_virtual_docs_funding

GO

