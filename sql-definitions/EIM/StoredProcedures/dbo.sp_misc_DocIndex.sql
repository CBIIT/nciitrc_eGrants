SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER ON
CREATE PROCEDURE [dbo].[sp_misc_DocIndex] AS


Declare @Phrase varchar(100)
Declare @CategoryID smallint
Declare @Threshhold smallint


--create temp table to hold potential candidates

create table #t(page_id int,rank int)


---------------------------------------------------------------------------------------------------------------------
--Start category index for Face
select @CategoryID=category_id from categories where category_name='Face'

--Use phrase #1
SET @Phrase='entity identification'
SET @Threshhold=90

insert documents_i(page_id,category_id,rank)
select page_id,@CategoryID,rank from
freetexttable(ncieim_b..dt,*,@Phrase) c,ncieim_b..dt d where
d.page_id=c.[key] and
rank>=@Threshhold and page_id not in (select page_id from documents_i)

--Phrase 2

SET @Phrase='"response to specific request"'

insert documents_i(page_id,category_id,rank)
select page_id,@CategoryID ,rank from
containstable(ncieim_b..dt,*,@Phrase) c,ncieim_b..dt d where
d.page_id=c.[key] and page_id not in (select page_id from documents_i)


--Phrase 3

SET @Phrase='"congressional district"'

insert documents_i(page_id,category_id,rank)
select page_id,@CategoryID,rank from
containstable(ncieim_b..dt,*,@Phrase) c,ncieim_b..dt d where
d.page_id=c.[key] and page_id not in (select page_id from documents_i)

--Phrase 4

SET @Phrase='"major subdivision"'

insert documents_i(page_id,category_id,rank)
select page_id,@CategoryID,rank from
containstable(ncieim_b..dt,*,@Phrase) c,ncieim_b..dt d where
d.page_id=c.[key] and page_id not in (select page_id from documents_i)

--Phrase 5


SET @Phrase='"type of organization"'
SET @Threshhold=41


insert documents_i(page_id,category_id,rank)
select page_id,@CategoryID,rank from
containstable(ncieim_b..dt,*,@Phrase) c,ncieim_b..dt d where
d.page_id=c.[key] and page_id not in (select page_id from documents_i) and rank>=@Threshhold


--Phrase 6

SET @Phrase='"Face Page AA"'

insert documents_i(page_id,category_id,rank)
select page_id,@CategoryID,rank from
containstable(ncieim_b..dt,*,@Phrase) c,ncieim_b..dt d where
d.page_id=c.[key] and page_id not in (select page_id from documents_i)


-------------------------------------------------------------------------------------------------------------------------------------

--Next category - Description

SELECT @CategoryID=category_id from categories where category_name='Description'

--Phrase 1

SET @Phrase='relastedness succinct'
SET @Threshhold=100

insert documents_i(page_id,category_id,rank)
select page_id,@CategoryID,rank from
freetexttable(ncieim_b..dt,*,@Phrase) c,ncieim_b..dt d where
d.page_id=c.[key] and
rank>=@Threshhold and page_id not in (select page_id from documents_i)


--------------------------------------------------------------------------------------------------------------------------------------------

--Next category -Abstract

SELECT @CategoryID=category_id from categories where category_name='Abstract'

--Phrase 1

SET @Phrase='"abstract of research plan"'

insert documents_i(page_id,category_id,rank)
select page_id,@CategoryID,rank from
containstable(ncieim_b..dt,*,@Phrase) c,ncieim_b..dt d where
d.page_id=c.[key] and page_id not in (select page_id from documents_i)



----------------------------------------------------------------------------------------------------------------------------------------------------


--Next category - Table of Contents

SELECT @CategoryID=category_id from categories where category_name='Table of Contents'

--Phrase 1

SET @Phrase='"table of contents"'
SET @Threshhold=48

