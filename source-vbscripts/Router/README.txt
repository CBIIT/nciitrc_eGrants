
C# VB script router

	When the folder corresponding to this folder in a shared environment is configured it should have files like this in it :
	
		config.csv
		Router.exe
		Router.exe.config
		Router.pdb

	These files should be created from the EmailHandling project in the eGrants repo :
		nciitrc_eGrants\source-csxscripts\EmailHandling
	If you build the project, it will create a bin directory (if it doesn't already exist) and the needed elements should be in there.
	When deploying, make sure to use the EXISTING configure that is tailored to the specific environment (see PITFALL) below.
		
	Pitfall :
		All of the old vb script config should work with the C# config (same formatting, everything), except for the database connection string, which no longer uses the PROVIDER part. Remove that section from the connection string.
		


