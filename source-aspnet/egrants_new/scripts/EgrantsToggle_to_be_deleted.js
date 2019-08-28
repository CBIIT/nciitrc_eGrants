
var ShowToggleAppls = 0;        //indicate toggle appls showing or hide
var ShowToggleCats = 0;         //indicate toggle cats showing or hide

//show or hide search form
function show_search_form() {
    //show search form 
    document.getElementById("searchform").style.display = "inline";
    document.getElementById("show_search_form").style.display = "none";
    //hide toggle_appls at all
    document.getElementById("show_appls_toggle").style.display = "inline";
    document.getElementById("toggle_appls").style.display = "none";
    //clean toggle_cats at all 
    document.getElementById("show_cats_toggle").style.display = "none";
    //$('#toggle_cats').empty();
    //$('#cats_column_1').empty();
    //$('#cats_column_2').empty();
    //$('#cats_column_3').empty();
    //document.getElementById("all_toggle_cats").checked = false;
    document.getElementById("toggle_cats").style.display = "none";
    document.getElementById("toggle_title").innerHTML = "";
}

function before_show_toggle_cats() {
    if (ShowToggleAppls == 0 && document.getElementById("hidSelectedAppls").value == "" && document.getElementById("hidApplID").value == "") {
        alert("Please select grant year");
        return false;
    } else if (ShowToggleAppls == 1 && appls_list == "") {
        alert("Please select grant year to load categories");
        return false;
    } else show_toggle_cats();
    return true;
}
//-----------------------------------------------------------------------------
//seach by selected appls and selected cats from toggles
function search_by_toggle() {
    //alert("toggle seach");
    var grant_id = document.getElementById("hidGrantID").value;
    //to create seleted appl list
    if (ShowToggleAppls == 0) {
        if (document.getElementById("hidSelectedAppls").value != "") {
            appls_list = document.getElementById("hidSelectedAppls").value;
        } else if (document.getElementById("hidApplID").value != "") {
            appls_list = document.getElementById("hidApplID").value;
        }      
    } 
    
    //as default, all categories should be seleted
    if (ShowToggleCats == 0) {
        if (document.getElementById("hidSelectedCats").value != "") {
            cats_list = document.getElementById("hidSelectedCats").value;
        } else cats_list = "All";
    }
        
    //alert("grant_id=" + grant_id + ", appls_list=" + appls_list + ", cats_list=" + cats_list );
    if (appls_list == "") {
        alert("Please select grant year to search");
    } else if (appls_list != "" && cats_list == "") {
        alert("Please select category to search");  
    }else by_grant(grant_id, 'All', appls_list, cats_list, '');
}

//-------------------------------------------------------------------------------
//click toggle appls icon  grant_id, show_mode
function show_toggle_appls() {
    //show search form icon but not show search form
    document.getElementById("show_search_form").style.display = "inline";
    document.getElementById("searchform").style.display = "none";

    //show cate_toggle icon but not show toggle_cats
    document.getElementById("show_cats_toggle").style.display = "inline";
    //$('#toggle_cats').empty();
    $('#cats_column_1').empty();
    $('#cats_column_2').empty();
    $('#cats_column_3').empty();
    document.getElementById("all_toggle_cats").checked = false;
    document.getElementById("toggle_cats").style.display = "none";

    //show toogle_appls but not show appls_toggle icon
    document.getElementById("show_appls_toggle").style.display = "none";
    document.getElementById("toggle_appls").style.display = "inline";
    document.getElementById("toggle_title").innerHTML = "Grant Years";

    document.getElementById("defualt_appls").style.display = "inline";
    document.getElementById("all_appls").style.display = "none";
    check_toggle_appls_checkbox("defualt_appls");   
}

