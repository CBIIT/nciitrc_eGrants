var dtCh= "/";
var minYear=1900;
var maxYear=2100;

//-----------start Date Validation by Imran
function isDate(dtStr){
                var daysInMonth = DaysArray(12)
                var pos1=dtStr.indexOf(dtCh)
                var pos2=dtStr.indexOf(dtCh,pos1+1)
                var strMonth=dtStr.substring(0,pos1)
                var strDay=dtStr.substring(pos1+1,pos2)
                var strYear=dtStr.substring(pos2+1)
                strYr=strYear
                if (strDay.charAt(0)=="0" && strDay.length>1) strDay=strDay.substring(1)
                if (strMonth.charAt(0)=="0" && strMonth.length>1) strMonth=strMonth.substring(1)
                for (var i = 1; i <= 3; i++) {
                                if (strYr.charAt(0)=="0" && strYr.length>1) strYr=strYr.substring(1)
                }

                month=parseInt(strMonth)
                day=parseInt(strDay)
                year=parseInt(strYr)

                if (pos1==-1 || pos2==-1){
                                alert("Enter valid date");
                                return false;
                }
                if (strMonth.length<1 || month<1 || month>12){
                                alert("Enter valid month");
                                return false;
                }
                if (strDay.length<1 || day<1 || day>31 || (month==2 && day>daysInFebruary(year)) || day > daysInMonth[month]){
                                alert("Enter valid date");
                                return false;
                }
                if (strYear.length != 4 || year==0 || year<minYear || year>maxYear){
                                alert("Enter valid year");
                                return false;
                }
                if (dtStr.indexOf(dtCh,pos2+1)!=-1 || isInteger(stripCharsInBag(dtStr, dtCh))==false){
                                alert("Enter valid date");
                                return false;
                }

		//get current date
		var currentTime = new Date();
		var month = currentTime.getMonth() + 1;
		var day = currentTime.getDate();
		var year = currentTime.getFullYear();
		var D1=Date.parse(month + "/" + day + "/" + year);
		var D2=Date.parse(dtStr);

		if (D2>D1){
			alert("Enter valid date");
            return false;
		}
		return true
}

//-----------start Date Validation by Leon
function isAnyDate(dtStr) {
    var daysInMonth = DaysArray(12)
    var pos1 = dtStr.indexOf(dtCh)
    var pos2 = dtStr.indexOf(dtCh, pos1 + 1)
    var strMonth = dtStr.substring(0, pos1)
    var strDay = dtStr.substring(pos1 + 1, pos2)
    var strYear = dtStr.substring(pos2 + 1)
    strYr = strYear
    if (strDay.charAt(0) == "0" && strDay.length > 1) strDay = strDay.substring(1)
    if (strMonth.charAt(0) == "0" && strMonth.length > 1) strMonth = strMonth.substring(1)
    for (var i = 1; i <= 3; i++) {
        if (strYr.charAt(0) == "0" && strYr.length > 1) strYr = strYr.substring(1)
    }

    month = parseInt(strMonth)
    day = parseInt(strDay)
    year = parseInt(strYr)

    if (pos1 == -1 || pos2 == -1) {
        alert("Enter valid date");
        return false;
    }
    if (strMonth.length < 1 || month < 1 || month > 12) {
        alert("Enter valid month");
        return false;
    }
    if (strDay.length < 1 || day < 1 || day > 31 || (month == 2 && day > daysInFebruary(year)) || day > daysInMonth[month]) {
        alert("Enter valid date");
        return false;
    }
    if (strYear.length != 4 || year == 0 || year < minYear || year > maxYear) {
        alert("Enter valid year");
        return false;
    }
    if (dtStr.indexOf(dtCh, pos2 + 1) != -1 || isInteger(stripCharsInBag(dtStr, dtCh)) == false) {
        alert("Enter valid date");
        return false;
    }

    return true
}

//Call this fx to validate sequernce number
function validateText(numb){ 
	if (parseInt(numb)=="Nan"){
		alert("Enter valid integer")
		return false
	}else return true
} 

function daysInFebruary(year){
    return (((year % 4 == 0) && ( (!(year % 100 == 0)) || (year % 400 == 0))) ? 29 : 28 );
}

