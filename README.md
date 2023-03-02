# NCI ITRC eGrants

Source repository for eGrants application and supporting resources.

Cloned Repo
When the repo is cloned you will notice a number of web.config files.

Purpose:
1. Web.Base.Config - This file contains the web.config file that is required to run on the server. 
  - Whenever the code is compiled with the "Debug" confgureation the debug flag IS NOTT removed.
  i.e.
  system.web>
		<compilation debug="true" targetFramework="4.7.2"/>
    ...
   </system.web>
   
   - Whenever the code is compiled with the "Release" configuration the debug flag IS removed and the code is also "Optimized"
   
  or "Release" configurations, the file is not modified and will used as the web.config. 
1. Web.Config - This file contains the web.config file after the trasformations have been applied and is then used in the execution of the application.