//switch toggle appls mode
function toggle_appls_mode_switch(show_mode) {
    //alert(show_mode); 
    if (show_mode == "all_appls") {
        //show all_appls, clean and hide defualt_appls
        document.getElementById("all_appls").style.display = "inline";     
        document.getElementById("defualt_appls").style.display = "none";       
        var checkboxes = $('div#defualt_appls input[type="checkbox"]');
        checkboxes.each(function (index) {
            $(this).prop("checked", false);
        })           
    }

    if (show_mode == "defualt_appls") {
        //show defualt_appls, clean and hide all_appls
        document.getElementById("defualt_appls").style.display = "inline";       
        document.getElementById("all_appls").style.display = "none";
        var checkboxes = $('div#all_appls input[type="checkbox"]');
        checkboxes.each(function (selected_appls_checkbox) {
            $(this).prop("checked", false);
        })
    }

    check_toggle_appls_checkbox(show_mode);
}

//check checkbox if appl_id in selectedappl list
function check_toggle_appls_checkbox(show_mode) {      
    //check show appls mode
    if (show_mode == "defualt_appls") {
        var checkboxes = $('div#defualt_appls input[type="checkbox"]');
    } else {
        var checkboxes = $('div#all_appls input[type="checkbox"]');
    }

    if (ShowToggleAppls == 0) {
        if (document.getElementById("hidApplID").value != "") {
            appls_list = document.getElementById("hidApplID").value;
            set_single_appl_checkbox(appls_list, show_mode);
        }

        if (document.getElementById("hidSelectedAppls").value != "") {
            appls_list = document.getElementById("hidSelectedAppls").value;

            if (appls_list == "All" || appls_list == "all") {
                document.getElementById("toggle_appls_checkall").checked = true;
                checkboxes.each(function (index) {
                    $(this).prop("checked", true);
                })
            }
            else {
                if (appls_list.indexOf(",") >= 1) {
                    var selectedappls = appls_list;
                    var i = selectedappls.split(",").length - 1;
                    if (i > 0) {
                        while (i > 0) {
                            var comma = selectedappls.lastIndexOf(",");
                            var appl_id = selectedappls.substring(comma + 1, selectedappls.length);
                            set_single_appl_checkbox(appl_id, show_mode);
                            i = i - 1;
                            selectedappls = selectedappls.substring(0, comma);
                        }
                    }

                    if (i == 0) {
                        var appl_id = selectedappls;
                        set_single_appl_checkbox(appl_id, show_mode);
                    }
                } else set_single_appl_checkbox(appls_list, show_mode);
            }
        }
    }
    //selected checkbox by user selected and created appls_list
    else {
        if (appls_list == "All") {
            checkboxes.each(function (index) {
                $(this).prop("checked", true);
            })
        }else if (appls_list.indexOf(",") >= 1) {
            var selected_appls = appls_list;
            var i = selected_appls.split(",").length - 1;
            if (i > 0) {
                while (i > 0) {
                    var comma = selected_appls.lastIndexOf(",");
                    var appl_id = selected_appls.substring(comma + 1, selected_appls.length);
                    set_single_appl_checkbox(appl_id, show_mode);
                    i = i - 1;
                    selected_appls = selected_appls.substring(0, comma);
                }
            }
            //for last appl_id
            if (i == 0) {
                var appl_id = selected_appls;
                set_single_appl_checkbox(appl_id, show_mode);
            }
        } else set_single_appl_checkbox(appls_list, show_mode);
    }
}

//select checkbox by appl_id
function set_single_appl_checkbox(appl_id, show_mode) {
    //check show appls mode
    if (show_mode == "defualt_appls") {
        var checkboxes = $('div#defualt_appls input[type="checkbox"]');
    } else var checkboxes = $('div#all_appls input[type="checkbox"]');
    var inputCount = checkboxes.length;

    for (var i = 0; i < inputCount; i++) {
        if (checkboxes[i].id == appl_id) {
            checkboxes[i].checked = true;
        } else continue      
    }
}

