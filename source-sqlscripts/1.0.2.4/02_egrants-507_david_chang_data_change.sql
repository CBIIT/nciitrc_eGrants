update documents
set category_id = 627
from documents d
where category_id = 671
and not exists (select 1 
				from documents
				where appl_id = d.appl_id
				and category_id = 627);

delete documents
from documents d
where category_id = 671
and exists (select 1 
			from documents
			where appl_id = d.appl_id
			and category_id = 627);

delete categories
from categories c
where category_id = 671;