function DaysArray(n){
      for (var i=1; i<= n; i++) {
		this[i] = 31
		if (i==4 || i==6 || i==9 || i==11) {this[i] = 30}
		if (i==2) {this[i] = 29}
	} return this
}

function isInteger(s){
	var i;
	for (i = 0; i < s.length; i++){   
        // Check that current character is number.
        var c = s.charAt(i);
        if (((c < "0") || (c > "9"))) return false;
    }
    // All characters are numbers.
    return true;
}

function isCurrency(s) {
    var i;
    for (i = 0; i < s.length; i++) {
        // Check that current character is number.
        var c = s.charAt(i);
        if ((c != ".") && (c != ",") && ((c < "0") || (c > "9"))) return false;
    }
    // All characters are numbers.
    return true;
}

function stripCharsInBag(s, bag){
    var i;
    var returnString = "";
    // Search through string's characters one by one.
    // If character is not in bag, append to returnString.
    for (i=0; i<s.length; i++){   
        var c = s.charAt(i);
        if (bag.indexOf(c) == -1) returnString += c;
    }
    return returnString;
}
//----------------end Date Validation 

//----------------start Characters Validation for upload
function CheckCharacters(filelocation, stringtype) {   
    var backslash = filelocation.lastIndexOf("\\");
    var filename = filelocation.substr(backslash + 1, filelocation.length);

    var d = 0;
    for (var i = 0; i < filename.length; i++) {
        if (filename.charAt(i) == ".") {
            d = d + 1;
        }
    }

    if (d > 1) {
        alert("Don't use '.' to " + stringtype + ", correct it and try again");
        var result = "false";
        return result;
    } 

    var iChars = "\/:*?<|";
	for (var i = 0; i<filelocation.length; i++){
  		if (iChars.indexOf(filelocation.charAt(i)) != -1) {
			var character=filelocation.charAt(i);
			if (stringtype!=null && stringtype !='undefined'){
				alert("Don't use special character [ "+ character +" ] as " + stringtype);	
			}else {
          			alert ("Don't use special character [ "+ character +" ] to name file or folder, correct it and try again");
 			}
			var result="false";
			return result;
  		}
	}		
}

function CheckACMusername(username, stringtype){
	var iChars = "\/:*?<|";
	for (var i = 0; i<username.length; i++){
  		if (iChars.indexOf(username.charAt(i)) != -1) {
			var character=username.charAt(i);
			alert("Don't use special character [ "+ character +" ] as " + stringtype);	
			var result="false";
			return result;
  		}
	}		
}

function file_type(filelocation) {

    var dot = filelocation.lastIndexOf(".");

    var filetype = filelocation.substr(dot + 1, filelocation.length);

    var fileTypeLowerCase = filetype.toLowerCase();
    if (fileTypeLowerCase == 'pdf' ||
        fileTypeLowerCase == 'xls' ||
        fileTypeLowerCase == 'xlsx' ||
        fileTypeLowerCase == 'xlsm' ||
        fileTypeLowerCase == 'txt' ||
        fileTypeLowerCase == 'doc' ||
        fileTypeLowerCase == 'docx' ||
        fileTypeLowerCase == 'msg') {
        return filetype;
    } else {

        return 'false';
    }

//    if (filetype == 'pdf' ||
//        filetype == 'xls' ||
//        filetype == 'xlsx' ||
//        filetype == 'xlsm' ||
//        filetype == 'txt' ||
//        filetype == 'doc' ||
//        filetype == 'docx' ||
//        filetype == 'msg') {
//        return filetype;
//    } else {
//         return 'false';
//    }

}

/*check selected file for upload */
function check_file() {
    //check fiel
    if (!document.getElementById("customFile").value) {
        alert("Please select a file to upload!");
        return false;
    } else var filetype = file_type(document.getElementById("customFile").value);

    //check file type
    if (filetype == 'false') {
        document.getElementById("customFile").value = null;
        alert("Please only upload file with file type as 'pdf','xls','xlsx','xlsm','txt','doc','docx' or 'msg'");
        return false;
    } else var filesize = document.getElementById("customFile").files[0].size;

    //check file size
    if (filesize > 1500000000) {
        document.getElementById("customFile").value = null;
        alert("File size too large, please send to BOB Team");
        return false;
    } else return true;
}