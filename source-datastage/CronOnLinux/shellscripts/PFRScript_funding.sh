#!/bin/ksh
#set dirpath=""
#set -x -e

DIR_LIST=`ls -l /egrants/PFR/ | grep ^d | awk '{print $9}'`
for DIR in ${DIR_LIST}
do 


#xmlflnm=PFR_`date '+%m%d%y%H%M%S'`.xml
LogFile="/egrants/scripts/PFRScript/PFRScript.LOG"
indir="/egrants/PFR/$DIR/"
stagedir="/egrants/scripts/PFRScript/OUT_Files/stage/"
outdir="/egrants/scripts/PFRScript/OUT_Files/$DIR/"
errfile="/egrants/scripts/PFRScript/PFRScript.txt"
echo '<Startprocess>'`date '+%m/%d/%y [%H:%M:%S]'`'</startprocess>'>$LogFile
#file type is not capitalized properly PDF vs pdf
if [[ $DIR == 'package' ]];
then   
  #TXTFILE_LIST=`ls $indir*.PDF`
  TXTFILE_LIST=`ls -l $indir*.PDF|awk '{if ($5 != 0) print $9}'`
  fldrid=19
  for TXTFILE in ${TXTFILE_LIST}
  do
        sup=$(echo ${TXTFILE}|cut -d"_" -f1|awk 'BEGIN {FS="/"} {print $5}')
	if [[ $sup == 'SUPP' ]];
	then
		fld=3
		stagedir="/egrants/scripts/PFRScript/OUT_Files/stage/SUPP/"
		xmlflnm=SUPP_PFR_`date '+%m%d%y%H%M%S'`.xml
		echo "<Msg>SUPPLEMENT:$outdir  Metadata:$xmlflnm</Msg>">>$LogFile
	else
                fld=2
                stagedir="/egrants/scripts/PFRScript/OUT_Files/stage/$DIR/"
		xmlflnm=PFR_`date '+%m%d%y%H%M%S'`.xml
		echo "<Msg>Package:$outdir  Metadata:$xmlflnm</Msg>">>$LogFile
        fi 
	applid=$(echo ${TXTFILE}|cut -d"_" -f$fld | cut -d"_" -f1)
        alias=$(echo ${TXTFILE}|awk 'BEGIN {FS="/"} {print $5}')
	dt=`/egrants/filetime.pl ${TXTFILE}`
	echo "<Root>">$stagedir$xmlflnm	
        echo "<document>">>$stagedir$xmlflnm
        echo "<applid>$applid</applid>">>$stagedir$xmlflnm
        echo "<folderid>$fldrid</folderid>">>$stagedir$xmlflnm
        echo "<uploadid></uploadid>">>$stagedir$xmlflnm
        echo "<alias>$alias</alias>">>$stagedir$xmlflnm
        echo "<filename>$alias</filename>">>$stagedir$xmlflnm
        echo "<subject></subject>">>$stagedir$xmlflnm
        echo "<date>$dt</date>">>$stagedir$xmlflnm
        echo "<uid>nciogastage</uid>">>$stagedir$xmlflnm
        echo "<file_type>pdf</file_type>">>$stagedir$xmlflnm
	echo "</document>">>$stagedir$xmlflnm
	echo "</Root>">>$stagedir$xmlflnm
        `cp $TXTFILE $stagedir`
        echo "File processed and copied to Stage Directory">>$LogFile
	echo `ls -l $TXTFILE`>>$LogFile
        #`cp $TXTFILE $outdir`
 	`mv ${TXTFILE} ${TXTFILE}_done` 
	echo "Source File Marked ..Done">>$LogFile
	sleep 1
  done
  `cp /egrants/scripts/PFRScript/OUT_Files/stage/SUPP/* /egrants/scripts/PFRScript/OUT_Files/SUPP/dev/`
  `cp /egrants/scripts/PFRScript/OUT_Files/stage/SUPP/* /egrants/scripts/PFRScript/OUT_Files/SUPP/stage/`
  `cp /egrants/scripts/PFRScript/OUT_Files/stage/SUPP/* /egrants/scripts/PFRScript/OUT_Files/SUPP/prod/`
  `rm /egrants/scripts/PFRScript/OUT_Files/stage/SUPP/*.*`
  #`cp /egrants/scripts/PFRScript/OUT_Files/stage/* /egrants/scripts/PFRScript/OUT_Files/SUPP/`

  `cp /egrants/scripts/PFRScript/OUT_Files/stage/package/* /egrants/scripts/PFRScript/OUT_Files/package/dev/`
  `cp /egrants/scripts/PFRScript/OUT_Files/stage/package/* /egrants/scripts/PFRScript/OUT_Files/package/stage/`
  `cp /egrants/scripts/PFRScript/OUT_Files/stage/package/* /egrants/scripts/PFRScript/OUT_Files/package/prod/`
  `rm /egrants/scripts/PFRScript/OUT_Files/stage/package/*.*`
  #`cp /egrants/scripts/PFRScript/OUT_Files/stage/package/* /egrants/scripts/PFRScript/OUT_Files/package/`

fi
done
echo "<endprocess>Finished `date '+%m/%d/%y [%H:%M:%S]'` </endprocess>">>$LogFile
exit

