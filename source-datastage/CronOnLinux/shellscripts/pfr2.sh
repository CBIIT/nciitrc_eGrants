#!/bin/ksh
#set -x

generateXMLfile()
{
applcnt=0
while read line; do
        set $line
	fldrid=19
	pgcnt=0
	echo $line
        if [[ "$line" != "" ]]
        then
          applid=$1
	  xmlflnm=$applid"_"$filenumber.xml
	  pdfflnm=$applid"_"$filenumber.pdf
          alias=$(echo ${TXTFILE}|awk 'BEGIN {FS="/"} {print $5}')
          dt=`/egrants/filetime.pl ${TXTFILE}`
          echo "<Root>">$outdir$xmlflnm
          echo "<document>">>$outdir$xmlflnm
          echo "<applid>$applid</applid>">>$outdir$xmlflnm
          echo "<folderid>$fldrid</folderid>">>$outdir$xmlflnm
          echo "<uploadid></uploadid>">>$outdir$xmlflnm
          echo "<alias>$pdfflnm</alias>">>$outdir$xmlflnm
          echo "<filename>$pdfflnm</filename>">>$outdir$xmlflnm
          echo "<subject></subject>">>$outdir$xmlflnm
          echo "<date>$dt</date>">>$outdir$xmlflnm
          echo "<uid>nciogastage</uid>">>$outdir$xmlflnm
          echo "<file_type>pdf</file_type>">>$outdir$xmlflnm
          echo "</document>">>$outdir$xmlflnm
          echo "</Root>">>$outdir$xmlflnm
	  `cp $PDFName $outdir/$pdfflnm `
	  echo "<applid>$applid XML:Done, PDF:Done</applid>">>$LogFile
	  set applcnt = applcnt + 1
        fi
done <${TXTFILE}
if [[ $appcnt -ne $applcnt ]]
then
	echo '<Error>'`date '+%m/%d/%y [%H:%M:%S]'` File : $TXTFILE  COUNT=$appcnt DOES NOT MATCH WITH APPL COUNT=$applcnt '</Error>' >> $errfile
fi

}

getPDFName()
{
        IFS=/
        set $TXTFILE
        str=$5
        IFS=_
        set $str
        filenum=$4
        IFS=.
        set $filenum
	filenumber=$1
	echo $1

}
#LogFile=echo "/egrants/scripts/PFR2Script/pfr2LOG_"`date '+%m/%d/%y [%H:%M:%S]'`".xml"
LogFile="/egrants/scripts/PFRScript/pfr2LOG.xml"
indir="/egrants/PFR/funding_plan/"
outdir="/egrants/scripts/PFRScript/OUT_Files/pfr2/"
errfile="/egrants/scripts/PFRScript/OUT_Files/error/pfr2Error.txt"
errwebfolder="/egrants/funded/egrantsadmin/errors/pfr/pfr2Error.txt"
logwebfolder="/egrants/funded/egrantsadmin/logs/pfr/pfr2Log.xml"
echo '<root>'>$LogFile
echo '<msg>Begin Process:'`date '+%m/%d/%y [%H:%M:%S]'`'</msg>'>>$LogFile
TXTFILE_LIST=`ls $indir*.TXT`
echo "<metadataFiles>Files: "`ls $indir*.TXT`"</metadataFiles>">>$LogFile
echo "<PDFFiles>Files: "`ls $indir*.PDF`"</PDFFiles>">>$LogFile
TXTFLCNT=0
for TXTFILE in ${TXTFILE_LIST}
do
filenumber=0
appcnt=0
	filenumber=$(getPDFName $TXTFILE $filenumber)
	PDFName=$indir"PKG_FP_"$filenumber.PDF
	echo "<metadata>"$TXTFILE>>$LogFile 
	echo "<pdf>"$PDFName>>$LogFile
        if [ -f $PDFName ];
        then
           	pgcnt=$(pdfinfo -meta $PDFName|grep "Pages:"|awk '{print $2}')
           	echo "<pagecnt>"$pgcnt"</pagecnt>">>$LogFile
		echo " ">>$TXTFILE
		echo "<msg>Appended a blank line EOF</msg>">>$LogFile
		$appcnt = cat $TXTFILE|wc -l
		generateXMLfile $TXTFILE $PDFName $filenumber $appcnt
        else
		echo '<Error>' File : $PDFName  NOT FOUND'</Error>' >> $LogFile
		echo '<Error>'`date '+%m/%d/%y [%H:%M:%S]'` File : $PDFName  NOT FOUND'</Error>' >> $errfile
		echo "<TextFilelist>Files: "`ls $indir*.TXT`"</TextFilelist>">>$errfile
		echo "<PDFList>Files : "`ls $indir*.PDF`"</PDFList>">>$errfile
        fi

    	`mv ${TXTFILE} ${TXTFILE}"_done"`
	`mv $PDFName $PDFName"_done"`
	echo "</pdf>">>$LogFile
        echo "</metadata>">>$LogFile
	set $TXTFLCNT = $TXTFLCNT + 1
done
`cp /egrants/scripts/PFRScript/OUT_Files/pfr2/* /egrants/scripts/PFRScript/OUT_Files/pfr2/dev/`
`cp /egrants/scripts/PFRScript/OUT_Files/pfr2/* /egrants/scripts/PFRScript/OUT_Files/pfr2/stage/`
`cp /egrants/scripts/PFRScript/OUT_Files/pfr2/* /egrants/scripts/PFRScript/OUT_Files/pfr2/prod/`

echo "<msg>End Processing: "`date "+%m/%d/%y [%H:%M:%S]"`"</msg>">>$LogFile
if [ -f $errfile ]; then
	echo '<msg>' FOUND ERRORS '</msg>' >> $LogFile
	cat $errfile >> $errwebfolder
	`rm $errfile `
	echo "<msg>Finished Copying error files</msg>">>$LogFile
fi
echo "</root>">>$LogFile
if [[ $TXTFLCNT -gt 0 ]]
then
	cat $LogFile>>$logwebfolder
fi
exit
