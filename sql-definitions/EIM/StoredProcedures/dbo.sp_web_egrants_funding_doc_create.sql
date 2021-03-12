SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

CREATE PROCEDURE [dbo].[sp_web_egrants_funding_doc_create]

@ApplID			int,
@CategoryID 	int,
@DocDate 		smalldatetime,
@SubCategory	varchar(50),
@FileType		varchar(5), 
@ic				varchar(10), 
@Operator		varchar(50),
@DocumentID		varchar(10) OUTPUT

AS
/****************************************************************************************************************/
/***									 								***/
/***	Procedure Name: sp_web_egrants_funding_doc_create				***/
/***	Description:create funding document								***/
/***	Created:	12/22/2009	Leon									***/
/***	Modified:	11/07/2013	Leon									***/
/***	Modified:	10/13/2015	Leon	set fy value by code			***/
/***	Modified:	02/07/2017	Leon	modify for MVC					***/
/***	Modified:	03/27/2019	Leon	added sub category				***/
/****************************************************************************************************************/

SET NOCOUNT ON

declare 
@fy				int,
@profile_id 	int,
@person_id 		int,
@count			int,
@document_id	int

--find the profile_id and person_id
SET @profile_id=(SELECT profile_id FROM profiles WHERE profile=@ic) 
SET @person_id =(SELECT person_id FROM people WHERE userid=@Operator and profile_id=@profile_id)

--set fy
SET @fy=YEAR(GETDATE())
IF MONTH(GETDATE())>=10 SET @fy=@fy+1

SET @FileType=SUBSTRING(@FileType,2,LEN(@FileType))

---to create new document
INSERT funding_documents(category_id,sub_string,document_date,created_date,created_by_person_id,document_fy) 
SELECT @CategoryID,ISNULL(@SubCategory,null), @DocDate,getdate(),@person_id,@fy		

SELECT @document_id = @@IDENTITY

--insert sub_category with fy 2014
declare @sub_category varchar(50)
set @sub_category=(
select category_name 
from vw_funding_categories 
where category_id=@CategoryID and (category_fy is null or category_fy=2014)
)

if @sub_category is not null
begin
update funding_documents set sub_category=@sub_category
where document_id=@document_id
end

INSERT funding_appls(document_id, appl_id) SELECT @document_id, @ApplID

/**return new  document_id and url **/
SET @Documentid=RIGHT('00000' + CONVERT(varchar,@document_id), 6) 

SELECT @Documentid

GO

