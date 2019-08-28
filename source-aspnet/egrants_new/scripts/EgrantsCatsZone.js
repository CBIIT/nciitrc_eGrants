//--------------------------------------------------------------------------
var ShowCatsZone = 0;               //indicate cats_zone showing or hide
//var SelectAllCatsZoneCheckBox = 0;

var CatsSelectedCount = 0;
var CatsCheckBoxCount = 0;

function show_cats_zone(grant_id) {
    //alert(grant_id);
    //show selected cats zone by grant_id
    var el = "cats_zone_" + grant_id;

    //close all other cats zone
    var inputs = document.getElementsByTagName("div");
    for (var i = 0; i < inputs.length; i++) {
        if (inputs[i].className == 'cats_zone' && inputs[i].id != el ) {
            inputs[i].style.display = "none";
        } else continue
    }

    if (document.getElementById(el).style.display == 'none') {
        ShowCatsZone = 1;
    } else ShowCatsZone = 0;

    //if (document.getElementById(el).style.display == "none") {
    //    document.getElementById(el).style.display = "inline";
    //} else if (document.getElementById(el).style.display == "inline") {
    //    document.getElementById(el).style.display = "none";
    //}

    if (ShowCatsZone == 1) {
        document.getElementById(el).style.display = "inline";

        load_category_zone(grant_id);

        //set_selected_cats(grant_id);
        var promise1 = new Promise(function (resolve, reject) {
            setTimeout(function () {
                //alert("to set_selected_cats");
                resolve(set_selected_cats(grant_id));
            }, 300);
        });

        promise1.then(function (value) {
            console.log(value);
        });
    }
    //hide cats_zone
    else if (document.getElementById(el).style.display == "inline") {
        document.getElementById(el).style.display = "none";
        ShowCatsZone = 0;
    }
}

function load_category_zone(grant_id) {
    //clean all cats zone columns
    $("div[id^=cats_zone_column_] ").empty();
    var el = "all_cats_" + grant_id;
    document.getElementById(el).checked = false;

    //create new cats zone columns
    var cats_zone_column_1 = document.getElementById("cats_zone_column_1_" + grant_id);
    var cats_zone_column_2 = document.getElementById("cats_zone_column_2_" + grant_id);
    var cats_zone_column_3 = document.getElementById("cats_zone_column_3_" + grant_id);
    var cats_zone_data_1 = "";
    var cats_zone_data_2 = "";
    var cats_zone_data_3 = "";

    //get selected years
    var el = "txtYears_" + grant_id;
    if (document.getElementById("hidApplID").value != "") {
        var yars = document.getElementById("hidApplID").value;
    } else if (document.getElementById(el).value == "") {
        var yars = "All";
    } else {
        var yars = document.getElementById(el).value;
    }

    //run ajax to get data 
    $.ajax(
    {
        type: 'POST',
        url: "/Egrants/LoadCategories",
        data: { grant_id: grant_id, years: yars },  //fy: fy, mechan: mechan,
        success: function (resp) {
            //load category zone by horizontal
            //var jsonobj = JSON.parse(resp);
            //alert(jsonobj);
            //$.each(jsonobj, function (index, element) {
            //    var SplitPoint = element.indexOf(":");
            //    var category_id = element.substring(0, SplitPoint);
            //    var category_name = element.substring(SplitPoint + 1, element.length);
            //    create_str = create_str + "<div class='col-md-3' id='catlist_" + grant_id + "'style='font-size:small; padding:0px; margin-left:5px;'>";
            //    create_str = create_str + "<input type='checkbox' id='cat_" + category_id + "' name=" + category_id + " /><span>&#0160;&#0160;</span>";
            //    create_str = create_str + "<span style='vertical-align:central'>" + category_name + "</span></div>";
            //    CatsCheckBoxCount = CatsCheckBoxCount + 1;
            //});
            //$(create_str).appendTo(container);

            //to check how many category in JSON
            var jsonobj = JSON.parse(resp);
            var json_str = jsonobj.toString();
            var total_cats = json_str.split(',').length;
            CatsCheckBoxCount = total_cats;
            if (total_cats < 3) { total_cats = 3 };
            var per_cats_zone_column = Math.ceil(total_cats / 3);

            //load cats zone columns by vertical
            $.each(jsonobj, function (index, element) {
                //alert("index=" + index);
                var SplitPoint = element.indexOf(":");
                var category_id = element.substring(0, SplitPoint);
                var category_name = element.substring(SplitPoint + 1, element.length);
                if (index < per_cats_zone_column) {
                    cats_zone_data_1 = cats_zone_data_1 + "<input type='checkbox' id='cat_" + category_id + "' name=" + category_id + " /><span>&#0160;&#0160;</span>";
                    cats_zone_data_1 = cats_zone_data_1 + "<span style='vertical-align:central'>" + category_name + "</span><br/>";
                }
                if (index >= per_cats_zone_column && index < (per_cats_zone_column * 2)) {
                    cats_zone_data_2 = cats_zone_data_2 + "<input type='checkbox' id='cat_" + category_id + "' name=" + category_id + " /><span>&#0160;&#0160;</span>";
                    cats_zone_data_2 = cats_zone_data_2 + "<span style='vertical-align:central'>" + category_name + "</span><br/>";
                }
                if (index >= (per_cats_zone_column * 2) && index < total_cats) {
                    cats_zone_data_3 = cats_zone_data_3 + "<input type='checkbox' id='cat_" + category_id + "' name=" + category_id + " /><span>&#0160;&#0160;</span>";
                    cats_zone_data_3 = cats_zone_data_3 + "<span style='vertical-align:central'>" + category_name + "</span><br/>";
                }
            });

            $(cats_zone_data_1).appendTo(cats_zone_column_1);
            $(cats_zone_data_2).appendTo(cats_zone_column_2);
            $(cats_zone_data_3).appendTo(cats_zone_column_3);
        }
    });
}

