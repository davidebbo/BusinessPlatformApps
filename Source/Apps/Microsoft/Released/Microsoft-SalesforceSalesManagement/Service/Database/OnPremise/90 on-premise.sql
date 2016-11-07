SET ANSI_NULLS              ON;
SET ANSI_PADDING            ON;
SET ANSI_WARNINGS           ON;
SET ANSI_NULL_DFLT_ON       ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER       ON;
go

BEGIN TRANSACTION;
DECLARE @ReturnCode INT;

IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs WHERE [name] = N'Data load and processing')
BEGIN
    EXEC @ReturnCode = msdb.dbo.sp_delete_job @job_name= 'Data load and processing';
    IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback;
END;

SET @ReturnCode = 0;

-- Object:  JobCategory [[Uncategorized (Local)]]
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
    EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]';
    IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback;
END;

DECLARE @jobId BINARY(16);
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'Data load and processing', 
          @enabled=1, 
          @notify_level_eventlog=0, 
          @notify_level_email=0, 
          @notify_level_netsend=0, 
          @notify_level_page=0, 
          @delete_level=0, 
          @category_name=N'[Uncategorized (Local)]', 
          @owner_login_name=N'sa', @job_id = @jobId OUTPUT;

IF (@@ERROR <> 0 OR @ReturnCode <> 0)
    GOTO QuitWithRollback;

-- Object:  Step [Start solution processing]
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Start solution processing', 
    @step_id=1, 
    @cmdexec_success_code=0, 
    @on_success_action=3, 
    @on_success_step_id=0, 
    @on_fail_action=2, 
    @on_fail_step_id=0, 
    @retry_attempts=0, 
    @retry_interval=0, 
    @os_run_priority=0, @subsystem=N'CmdExec', 
    @command=N'powershell.exe -ExecutionPolicy Bypass -file "$(ProgramFiles)\PBIST\SM\PowerShell\Scripts\Invoke solution processing.ps1" -sqlServer $(SQL_SERVER) -sqlDatabase "$(DatabaseName)"', 
    @flags=0;

IF (@@ERROR <> 0 OR @ReturnCode <> 0)
    GOTO QuitWithRollback;

-- Object:  Step [Check data load]
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Check data load', 
    @step_id=2, 
    @cmdexec_success_code=0, 
    @on_success_action=3, 
    @on_success_step_id=0, 
    @on_fail_action=2, 
    @on_fail_step_id=0, 
    @retry_attempts=0, 
    @retry_interval=0, 
    @os_run_priority=0, @subsystem=N'CmdExec', 
    @command=N'powershell.exe -ExecutionPolicy Bypass -file "$(ProgramFiles)\PBIST\SM\PowerShell\Scripts\Check solution status.ps1" -sqlServer $(SQL_SERVER) -sqlDatabase "$(DatabaseName)"',
    @flags=0;

IF (@@ERROR <> 0 OR @ReturnCode <> 0)
    GOTO QuitWithRollback;

-- Object:  Step [Check data load]
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Update domain account information',
    @step_id=3, 
    @cmdexec_success_code=0, 
    @on_success_action=3, 
    @on_success_step_id=0, 
    @on_fail_action=2, 
    @on_fail_step_id=0, 
    @retry_attempts=0, 
    @retry_interval=0, 
    @os_run_priority=0, @subsystem=N'CmdExec', 
    @command=N'powershell.exe -ExecutionPolicy Bypass -file "$(ProgramFiles)\PBIST\SM\PowerShell\Scripts\Update domain accounts.ps1" -sqlServer $(SQL_SERVER) -sqlDatabase "$(DatabaseName)"',
    @flags=0;

IF (@@ERROR <> 0 OR @ReturnCode <> 0)
    GOTO QuitWithRollback;


-- Object:  Step [Process SSAS database]
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Process SSAS database', 
    @step_id=4, 
    @cmdexec_success_code=0, 
    @on_success_action=1, 
    @on_success_step_id=0, 
    @on_fail_action=2, 
    @on_fail_step_id=0, 
    @retry_attempts=0, 
    @retry_interval=0, 
    @os_run_priority=0, @subsystem=N'ANALYSISCOMMAND', 
    @command=N'<Process xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
    <Object>
    <DatabaseID>$(SSAS_DB)</DatabaseID>
    </Object>
    <Type>ProcessFull</Type>
</Process>', 
    @server=N'localhost', 
    @database_name=N'$(DatabaseName)', 
    @flags=0;

IF (@@ERROR <> 0 OR @ReturnCode <> 0)
    GOTO QuitWithRollback;

EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1;
IF (@@ERROR <> 0 OR @ReturnCode <> 0)
    GOTO QuitWithRollback;

EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)';
IF (@@ERROR <> 0 OR @ReturnCode <> 0)
    GOTO QuitWithRollback;

COMMIT TRANSACTION

BEGIN TRANSACTION

SET @ReturnCode = 0;
-- Create the job which will save the credential
IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs WHERE [name] = N'Save credential')
BEGIN
    EXEC @ReturnCode = msdb.dbo.sp_delete_job @job_name= 'Save credential';
    IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback;
END;

SET @jobId = NULL;

EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'Save credential', 
    @enabled=1, 
    @notify_level_eventlog=0, 
    @notify_level_email=0, 
    @notify_level_netsend=0, 
    @notify_level_page=0, 
    @delete_level=0, 
    @description=N'No description available.', 
    @category_name=N'[Uncategorized (Local)]', 
    @owner_login_name=N'sa', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0)
    GOTO QuitWithRollback;

EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Encrypt', 
    @step_id=1, 
    @cmdexec_success_code=0, 
    @on_success_action=1, 
    @on_success_step_id=0, 
    @on_fail_action=2, 
    @on_fail_step_id=0, 
    @retry_attempts=0, 
    @retry_interval=0, 
    @os_run_priority=0, @subsystem=N'CmdExec', 
    @command=N'powershell.exe -ExecutionPolicy Bypass -file "$(ProgramFiles)\PBIST\SM\PowerShell\Scripts\Encrypt password for SQL agent.ps1" -sqlServer $(SQL_SERVER) -sqlDatabase "$(DatabaseName)"', 
    @flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0)
    GOTO QuitWithRollback;
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1;
IF (@@ERROR <> 0 OR @ReturnCode <> 0)
    GOTO QuitWithRollback;

EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)';
IF (@@ERROR <> 0 OR @ReturnCode <> 0)
    GOTO QuitWithRollback;

COMMIT TRANSACTION

GOTO EndSave

QuitWithRollback:
    IF (@@TRANCOUNT > 0)
      ROLLBACK TRANSACTION;
EndSave:

go

-- CREATE login for SSAS
IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name='NT SERVICE\MSSQLServerOLAPService')
BEGIN
    CREATE LOGIN [NT SERVICE\MSSQLServerOLAPService] FROM WINDOWS WITH DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english];
END;
go

USE [$(DatabaseName)]
go

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name='NT SERVICE\MSSQLServerOLAPService')
BEGIN
    CREATE USER [NT SERVICE\MSSQLServerOLAPService] FOR LOGIN [NT SERVICE\MSSQLServerOLAPService] WITH DEFAULT_SCHEMA=[dbo]
END;
go

ALTER ROLE [db_datareader] ADD MEMBER [NT SERVICE\MSSQLServerOLAPService]