//create appls list after user selected checkbox
function create_selected_appls_list(show_mode) {
    ShowToggleAppls = 1;
    appls_list = "";
    if (show_mode == "defualt_appls") {
        var checkboxes = $('div#defualt_appls input[type="checkbox"]');
    } else if (show_mode == "all_appls") {
        var checkboxes = $('div#all_appls input[type="checkbox"]');
    }

    for (var i = 0; i < checkboxes.length; i++) {
        if (checkboxes[i].checked == true && checkboxes[i].id != "toggle_appls_checkall") {
            appls_list = appls_list + checkboxes[i].id + ',';
        }
    }

    if (appls_list != "" && appls_list.indexOf(",") > 0) {
        appls_list = appls_list.substring(0, appls_list.length - 1);
    }
}

//check all checkbox after user check select all
function select_all_toggle_appls() {
    ShowToggleAppls = 1;
    //alert(document.getElementById("toggle_appls_checkall").checked);
    if (document.getElementById("defualt_appls").style.display == "inline") {      
        var checkboxes = $('div#defualt_appls input[type="checkbox"]');
    } else var checkboxes = $('div#all_appls input[type="checkbox"]');
 
    if (document.getElementById("toggle_appls_checkall").checked == false) {
        checkboxes.each(function (index) {
            $(this).prop("checked", false);
        })
        //clean appls_list
        appls_list = "";
    }
    else {
        checkboxes.each(function (index) {
            $(this).prop("checked", true);
        })
        //create appls_list
        appls_list = "All";             
    } 
}
//---------------------------------------------------------------------------------
//create toggle for categories
function show_toggle_cats() {
    //load toggle cats
    load_toggle_cats();

    //set toggle appls
    var promise1 = new Promise(function (resolve, reject) {
        setTimeout(function () {
            resolve(check_toggle_cats_checkbox());
        }, 300);
    });

    promise1.then(function (value) {
        console.log(value);
    });

    //show search icon
    document.getElementById("show_search_form").style.display = "inline";
    //show toggle appls icon and hide toggle_appls
    document.getElementById("show_appls_toggle").style.display = "inline";
    document.getElementById("toggle_appls").style.display = "none";   
    //hide toggle cats icon and show toggle_cats
    document.getElementById("show_cats_toggle").style.display = "none";
    document.getElementById("toggle_cats").style.display = "inline";
    document.getElementById("toggle_title").innerHTML = "Category";
}

