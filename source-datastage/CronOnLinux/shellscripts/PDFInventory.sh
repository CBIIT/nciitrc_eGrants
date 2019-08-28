#!/bin/ksh
#set dirpath=""
#set -x -e

getpdfinfo() {
while read line; do
        set $line
        pgcnt=0
        if [[ "$line" != "" ]]
        then
          fln=$2$1
          if [[ -r "$fln" ]]; then 
	    pgcnt=$(/usr/local/bin/pdfinfo -meta $fln|grep "Pages:"|awk '{print $2}')
            echo $1,$pgcnt,$2>>$outfile
            echo "<fln>$1,$pgcnt,$2</fln>">>$LogFile
          else
            #Even if a file is unreadable report page count 0
            #Report Error for pagecount=0 in apps(web page)
            echo $1,$pgcnt,$2>>$outfile
            echo "<fln>$1,$pgcnt,$2</fln>">>$LogFile
            #Gen an error file email to maintenance
            echo "<Error> file: $fln UNREADABLE </Error>">>$errfile
            echo "<Error> file: $fln UNREADABLEi </Error>">>$LogFile
          fi
          #echo $1,$pgcnt,$2
        fi
done <${TXTFILE}
}

LogFile="/egrants/pdfinventory/PDFInventory.LOG"
indir="/egrants/pdfinventory/IN-Files/"
outfile="/egrants/pdfinventory/OUT-Files/inventory.txt"
errfile="/egrants/pdfinventory/OUT-Files/errfile_"`date '+%m%d%y_%H%M%S'`".txt"
echo '<Startprocess>'`date '+%m/%d/%y [%H:%M:%S]'`'</startprocess>'>$LogFile
TXTFILE_LIST=`ls $indir*.txt`
for TXTFILE in ${TXTFILE_LIST}
do
	#echo ${TXTFILE}
	cat $TXTFILE|tr -d '\r' > tmp_file
	mv tmp_file $TXTFILE
    	echo "<File>Started : $TXTFILE </File>">>$LogFile
	echo "<info>Transformation complete</info>">>$LogFile
	getpdfinfo
    	`mv ${TXTFILE} ${TXTFILE}_done`
	echo "<info>Done working with file: $TXTFILE </info>">>$LogFile
done
echo "<endprocess>Finished `date '+%m/%d/%y [%H:%M:%S]'` </endprocess>">>$LogFile
exit
