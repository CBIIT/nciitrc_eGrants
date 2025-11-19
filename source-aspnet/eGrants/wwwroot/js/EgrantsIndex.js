var currenturl = window.document.location.href;
var mode;

var can_qc = 0;

var ShowToggleAppls = 0; // indicate toggle appls showing or hide
var ShowToggleCats = 0; // indicate toggle cats showing or hide
var ShowToggleDownload = 0;
var SelectAllDocs = 0; // indicate select all docs to qc
var SelectAllCats = 0;

var cats_list = [];
var appls_list = [];

var SelectAllApplsCheckBox = 0;

$(document).ready(function () {
    set_default();
});

function set_default() {
    // set nci as default IC
    if (document.getElementById("hidFilterAdminCode").value != "") {
        $('#AdminCode').val(document.getElementById("hidFilterAdminCode").value);
    } else $('#AdminCode').val('CA');

    // show search form or show toggle
    var grant_id = document.getElementById("hidGrantID").value;
    var appl_id = document.getElementById("hidApplID").value;
    var selected_appls = document.getElementById("hidSelectedAppls").value;
    var selected_years = document.getElementById("hidSelectedYears").value;
    if (document.getElementById("hidPositionID").value >= 2) {
        ShowToggleDownload = 1;
    } else {
        ShowToggleDownload = 0;
    }

    // show search form but not show toggle bar
    if ((grant_id == "" && appl_id == "") || (grant_id != "" && appl_id == "")) {

        // check search str
        if (document.getElementById("txtKW")) {
            if (document.getElementById("hidSearchStr").value != "" && (document.getElementById("hidSearchStr").value).length > 2 && document.getElementById("hidSearchStr").value.substring(0, 7) != "filters") {
                document.getElementById("txtKW").value = document.getElementById("hidSearchStr").value;
            } else document.getElementById("txtKW").focus();
        }

        // load data for filters
        if (document.getElementById("hidFilterFY").value != "" && document.getElementById("hidFilterFY").value != 0) {
            document.getElementById("FiscalYear").value = document.getElementById("hidFilterFY").value;
        }

        if (document.getElementById("hidFilterMechanism").value != "") {
            document.getElementById("Mechanism").value = document.getElementById("hidFilterMechanism").value;
        }

        if (document.getElementById("hidFilterSerialNumber").value != "" && document.getElementById("hidFilterSerialNumber").value != 0) {
            document.getElementById("SerialNumber").value = document.getElementById("hidFilterSerialNumber").value;
        }

        // fill value for FiscalYear text Thanks for pushing it through. box
        if (document.getElementById("hidFilterFY").value != "") {
            document.getElementById("FiscalYear").value = document.getElementById("hidFilterFY").value;
        }

        // fill value for years textbox
        if (document.getElementById("hidSelectedYears").value != "" && document.getElementById("hidSelectedYears").value != "undefined") {
            var selectedYears = document.getElementById("hidSelectedYears").value;
            if (document.getElementById("hidGrantID").value != "" && selectedYears.length <= 3) {
                // var grant_id = document.getElementById("hidGrantID").value;
                var el = "txtYears_" + grant_id;
                document.getElementById(el).value = document.getElementById("hidSelectedYears").value;
            }
        }



//         console.log("ShowToggleDownload: " + ShowToggleDownload);
        // fill value for FiscalYear text Thanks for pushing it through. box
//         if (document.getElementById("hidDownloadForm").value == "") {
//            document.getElementById("hidDownloadForm").value = "Standard";
//         }
    }

    // show toggle bar but do not show search form ---
    if (grant_id != "" || appl_id != "") {

        // hide search bar
        var el = "search_bar_" + grant_id;
        document.getElementById(el).style.display = "none";
        ShowSearchForm = 0;

        // document.getElementById("toggle_bar").style.display = "inline";
        document.getElementById("toggle_appls").style.display = "inline";
        document.getElementById("toggle_title").innerHTML = "Grant Years";

        document.getElementById("show_search_form").style.display = "inline";
        document.getElementById("show_cats_toggle").style.display = "inline";
        //document.getElementById("show_download_toggle").style.display = "inline";
        show_toggle_appls();
    }

    // show search form and appls_toggle icon
    if (grant_id != "" && appl_id != "") // hide search_bar and show toggle bar if (document.getElementById("hidGrantID").value != "")
    {
        // hide search bar and flag zone
        var el = "search_bar_" + grant_id;
        document.getElementById(el).style.display = "none";

        // show search bar and toggle_appls icon only
        document.getElementById("toggle_appls").style.display = "inline";

        document.getElementById("show_appls_toggle").style.display = "none";
        document.getElementById("show_search_form").style.display = "inline";
        document.getElementById("show_cats_toggle").style.display = "inline";
        //document.getElementById("show_download_toggle").style.display = "inline";
    }

    // check tab data and show tab
    var el = "tab_" + document.getElementById("hidCurrentTab").value;
    if (document.getElementById(el) != null && document.getElementById(el).style.display == "none") {
        document.getElementById(el).style.display = "inline";
    }

    displayDownloadOrAuditForms();  // re-render because the category code above may interfere with rendering AFD
}

