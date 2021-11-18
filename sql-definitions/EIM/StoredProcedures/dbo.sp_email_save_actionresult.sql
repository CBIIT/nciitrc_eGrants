SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

/*
                    cmd.Parameters.Add("@emailruleid", SqlDbType.Int).Value = result.RuleId;
                    cmd.Parameters.Add("@EmailMessageid", SqlDbType.Int).Value = result.MessageId;
                    cmd.Parameters.Add("@actionid", SqlDbType.Int).Value = result.ActionId;
                    cmd.Parameters.Add("@actioncompleted", SqlDbType.Bit).Value = result.ActionCompleted;
                    cmd.Parameters.Add("@successful", SqlDbType.Bit).Value = result.Successful;
                    cmd.Parameters.Add("@actionmessage", SqlDbType.VarChar).Value = result.ActionMessage;
                    cmd.Parameters.Add("@actionstarted", SqlDbType.Bit).Value = result.ActionStarted;
                    cmd.Parameters.Add("@errorexception", SqlDbType.VarChar).Value = result.ErrorException.ToString();
                    cmd.Parameters.Add("@createdate", SqlDbType.DateTimeOffset).Value = result.CreatedDate;
*/

CREATE   procedure dbo.sp_email_save_actionresult

@emailruleid Int,
@EmailMessageid Int ,
@actionid Int,
@actioncompleted bit,
@successful Int ,
@actionmessage varchar(500),
@actionstarted bit,
@errorexception varchar(MAX),
@createdate datetimeoffset,
@actiondata varchar(max)

as 
BEGIN

INSERT INTO [dbo].[EmailRulesActionResults]
           ([RuleId]
           ,[MessageId]
           ,[Successful]
           ,[ActionStarted]
           ,[ActionCompleted]
           ,[ActionMessage]
           ,[ExceptionText]
           ,[CreatedDate]
           ,[ActionDataPassed])
     VALUES
           (@emailruleid
           ,@EmailMessageid
           ,@successful
           ,@actionstarted
           ,@actioncompleted
           ,@actionmessage
           ,@errorexception
           ,@createdate
           ,@actiondata)

	  Select @@IDENTITY
END



GO

