/*
filedrag.js - HTML5 File Drag & Drop 
*/
// getElementById
var dropedfile = null;
var droppedFiles = null;
//var frmdata = new FormData();

function $id(id) {
    return document.getElementById(id);
}

// output information
function Output(msg) {
    var m = $id("dropArea");
    m.innerHTML = msg;
}

window.addEventListener("dragenter", function (e) {
    if (e.target.id != dropArea) {
        e.preventDefault();
        //e.dataTransfer.effectAllowed = "none";
        e.dataTransfer.dropEffect = "none";
    }
}, false);

window.addEventListener("dragover", function (e) {
    if (e.target.id != dropArea) {
        e.preventDefault();
        //e.dataTransfer.effectAllowed = "none";
        e.dataTransfer.dropEffect = "none";
    }
});

function FileDragHover(e) {   
    e.stopPropagation();
    e.preventDefault();
    $('#dropArea').addClass('active-drop');
}

function FileDragleave(e) {
    e.stopPropagation();
    e.preventDefault();
    $('#dropArea').removeClass('active-drop');
    $('#dropArea').html('');
}

// file selection
function FileSelectHandler(e) {
    // cancel event and hover styling
    FileDragHover(e);
    dropedfile = null;
    dropedfiles = null;
    // fetch file object
   
    dropedfiles = e.target.files || e.dataTransfer.files;  

    if (dropedfiles.length == 1) {

        dropedfile = dropedfiles[0];

        var filext = dropedfile.name.split('.').pop();
        var fileExtLowerCase = filext.toLowerCase();

        $('#dropArea').addClass('active-drop');
        var extArr = ['pdf', 'xls', 'xlsm', 'xlsx', 'txt', 'doc', 'docx', 'msg'];

        if ((extArr.indexOf(fileExtLowerCase) > -1) == false) {
            alert("This file type is not allowed. Please only upload the following file types: '.msg', '.txt', '.pdf', '.docx', '.doc', '.xls', '.xlsx','.xlsm'");
            $('#dropArea').removeClass('active-drop');
            $('#dropArea').html('Drag-drop only one file here to upload');
            $('#btnDragdrop').attr('disabled', true);
            $('#btnPdfDragdrop').attr('disabled', true);
            return false;
        } else var filesize = (dropedfile.size / 1000);
  
        if (filesize > 1500000) {
            alert("File size too large, please send to BOB Team");
            $('#dropArea').removeClass('active-drop');
            $('#dropArea').html('Drag-drop only one file here to upload');
            $('#btnDragdrop').attr('disabled', true);
            $('#btnPdfDragdrop').attr('disabled', true);
            return false;
        }

        ParseFile(dropedfile);
        //$('#dropArea').removeClass('active-drop');
        console.log('Bytes Loaded: ' + dropedfile);

        // since there's just one file, okay to do a regular, non-PDF add here
        $('#btnFileUpload').attr('disabled', false);
        $('#btnDragdrop').attr('disabled', false);
    } else if (dropedfiles.length > 1) {
        for (var i = 0; i < dropedfiles.length; i++) {
            dropedfile = dropedfiles[i];

            var filext = dropedfile.name.split('.').pop();
            var fileExtLowerCase = filext.toLowerCase();

            $('#dropArea').addClass('active-drop');
            var extArr = ['pdf', 'xls', 'xlsm', 'xlsx', 'txt', 'doc', 'docx', 'msg'];

            if ((extArr.indexOf(fileExtLowerCase) > -1) == false) {
                alert("This file type is not allowed. Please only upload the following file types: '.msg', '.txt', '.pdf', '.docx', '.doc', '.xls', '.xlsx','.xlsm'");
                $('#dropArea').removeClass('active-drop');
                $('#dropArea').html('Drag-drop only one file here to upload');
                $('#btnDragdrop').attr('disabled', true);
                $('#btnPdfDragdrop').attr('disabled', true);
                return false;
            } else var filesize = (dropedfile.size / 1000);

            //alert(filesize);      
            if (filesize > 1500000) {
                alert("File size too large, please send to BOB Team");
                $('#dropArea').removeClass('active-drop');
                $('#dropArea').html('Drag-drop only one file here to upload');
                $('#btnDragdrop').attr('disabled', true);
                $('#btnPdfDragdrop').attr('disabled', true);
                return false;
            }

            dropedfile = null;
        }
        ParseFiles(dropedfiles);     // has to be all at once so it the previous is not overwritten

        // since there's more than one file, not okay to do a regular, non-PDF add here
        $('#btnFileUpload').attr('disabled', true);
        $('#btnDragdrop').attr('disabled', true);
    }

    $('#btnPdfDragdrop').attr('disabled', false);
}

// output file information
function ParseFile(file) {
    Output(
        "<p style='color:blue'> <i> File Name:<strong>" + file.name +
        "</strong>" +
        ", File Size: <strong>" + (file.size / 1000).toFixed(2) +
        " kb </strong> </i></p>"
    );
}

// output files information
function ParseFiles(files) {
    var outputText = '';
    for (var i = 0; i < files.length; i++) {
        var file = files[i];
        outputText += "<p style='color:blue'> <i> File Name:<strong>" + file.name +
            "</strong>" +
            ", File Size: <strong>" + (file.size / 1000).toFixed(2) +
            " kb </strong> </i></p>"
    }
    Output(outputText);
}

// initialize
function Init() {
    var dropAreaEl = $id("dropArea");
    // is XHR2 available?
    var xhr = new XMLHttpRequest();
    if (xhr.upload) {
        dropAreaEl.addEventListener("dragover", FileDragHover, false);
        dropAreaEl.addEventListener("dragleave", FileDragleave, false);
        dropAreaEl.addEventListener("drop", FileSelectHandler, false);
    }
}





