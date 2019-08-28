/*
filedrag.js - HTML5 File Drag & Drop 
*/
// getElementById
var dropedfile = null;
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
    var dropedfiles = null;
    // fetch file object
   
    dropedfiles = e.target.files || e.dataTransfer.files;  
    
    if (dropedfiles.length > 1) {
        alert("Please drag and drop only one file!");
        $('#dropArea').removeClass('active-drop');
        $('#dropArea').html('Drag-drop only one file here to upload');
        $('#btnDragdrop').attr('disabled', true);
        return false;
    }

    if (dropedfiles.length = 1) {
        dropedfile = dropedfiles[0];
        var filext = dropedfile.name.split('.').pop();
        $('#dropArea').addClass('active-drop');
        // alert(filext);
        var extArr = ['pdf',  'xls', 'xlsm', 'xlsx', 'txt','doc', 'docx', 'msg'];
        if ((extArr.indexOf(filext) > -1) == false) {
            alert("The file type is not acceptable. Please upload files only with extension of 'pdf','xls','xlsx','xlsm','txt','doc','docx' or 'msg'");
            $('#dropArea').removeClass('active-drop');
            $('#dropArea').html('Drag-drop only one file here to upload');
            $('#btnDragdrop').attr('disabled', true);
            return false;
        } else var filesize = (dropedfile.size / 1000);
     
        //alert(filesize);      
        if (filesize > 1500000) {
            alert("File size too large, please send to BOB Team");
            $('#dropArea').removeClass('active-drop');           
            $('#dropArea').html('Drag-drop only one file here to upload');
            $('#btnDragdrop').attr('disabled', true);
            return false;	          
          }
    }

    ParseFile(dropedfile);
    //$('#dropArea').removeClass('active-drop');
    $('#btnDragdrop').attr('disabled', false);
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





