SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE TRIGGER [DDLTrigger_Sample]
    ON DATABASE
    FOR CREATE_PROCEDURE, ALTER_PROCEDURE, DROP_PROCEDURE, CREATE_TABLE, ALTER_TABLE, DROP_TABLE, 
     CREATE_TRIGGER, ALTER_TRIGGER, DROP_TRIGGER,
        CREATE_FUNCTION, ALTER_FUNCTION, DROP_FUNCTION, CREATE_VIEW, ALTER_VIEW, DROP_VIEW
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE
        @EventData XML = EVENTDATA();
 
    DECLARE @ip varchar(48) = CONVERT(varchar(48), 
        CONNECTIONPROPERTY('client_net_address'));
 
    INSERT dbo.Test_DDLEvents
    (
        EventType,
        EventDDL,
        EventXML,
        DatabaseName,
        SchemaName,
        ObjectName,
        HostName,
        IPAddress,
        ProgramName,
        LoginName
    )
    SELECT
        @EventData.value('(/EVENT_INSTANCE/EventType)[1]',   'NVARCHAR(100)'), 
        @EventData.value('(/EVENT_INSTANCE/TSQLCommand)[1]', 'NVARCHAR(MAX)'),
        @EventData,
        DB_NAME(),
        @EventData.value('(/EVENT_INSTANCE/SchemaName)[1]',  'NVARCHAR(255)'), 
        @EventData.value('(/EVENT_INSTANCE/ObjectName)[1]',  'NVARCHAR(255)'),
        HOST_NAME(),
        @ip,
        PROGRAM_NAME(),
        SUSER_SNAME();
END

ENABLE TRIGGER [DDLTrigger_Sample] ON DATABASE
GO