insert documents_i(page_id,category_id,rank)
select page_id,@CategoryID,rank from
containstable(ncieim_b..dt,*,@Phrase) c,ncieim_b..dt d where
d.page_id=c.[key] and page_id not in (select page_id from documents_i) and rank>=@Threshhold


--------------------------------------------------------------------------------------------------------------------------------------------------------


--Next category - Budget

SELECT @CategoryID=category_id from categories where category_name='Budget'

SET @Phrase='"budget justification"'

insert documents_i(page_id,category_id,rank)
select page_id,@CategoryID,rank from
containstable(ncieim_b..dt,*,@Phrase) c,ncieim_b..dt d where
d.page_id=c.[key] and page_id not in (select page_id from documents_i) 


-----------------------------------------------------------------------------------------------------------------------------------------------------


--Next category -BIO

SELECT @CategoryID=category_id from categories where category_name='BIO'

SET @Phrase='"biographical sketch"'

insert documents_i(page_id,category_id,rank)
select page_id,@CategoryID,rank from
containstable(ncieim_b..dt,*,@Phrase) c,ncieim_b..dt d where
d.page_id=c.[key] and page_id not in (select page_id from documents_i) 


-----------------------------------------------------------------------------------------------------------------------------------------------------

--Next category - Other Support

SELECT @CategoryID=category_id from categories where category_name='Other Support'

SET @Phrase='other research support active pending overlap'
SET @Threshhold=40

insert #t(page_id,rank)
select [key],rank from 
freetexttable(ncieim_b..dt,*,@Phrase)
where rank>=@Threshhold

insert documents_i(page_id,category_id,rank)
select page_id,@CategoryID,rank from #t where page_id not in (select page_id from documents_i)


-- c,ncieim_b..dt d where
--d.page_id=c.[key] and rank>=@Threshhold and page_id not in (select page_id from documents_i)


----------------------------------------------------------------------------------------------------------------------------------------------------------


--Next category - Specific Aims

SELECT @CategoryID=category_id from categories where category_name='Specific Aims'

SET @Phrase='"a. specific aims"'
--In this case we use positional information to check if the page is greater than 7

insert documents_i(page_id,category_id,rank)
select page_id,@CategoryID,rank from
containstable(ncieim_b..dt,*,@Phrase) c,ncieim_b..dt d where
d.page_id=c.[key] and page>7and  page_id not in (select page_id from documents_i)

---------------------------------------------------------------------------------------------------------------------------------------------------


--Next category - Checklist

SELECT @CategoryID=category_id from categories where category_name='Checklist'

SET @Phrase='"checklist" and "assurances"'


insert documents_i(page_id,category_id,rank)
select page_id,@CategoryID,rank from
containstable(ncieim_b..dt,*,@Phrase) c,ncieim_b..dt d where
d.page_id=c.[key] and  page_id not in (select page_id from documents_i)





-----------------------------------------------------------------------------------------------------------------------------------------------------------

--Next category - Assurance Human

SELECT @CategoryID=category_id from categories where category_name='Assurance Human'

SET @Phrase='"human subjects"'
SET @Threshhold=57

insert documents_i(page_id,category_id,rank)
select page_id,@CategoryID,rank from
containstable(ncieim_b..dt,*,@Phrase) c,ncieim_b..dt d where
d.page_id=c.[key] and rank>=@Threshhold and  page_id not in (select page_id from documents_i)


----------------------------------------------------------------------------------------------------------------------------------------------------

--Next category - Assurance Animal

SELECT @CategoryID=category_id from categories where category_name='Assurance Animal'

SET @Phrase='"vertebrate animals"'


insert documents_i(page_id,category_id,rank)
select page_id,@CategoryID,rank from
containstable(ncieim_b..dt,*,@Phrase) c,ncieim_b..dt d where
d.page_id=c.[key] and  page_id not in (select page_id from documents_i)


------------------------------------------------------------------------------------------------------------------------------------------------------
GO

