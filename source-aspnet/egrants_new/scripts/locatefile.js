/*check selected file for upload */

function check_file() {
    //check fiel
    if (!document.getElementById("customFile").value) {
        alert("Please select a file to upload!");
        return false;
    } else var filetype = file_type(document.getElementById("UploadFile").value);

    //check file type
    if (filetype == 'false') {
        document.getElementById("customFile").value = null;
        alert("This file type is not allowed. Please only upload the following file types: '.msg', '.txt', '.pdf', '.docx', '.doc', '.xls', '.xlsx','.xlsm'");
        return false;
    } else var filesize = document.getElementById("UploadFile").files[0].size;

    //check file size
    if (filesize > 1500000000) {
        document.getElementById("customFile").value = null;
        alert("File size too large, please send to BOB Team");
        return false;
    } else return true;
}