function by_str(str) {
    var url = '@Url.Action("by_str", "Egrants")?str=' + str;
    window.document.location.href = url;
}

function by_grant(grant_id, package_name, appls_list, categories, years) {
    mode = document.getElementById("hidMode").value;
    if (mode == "" || mode == undefined) {
        mode = "";
    }

    var url = 'by_grant?grantId=' + grant_id + '&package=' + package_name + '&mode=' + mode + '&applsList=' + appls_list + '&categories=' + categories + '&years=' + years;
    window.document.location.href = url;
}

function by_appl(appl_id) {
    mode = document.getElementById("hidMode").value;
    if (mode == "" || mode == undefined) {
        mode = "";
    }

    var url = 'by_appl?applId=' + appl_id + '&mode=' + mode;
    window.document.location.href = url;
}

function show_supplement(grant_id, act) {
    var url = loadSupplementBaseUrl + "?act=" + encodeURIComponent(act) + "&grantId=" + encodeURIComponent(grant_id);
    var MeddleWindow = window.open(url, act, "toolbar=0,menubar=0,location=0,status=0,width=1000,height=500,scrollbars=yes,left=80,top=100");
    MeddleWindow.focus();
}

function view_supplement(grant_id, act) {
    var url = '@Url.Action("LoadSupplement", "EgrantsDoc")?act=' + act + '&grantId=' + grant_id;
}

// after delete or store doc
function refresh_after_modify() {
    var url = currenturl;
    window.document.location.href = url;
}

// to create new document without selected grant year
function create_new() {
    var previous_url = encodeURIComponent(currenturl);
    var url = '/EgrantsDoc/doc_create_without_applid?previous_url=' + previous_url;
    window.document.location.href = url;
}

// to create new document or funding document with selected grant year
function create_new_doc(type, admincode, serialnum, appl_id) {
    var previous_url = encodeURIComponent(currenturl);
    if (type == 'doc') {
        var url = '@Url.Action("doc_create_with_applid", "EgrantsDoc")?admin_code=' + admincode + '&serial_num=' + serialnum + '&appl_id=' + appl_id + '&previous_url=' + previous_url;
    } else var url = '@Url.Action("funding_doc_default", "EgrantsFunding")?admin_code=' + admincode + '&serial_num=' + serialnum + '&appl_id=' + appl_id + '&previous_url=' + previous_url;
    window.document.location.href = url;
}

function show_rename_dialog(applId) {
    $('#renameModalId' + applId).modal({
        show: true
    });
}

function hide_rename_dialog(applId) {
    $('#renameModalId' + applId).modal('hide');
}


function getByAll(grant_id) {
    check_selected_cats(grant_id);
    by_grant(grant_id, 'All', 'All', cats_list, 'All');
}

function checkYears(event, grant_id) {

    var char = event.which || event.keyCode;
    // alert(char);
    if (char == 13) {
        var el = "txtYears_" + grant_id;
        var yrs = (document.getElementById(el).value).trim();
        // alert("yrs="+yrs);
        if (yrs == '' || yrs == 0 || isInteger(yrs) == false) {
            alert("Please insert correct number of the years to search");
            document.getElementById(el).value = "";
            document.getElementById(el).focus();
            return false;
        } else {
            getBySelected(grant_id);
        }
    } else {
        return false;
    }
}

function getBySelected(grant_id) {
    check_selected_cats(grant_id);
    // check inserted years
    var el = "txtYears_" + grant_id;

    if (document.getElementById(el).value != '') {
        var years = (document.getElementById(el).value).trim();
        if (isInteger(years) == false || years == 0) {
            alert("Please insert correct number of the years to search");
            document.getElementById(el).value = "";
            document.getElementById(el).focus();
            return false;
        } else by_grant(grant_id, 'All', 'All', cats_list, years);
    }

    if (document.getElementById(el).value == '') {
        if (document.getElementById("hidApplID").value == "") {
            by_grant(grant_id, 'All', 'All', cats_list, 'All');
        } else {
            var appl_id = document.getElementById("hidApplID").value;
            by_grant(grant_id, 'All', appl_id, cats_list, appl_id);
        }
    }
}

function check_selected_cats(grant_id) {
    // var SelectedCats = 0;
    cats_list = "";
    var el = "all_cats_" + grant_id;
    if (document.getElementById(el).checked == true) {
        cats_list = "All";
    } else {

        // create selected category_id list
        var checkboxes = $('div#catlist_' + grant_id + ', input[type="checkbox"]');

        for (var i = 0; i < checkboxes.length; i++) {
            if (checkboxes[i].checked == true) {
                cats_list = cats_list + checkboxes[i].name + ',';
            }
        }

        // create selected category_id list
        if (cats_list != "" && cats_list.indexOf(",") > 0) {
            cats_list = cats_list.substring(0, cats_list.length - 1);
        }
    }

    // take off last comma
    if (cats_list == "") {
        cats_list = "All";
    }
    // alert("cats_list="+cats_list);
}

