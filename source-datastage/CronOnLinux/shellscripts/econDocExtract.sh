#!/usr/bin/ksh
#set dirpath=""
#set -x -e

getpdfextract() {
while read line; do
        set $line
	extension=$(echo $1|awk -F\. '{print $2}')
        if [[ "$line" != "" && $extension == "pdf" ]]
        then
          fln=$2
          if [[ -f "$fln" ]]; then 
            if [[ -s "$fln" ]]; then 
              cp $fln /egrants/scripts/econ/OUT-Files
              pdftotext -raw -nopgbrk -q /egrants/scripts/econ/OUT-Files/$1 
              #/egrants/xpdf302/pdftotext /egrants/scripts/econ/OUT-Files/$1
              txtfl=$(echo $1|sed "s/.pdf/.txt/g")
              flsz=`cat /egrants/scripts/econ/OUT-Files/$txtfl|wc -l`
              #echo "=====>>>>>>>"$flsz
              #echo ""
              if [[ $flsz -eq 0 ]]; then
                echo "<Error> file: $fln UNREADABLE</Error>">>$errfile
                echo "<Error> file: $fln UNREADABLE</Error>">>$LogFile
		#rm /egrants/scripts/econ/OUT-Files/$txtfl
		#rm /egrants/scripts/econ/OUT-Files/$1
	      else
	        #sed '
	        #s/\.\.//g
	        #s/+//g
	        #s/ii//g
	        #s/\_//g
	        #s/\,\,//g 
                #s/®//g'</egrants/scripts/econ/OUT-Files/$txtfl  >/egrants/scripts/econ/OUT-Files/imm.txt
                while read $line ; do
		  echo $line | tr -d '®=;:`"<>,./?!@#$%^&(){}[]'|tr -d "-"|tr -d "'" | tr -d "_" 
		  #echo "Imran>>":$line
		done</egrants/scripts/econ/OUT-Files/$txtfl >/egrants/scripts/econ/OUT-Files/imm.txt
                #cat /egrants/scripts/econ/OUT-Files/imm.txt|col -b>/egrants/scripts/econ/OUT-Files/$txtfl 
                rm /egrants/scripts/econ/OUT-Files/$1
	        rm /egrants/scripts/econ/OUT-Files/imm.txt
                echo "<Msg>$1, Extraction completed. File size: $flsz</Msg>">>$LogFile
              fi
            else
              echo "<Error> file: $fln is 0 size</Error>">>$errfile
              echo "<Error> file: $fln is 0 size</Error>">>$LogFile
            fi
          else
            #Gen an error file email to maintenance
            echo "<Error> file: $fln NOT FOUND">>$errfile 
            echo "<Error> file: $fln NOT FOUND">>$LogFile
          fi
        fi
done <${TXTFILE}
}

LogFile="/egrants/scripts/econ/econExtract.LOG"
indir="/egrants/scripts/econ/IN-Files/"
outdir="/egrants/scripts/econ/OUT-Files/"
errfile="/egrants/scripts/econ/errfile.txt"
echo '<Startprocess>'`date '+%m/%d/%y [%H:%M:%S]'`'</startprocess>'>$LogFile
TXTFILE_LIST=`ls $indir*.txt`
for TXTFILE in ${TXTFILE_LIST}
do
        echo "<File>==>Started Working on $TXTFILE</File>">>$LogFile
	getpdfextract
       `mv ${TXTFILE} ${TXTFILE}_done`
	echo "<File>==>Finished working on $TXTFILE</File>">>$LogFile
done
find /egrants/scripts/econ/OUT-Files -type f -size 0 -print|xargs rm
find /egrants/scripts/econ/OUT-Files -type f -size 1 -print|xargs rm
echo "<endprocess>Finished `date '+%m/%d/%y [%H:%M:%S]'` </endprocess>">>$LogFile
exit
