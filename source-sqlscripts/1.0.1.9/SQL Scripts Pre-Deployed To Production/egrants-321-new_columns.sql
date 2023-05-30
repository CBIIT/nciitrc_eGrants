ALTER TABLE [dbo].GM_ACTION_QUEUE_VW ADD [ACTION_ID] NUMERIC(10) NULL;

ALTER TABLE [dbo].Grant_Contacts_PD_GS ADD [ACTION_ID] NUMERIC(10) NULL;

SELECT * INTO dbo.grant_contacts_pd_Gs_bkup FROM dbo.grant_contacts_pd_Gs;

TRUNCATE TABLE grant_contacts_pd_Gs;