function send_email(email_address, email_subject, pi_name) {
    var maillink = "mailto:" + email_address + "?subject=" + email_subject + ' [' + pi_name + ']';
    window.open(maillink, "_self");
}

function show_grant_with_flag(package, grantId) {
    if (package == "FDA" || package == "MS" || package == "OD" || package == "DS") {
        // show appls with flag for this grant
        var categories = "All";
        var applsList = "All";
        var url = 'by_grant?grantId=' + grantId + '&package=' + package + '&categories=' + categories + '&applsList=' + applsList + '&years=';
    } else if (package == "MPI") {
        var thisApplId = document.getElementById("hidApplID").value;
        console.log(thisApplId);
        if (!thisApplId) {
                thisApplId = "@Model.SelectedAppls";
            if (thisApplId.indexOf(",") != -1) {
                // just show multi if there's a , in there
                thisApplId = ''; // displays as Has Multi-PI Grant Year(s) below
            }
        }
        if (thisApplId){
            $('#mpiModal' + thisApplId).modal({
                show: true
            });
        }

        return;
    } else if (package == "ARRA") {
        package = "All";
        var url = '@Url.Action("by_grant", "Egrants")?grantId=' + grantId + '&package=&categories=&applsList=&years=';
    }
    // alert(url);
    // window.open(url, top);
    window.document.location.href = url;
}

// show or hide search form
function show_search_form() {
    // show search form
    document.getElementById("searchform").style.display = "inline";
    document.getElementById("show_search_form").style.display = "none";
    // hide toggle_appls at all
    document.getElementById("show_appls_toggle").style.display = "inline";
    document.getElementById("toggle_appls").style.display = "none";
    // clean toggle_cats at all
    //document.getElementById("show_download_toggle").style.display = "none";
    document.getElementById("show_cats_toggle").style.display = "none";
    document.getElementById("toggle_cats").style.display = "none";
    document.getElementById("toggle_title").innerHTML = "";
}

// This is the callback function that
// processes the Web Service return value.
function SucceededCallback(result) {
    alert(result);
}

// seach by selected appls and selected cats from toggles
function search_by_toggle() {
    // alert("toggle seach");
    // alert(ShowToggleAppls);
    var grant_id = document.getElementById("hidGrantID").value;
    // to create seleted appl list by default selected
    if (ShowToggleAppls == 0) {
        if (document.getElementById("hidSelectedAppls").value != "") {
            appls_list = document.getElementById("hidSelectedAppls").value;
        } else if (document.getElementById("hidApplID").value != "") {
            appls_list = document.getElementById("hidApplID").value;
        }
    } else {
        appls_list = appls_list;
    }

    // as default, all categories should be seleted
    if (ShowToggleCats == 0) {
        if (document.getElementById("hidSelectedCats").value != "") {
            cats_list = document.getElementById("hidSelectedCats").value;
        } else cats_list = "All";
    } else {
        cats_list = cats_list;
    }

    // alert("grant_id=" + grant_id + ", appls_list=" + appls_list + ", cats_list=" + cats_list );
    if (appls_list == "") {
        alert("Please select grant year to search");
    } else if (appls_list != "" && cats_list == "") {
        alert("Please select category to search");
    } else by_grant(grant_id, 'All', appls_list, cats_list, '');
}

function updateGrantYearName(newName, applId) {
    $.ajax(
        {
            type: 'POST',
            url: "/Egrants/NewGrantYearName",
            data: { name: newName, applId: applId },  
            success: function (resp) {
                if (resp !== "True")
                    console.log("Error on rename, failed on backend");
                document.getElementById('yearName' + applId).innerHTML = newName;
                hide_rename_dialog(applId);

                // if empty name, hide the delete button and change name to Add
                if (!newName) {  // empty
                    document.getElementById('deleteGrantYearName' + applId).style.visibility = 'hidden';
                    document.getElementById('newGrantYearName' + applId).value = newName;    // user might not have erased it, so manually do that here
                    document.getElementById('renameDialogName' + applId).innerHTML = "Add Request Name";
                    document.getElementById('openGrantYearNameDialogButton' + applId).innerHTML = "Add Request Name";
                } else {
                    document.getElementById('deleteGrantYearName' + applId).style.visibility = 'visible';
                    document.getElementById('newGrantYearName' + applId).value = newName;    // user might not have erased it, so manually do that here
                    document.getElementById('renameDialogName' + applId).innerHTML = "Edit Request Name";
                    document.getElementById('openGrantYearNameDialogButton' + applId).innerHTML = "Edit Request Name";
                }
            }
        });
}

function cancelGrantYearNameRename(applId) {
    document.getElementById('newGrantYearName' + applId).value = document.getElementById('yearName' + applId).innerHTML;
}
