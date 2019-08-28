var selectall = 0;
var mode;

//to switch mode between QC mode and normal mode
//function switch_mode() {
//    //mode = document.getElementById("hidMode").value;
//    //document.getElementById("show_switch_mode").style.display = "inline";
//    if (mode == "qc") {
//        mode = "";
//        document.getElementById("hidMode").value = "";
//        hide_qc_mode();
//    }
//    else
//    {     
//        mode = "qc";
//        document.getElementById("hidMode").value = "qc";      
//        show_qc_mode();
//    }
//}

//to show qc mode
//function show_qc_mode() {
//    document.getElementById("switch_mode").innerHTML = "Switch to Read Only Mode";
//    document.getElementById("to_select").style.display = "inline";

//    //show all qc options
//    var inputs = document.getElementsByTagName("div");
//    for (var i = 0; i < inputs.length; i++) {
//        if (inputs[i].className == 'qc_options') {
//            inputs[i].style.display = "inline";
//        } else continue
//    }

//    //show chackbox
//    var inputs = document.getElementsByTagName("div");
//    for (var i = 0; i < inputs.length; i++) {
//        if (inputs[i].className == 'qc_checkbox') {
//            inputs[i].style.display = "inline";
//        } else continue;
//    }

//    //show minus icon
//    var inputs = document.getElementsByTagName("div");
//    for (var i = 0; i < inputs.length; i++) {
//        if (inputs[i].className == 'show_qc') {
//            inputs[i].style.display = "none";
//        } else if (inputs[i].className == 'hide_qc') {
//            inputs[i].style.display = "inline";
//        } else continue;
//    }
//}

//to hide qc mode
//function hide_qc_mode() {
//    document.getElementById("switch_mode").innerHTML = "Switch to Update Mode";
//    document.getElementById("to_select").style.display = "none";

//    //hide qc mode
//    var inputs = document.getElementsByTagName("div");
//    for (var i = 0; i < inputs.length; i++) {
//        if (inputs[i].className == 'qc_options') {
//            inputs[i].style.display = "none";
//        } else continue
//    }

//    //hide chackbox
//    var inputs = document.getElementsByTagName("div");
//    for (var i = 0; i < inputs.length; i++) {
//        if (inputs[i].className == 'qc_checkbox') {
//            inputs[i].style.display = "none";
//        } else continue
//    }

//    //show plus icon
//    var inputs = document.getElementsByTagName("div");
//    for (var i = 0; i < inputs.length; i++) {
//        if (inputs[i].className == 'show_qc') {
//            inputs[i].style.display = "inline";
//        } else if (inputs[i].className == 'hide_qc') {
//            inputs[i].style.display = "none";
//        } else continue
//    }
//}

//to count documents
function GetDocCount() {
    var DocCount = 0
    for (var i = 0; i < window.document.frmDocs.length; i++) {
        var control = window.document.frmDocs.elements[i];
        if (control.type != 'checkbox' && control.className != 'qc') {
            continue
        }else if (control.checked) {
            DocCount = DocCount + 1;
        }
    }
    return DocCount;
}

//to count how many checkbox has been checked
//function GetCheckbox() {
//    var Checkbox = 0;
//    for (var i = 0; i < window.document.frmDocs.length; i++) {
//        var control = window.document.frmDocs.elements[i];
//        if (control.type != 'checkbox') {
//            continue;
//        } else Checkbox = Checkbox + 1;
//    }
//    return Checkbox;
//}

//to create a selected document_id list
function GetDocids() {
    var DocidString = "";
    for (var i = 0; i < window.document.frmDocs.length; i++) {
        var control = window.document.frmDocs.elements[i];
        if (control.type != 'checkbox' && control.className != 'qc') {
            continue
        }else if (control.checked) {           
            DocidString = DocidString + control.value + ',';          
        }
    }

    DocidString = DocidString.substring(0, DocidString.length - 1);
    return DocidString;
}

function SelectAll() {
    if (SelectAllDocs == 0) {
        SelectAllDocs = 1;
        document.getElementById("selectall").checked = true;
        for (var i = 0; i < window.document.frmDocs.length; i++) {
            var control = window.document.frmDocs.elements[i];

            if (control.type != 'checkbox' && control.className != 'qc') {
                continue
            } else control.checked = true;
        }
    } else UnselectAll();
}

function UnselectAll() {
    SelectAllDocs = 0;
    document.getElementById("selectall").checked = false;
    for (var i = 0; i < window.document.frmDocs.length; i++) {
        var control = window.document.frmDocs.elements[i];
        if (control.type != 'checkbox' && control.className != 'qc') {
            continue
        } else control.checked = false;
    }
}

//to delete or store doc for multiple documents
function multiple_act(act) {
    var DocCount = GetDocCount();
    if (DocCount == 0) {
        alert("Please select documents you want " + act);
    }
    else {
        var docids = GetDocids();
        act = act + " all";
        doc_modify(act, docids);
    }
}

//to delete or store doc for single document
function doc_modify(act, docids) {
    //alert("act=" + act + ", docid=" + docids);
    //hide all processing icon
    var inputs = document.getElementsByTagName("div");
    for (var i = 0; i < inputs.length; i++) {
        if (inputs[i].className == 'processing') {            
            inputs[i].style.display = "none";
        } else continue
    }

    //get first document_id to display processing icon
    if (!isInteger(docids)) {
        var endComma = docids.indexOf(",");
        var first_docid = docids.substring(0, endComma);
    } else var first_docid = docids;  

    if (confirm("Are you sure that you want " + act + " selected document(s)?")) {       
        //show processing icon
        var el = "process_" + first_docid;
        //alert(el);
        if (document.getElementById(el)) {
            document.getElementById(el).style.display = "inline";
        }
      
        //modify document
        var frmdata = new FormData();
        frmdata.append("act", act);
        frmdata.append("docids", docids);

        $.ajax({
            type: 'POST',
            url: "/EgrantsDoc/doc_modify",
            data: frmdata,
            processData: false,
            contentType: false,
            success: function (data) {
                refresh_after_modify();
            }
        });
    }
}



