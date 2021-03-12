SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

CREATE FUNCTION ContainerTypeID (@Container varchar(50))  
RETURNS tinyint AS  
BEGIN 


IF (SELECT COUNT(*)  FROM locations WHERE location=@Container)=1
RETURN 1



IF (SELECT COUNT(*)  FROM specialists WHERE specialist_name=@Container)=1
RETURN 3


RETURN 2



 
END

GO