function set_selected_cats(grant_id) {
    if (document.getElementById("hidSelectedCats").value != "") {
        var selectedcats = document.getElementById("hidSelectedCats").value;
        var checkboxes = $("div#catlist_" + grant_id + ", input[type='checkbox']");

        if (selectedcats == "All" || selectedcats == "all") {
            var el = "all_cats_" + grant_id
            document.getElementById(el).checked = true;
            //check all checkbox
            checkboxes.each(function (index) {
                $(this).prop("checked", true);
                CatsSelectedCount = CatsSelectedCount + 1;
            })
        }
        else {
            var i = selectedcats.split(",").length - 1;
            //alert(i);
            if (i == 0) {
                var categroy_id = selectedcats;
                var el = "cat_" + categroy_id;
                document.getElementById(el).checked = true;
                CatsSelectedCount = 1;
            }

            if (i > 0) {
                while (i > 0) {
                    comma = selectedcats.lastIndexOf(",");
                    var categroy_id = selectedcats.substring(comma + 1, selectedcats.length);
                    var el = "cat_" + categroy_id;
                    if (document.getElementById(el) != null) {
                        document.getElementById(el).checked = true;
                        CatsSelectedCount = CatsSelectedCount + 1;
                    }
                    i = i - 1;
                    selectedcats = selectedcats.substring(0, comma);
                }
                //get last categroy_id;
                var categroy_id = selectedcats.substring(0, comma);
                var el = "cat_" + categroy_id;
                document.getElementById(el).checked = true;
                CatsSelectedCount = CatsSelectedCount + 1;
            }
        }

        if (CatsSelectedCount == CatsCheckBoxCount) {         
            var el = "all_cats_" + grant_id
            document.getElementById(el).checked = true;
        }
    }
}

//check or uncheck all checkbox for cats zone
function select_all_cats_zone(grant_id) {
    var checkboxes = $("div#catlist_" + grant_id + ", input[type='checkbox']");
    var el = "all_cats_" + grant_id;
    if (document.getElementById(el).checked == true) {
        checkboxes.each(function (index) {
            $(this).prop("checked", true);
        })
    } else {
        checkboxes.each(function (index) {
            $(this).prop("checked", false);
        })
    }
}
