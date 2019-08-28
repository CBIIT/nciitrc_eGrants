#!/bin/ksh
#set dirpath=""
#set -x -e

getpdfextract() {
while read line; do
        set $line
        pgcnt=0
        if [[ "$line" != "" ]]
        then
          fln=$2$1
          if [[ -f "$fln" ]]; then 
	    if [[ -s "$fln" ]]; then 
              cp $fln /egrants/scripts/correspondence/OUT-Files
              pdftotext /egrants/scripts/correspondence/OUT-Files/$1 
              txtfl=$(echo $1|sed "s/.pdf/.txt/g")  
              flsz=`cat /egrants/scripts/correspondence/OUT-Files/$txtfl|wc -w`
	      if [[ $flsz -eq 0 ]]; then
                echo "<Error> file: $fln UNREADABLE</Error>">>$errfile
                echo "<Error> file: $fln UNREADABLE</Error>">>$LogFile
              else
                mv /egrants/scripts/correspondence/OUT-Files/$txtfl /egrants/scripts/correspondence/OUT-Files/imm.txt
                cat /egrants/scripts/correspondence/OUT-Files/imm.txt|col -b>/egrants/scripts/correspondence/OUT-Files/$txtfl 
                rm /egrants/scripts/correspondence/OUT-Files/$1
	        rm /egrants/scripts/correspondence/OUT-Files/imm.txt
                echo "<fln>$1, Extraction completed</fln>">>$LogFile
	      fi
	  else
            echo "<Error> file: $fln is 0 size</Error>">>$errfile  
	    echo "<Error> file: $fln is 0 size</Error>">>$LogFile
          fi  
          else
            #Gen an error file email to maintenance
            echo "<Error> file: $fln DOES NOT EXIST</Error>">>$errfile 
            echo "<Error> file: $fln DOES NOT EXIST</Error>">>$LogFile
          fi
        fi
done <${TXTFILE}
}

LogFile="/egrants/scripts/correspondence/corres.LOG"
indir="/egrants/scripts/correspondence/IN-Files/"
outdir="/egrants/scripts/correspondence/OUT-Files/"
errfile="/egrants/scripts/correspondence/errfile.txt"
echo '<Startprocess>'`date '+%m/%d/%y [%H:%M:%S]'`'</startprocess>'>$LogFile
TXTFILE_LIST=`ls $indir*.txt`
for TXTFILE in ${TXTFILE_LIST}
do
    #echo ${TXTFILE_LIST}
    echo "<File>$TXTFILE</File>">>$LogFile
    cat $TXTFILE|tr -d '\r' > tmp_file
    sed -e "s/^M//" tmp_file > tmp_file1 
    mv tmp_file1 $TXTFILE
    rm tmp_file
    getpdfextract
    `mv ${TXTFILE} ${TXTFILE}_done`
done
find /egrants/scripts/correspondence/OUT-Files -type f -size 0 -print|xargs rm
echo "<endprocess>Finished `date '+%m/%d/%y [%H:%M:%S]'` </endprocess>">>$LogFile
exit
