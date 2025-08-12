
	Unchecked in changes:
		Certain changes can't be checked in or they will cause problems in the shared environments.
		But the changes are needed for running locally.
		
			source-aspnet/egrants_new/egrants_new.csproj
				A version of this file that runs locally is here are in the same folder as this READ_ME.txt :
					nciitrc_eGrants\HelpAndRestore\egrants_new_latest.csproj.txt
				Of course it should not have the .txt at the end when you set it up on your local.
					Just paste it into the project file.
					
			nciitrc_eGrants\source-aspnet\egrants_new
				A version of this file that runs locally is here are in the same folder as this READ_ME.txt :
					source-aspnet/egrants_new/Web.config.txt
				This might not be necessary as there are there are several different environments set up by Robin in the
					Visual Studio dropdown run debug menu.
	
			nciitrc_eGrants/source-aspnet/egrants_new/Global.asax.cs :
				Modify the UserId to return your user Id. E.g. :
					protected string UserID
					{
						get
						{
							this.userid = this.Context.Request.ServerVariables["HEADER_SM_USER"];
							if (this.userid == null)
							{
								this.userid = "";
			#if DEBUG

								this.userid = "hooverrl"; // should correspond to person table, column: active
			 #endif
							}

							return this.userid;
						}
					}
					
			web.config and web.base.config
				Please update the GitHubToken in the connection strings in both files to run the application locally.
				This token can be found in Jenkins or will be present with another developer
	

		
	Visual Studio
		You should not need to run this project with elevated permissions, but if for some reason you do find yourself needing to do that, the previous developer, Robin recommended creating a file on the desktop and naming it something like access.bat, and then populating it like this :
			net localgroup administrators hooverrl /add
		And then opening up an elevated cmd console and then cd to Desktop and then type "access.bat".
		Then you can right click on Visual Studio, and run with elevated access. You'll have to type a password (again).
		
		
	Generated Code
		This project does not use meta-programming.
		
	SQL access :
		When running this project, you have to have some Auth-Z configured in the database.
		Start by looking at the data in the People table:
			select * from [EIM].[dbo].[People]
		Search for a person_name like your name and see if there is already something there.
		If not, insert it into the database.
			Look at person_name like '%Hoover, Micah%' to see an example of what it should look like.
			Make sure profile_id is 1, position_id is 8, active is 1, team_id is 0, cft and econ and gft and mgt and egrants and admin and docman are all 1, that start_date is GETDATE(), created_by is 3243, created_date is GETDATE(), last_updated_by is 3243, and last_updated_date is GETDATE().
		Once it is inserted, query for it to get the person_id.
		