//load toggle cats
function load_toggle_cats() {
    var grant_id = document.getElementById("hidGrantID").value;
    var appl_id = document.getElementById("hidApplID").value;
    //var container = document.getElementById("toggle_cats");
    //var create_str = "<div style='float:left; display:inline; width:900px;'><span>&#0160;</span><input type='checkbox' id='all_toggle_cats' onClick='JavaScript:select_all_toggle_cats()'/><span>&#0160;&#0160;</span><b>Select All</b></div><div style='float:left; display:inline; width:900px;'>";
    var cats_column_1 = document.getElementById("cats_column_1");
    var cats_column_2 = document.getElementById("cats_column_2");
    var cats_column_3 = document.getElementById("cats_column_3");
    var cats_data_1 = "";
    var cats_data_2 = "";
    var cats_data_3 = "";   

    if (appls_list != "") {
        yars = appls_list;
    } else yars = appl_id;

    $.ajax(
    {
        type: 'POST',
        url: "/Egrants/LoadCategories",
        data: { grant_id: grant_id, years: yars },  //fy: fy, mechan: mechan,
        success: function (resp) {
            //sessionStorage.year = resp;
            var jsonobj = JSON.parse(resp);

            //to check how many category in JSON
            var json_str = jsonobj.toString();
            var total_cats = json_str.split(',').length;
            if (total_cats < 3) { total_cats = 3 }; 
            var per_cats_column = Math.ceil(total_cats / 3);           
            //alert("total_cats=" + total_cats + " and per cats column=" + per_cats_column);

            //$.each(jsonobj, function (index, element) {
            //    //alert("index=" + index);
            //    var SplitPoint = element.indexOf(":");
            //    var category_id = element.substring(0, SplitPoint);
            //    var category_name = element.substring(SplitPoint + 1, element.length);
            //    create_str = create_str + "<div class='col-md-3' id='catlist_" + grant_id + "' style='font-size:small; padding:0px; margin-left:5px; display:inline;'>";
            //    create_str = create_str + "<input type='checkbox' id=" + category_id + " name=" + category_id + " onchange='JavaScript: create_selected_cats_list();'/><span>&#0160;&#0160;</span>";
            //    create_str = create_str + "<span style='vertical-align:central'>" + category_name + "</span></div>";
            //});

            //$(create_str).appendTo(container);

            //var load_search_button = "</div><div style='display:inline; float:left; width:900px; margin-left:5px;'><a href='JavaScript:search_by_toggle()' title='search by selected appls and categories'><img style='vertical-align:middle;width:30px' src='../Content/images/searchicon.png'/></a></div>";
            //$(load_search_button).appendTo(container);

            //load data vertical
            $.each(jsonobj, function (index, element) {
                //alert("index=" + index);
                var SplitPoint = element.indexOf(":");
                var category_id = element.substring(0, SplitPoint);
                var category_name = element.substring(SplitPoint + 1, element.length);
                if (index < per_cats_column) {
                    //cats_data_1 = cats_data_1 + "<div class='col-md-4' id='catlist_" + grant_id + "' style='font-size:small; padding:0px; margin-left:5px; display:inline;'></div>";
                    cats_data_1 = cats_data_1 + "<input type='checkbox' id=" + category_id + " name=" + category_id + " onchange='JavaScript: create_selected_cats_list();'/><span>&#0160;&#0160;</span>";
                    cats_data_1 = cats_data_1 + "<span style='vertical-align:central'>" + category_name + "</span><br/>";
                }
                if (index >= per_cats_column && index < (per_cats_column * 2)) {
                    //cats_data_2 = cats_data_2 + "<div class='col-md-3' id='catlist_" + grant_id + "' style='font-size:small; padding:0px; margin-left:5px; display:inline;'></div>";
                    cats_data_2 = cats_data_2 + "<input type='checkbox' id=" + category_id + " name=" + category_id + " onchange='JavaScript: create_selected_cats_list();'/><span>&#0160;&#0160;</span>";
                    cats_data_2 = cats_data_2 + "<span style='vertical-align:central'>" + category_name + "</span><br/>";
                }
                if (index >= (per_cats_column * 2) && index < total_cats) {
                    //cats_data_3 = cats_data_3 + "<div class='col-md-3' id='catlist_" + grant_id + "' style='font-size:small; padding:0px; margin-left:5px; display:inline;'></div>";
                    cats_data_3 = cats_data_3 + "<input type='checkbox' id=" + category_id + " name=" + category_id + " onchange='JavaScript: create_selected_cats_list();'/><span>&#0160;&#0160;</span>";
                    cats_data_3 = cats_data_3 + "<span style='vertical-align:central'>" + category_name + "</span><br/>";
                }
            });

            $(cats_data_1).appendTo(cats_column_1);
            $(cats_data_2).appendTo(cats_column_2);
            $(cats_data_3).appendTo(cats_column_3);       
        }
    });  
}

//check all checkbox by selecte catslist
function check_toggle_cats_checkbox() {
    var checkboxes = $('div#toggle_cats input[type="checkbox"]');

    if (ShowToggleCats == 0 && document.getElementById("hidSelectedCats").value != "") {
        if (document.getElementById("hidSelectedCats").value == "All") {
            cats_list = "All";
            //select_all           
            document.getElementById("all_toggle_cats").checked = true;
            checkboxes.each(function (index) {
                $(this).prop("checked", true);
            })
        }
        else {
            cats_list = document.getElementById("hidSelectedCats").value;
            var selectedcats = cats_list;
            var i = selectedcats.split(",").length - 1;
            //alert(i);
            if (i == 0) {
                var categroy_id = selectedcats;
                var el = categroy_id;
                document.getElementById(el).checked = true;              
            }else if (i > 0) {
                while (i > 0) {
                    comma = selectedcats.lastIndexOf(",");                   
                    var categroy_id = selectedcats.substring(comma + 1, selectedcats.length);
                    var el = categroy_id;
                    if (document.getElementById(el) != null) {
                        document.getElementById(el).checked = true;
                    }
                    i = i - 1;
                    selectedcats = selectedcats.substring(0, comma);
                }
                //get last categroy_id;         
                var categroy_id = selectedcats.substring(0, comma);
                var el = categroy_id;
                if (document.getElementById(el) != null) {
                    document.getElementById(el).checked = true;
                }
            }
        }
        //check if all checkbox has been selected
        selecte_cats_all_checkbox();        
    }

    if (ShowToggleCats == 1 && cats_list != "") {
        if (cats_list == "All") {
            //select_all           
            document.getElementById("all_toggle_cats").checked = true;
            checkboxes.each(function (index) {
                $(this).prop("checked", true);
            })
        } else {
            var selected_cats = cats_list;
            var i = selected_cats.split(",").length - 1;
            //alert(i);
            if (i == 0) {
                var categroy_id = selected_cats;
                var el = categroy_id;
                document.getElementById(el).checked = true;
            }

            if (i > 0) {
                while (i > 0) {
                    comma = selected_cats.lastIndexOf(",");
                    var categroy_id = selected_cats.substring(comma + 1, selected_cats.length);
                    var el = categroy_id;
                    if (document.getElementById(el) != null) {
                        document.getElementById(el).checked = true;
                    }
                    i = i - 1;
                    selected_cats = selected_cats.substring(0, comma);
                }
                    //get last categroy_id;         
                    var categroy_id = selected_cats.substring(0, comma);
                    var el = categroy_id;
                    document.getElementById(el).checked = true;
            }
            //check if all checkbox has been selected
            selecte_cats_all_checkbox();
        }                 
    }   
}

//create selected cats list by user selected checkbox
function create_selected_cats_list() {
    //alert("create selected cats list");
    ShowToggleCats = 1;
    cats_list = "";
    var checkboxes = $('div#toggle_cats input[type="checkbox"]');

    for (var i = 0; i < checkboxes.length; i++) {
        if (checkboxes[i].checked == true && checkboxes[i].id != "all_toggle_cats") {
            cats_list = cats_list + checkboxes[i].id + ',';
        }
    }

    if (cats_list != "" && cats_list.indexOf(",") > 0) {
        cats_list = cats_list.substring(0, cats_list.length - 1);
    }

    //alert("cats_list=" + cats_list);
    //check if all checkbox has been checked
    selecte_cats_all_checkbox();
}

//check if all checkbox have been selected 
function selecte_cats_all_checkbox() {
    var CatsSelectedCount = 0;
    var CatsCheckBoxCount = 0;

    var checkboxes = $('div#toggle_cats input[type="checkbox"]');
    var inputCount = checkboxes.length;
    var CatsCheckBoxCount = checkboxes.length - 1;

    for (var i = 0; i < inputCount; i++) {
        if (checkboxes[i].checked == true && checkboxes[i].id != "all_toggle_cats") {
            CatsSelectedCount = CatsSelectedCount + 1;
        }
    }

    //alert("CatsSelectedCount=" + CatsSelectedCount + ",CatsCheckBoxCount=" + CatsCheckBoxCount);
    if (CatsSelectedCount == CatsCheckBoxCount) {
        document.getElementById("all_toggle_cats").checked = true;
        cats_list = "All";
    }
}

//check all check box for toggle_cats after user check select_all
function select_all_toggle_cats() {
    ShowToggleCats = 1;
    //alert(document.getElementById("all_toggle_cats").checked);
    var checkboxes = $('div#toggle_cats input[type="checkbox"]');
    if (document.getElementById("all_toggle_cats").checked == false) {
        document.getElementById("all_toggle_cats").checked = false;
        checkboxes.each(function (index) {
            $(this).prop("checked", false);
        })
        //clean up cats_list
        cats_list = "";
    }
    else {
        document.getElementById("all_toggle_cats").checked = true;
        checkboxes.each(function (index) {
            $(this).prop("checked", true);
        })
        //create cats_list
        cats_list = "All";
    }   
}





