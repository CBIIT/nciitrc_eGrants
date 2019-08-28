
function show_cats_zone(grant_id) { 
    var el = "cats_zone_" + grant_id;
    if (document.getElementById(el).style.display == 'none') {
        ShowCatsZone = 1;
    } else ShowCatsZone = 0;
   
    //close all cats zone
    var inputs = document.getElementsByTagName("div");
    for (var i = 0; i < inputs.length; i++) {
        if (inputs[i].className == 'cats_zone') {
            inputs[i].style.display = "none";
        } else continue
    }

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
    } else {
        document.getElementById(el).style.display = "none";
        ShowCatsZone = 0;
    }

}
   
function load_category_zone(grant_id) {  
    $("div[id^=cats_zone_]").empty();
    var el = "cats_zone_" + grant_id;
    var container = document.getElementById(el);
    var create_str = "<div><span>&#0160;</span><input type='checkbox' id='all_cats_" + grant_id + "' onClick='JavaScript:select_all_cats(" + grant_id + ")'/><span>&#0160;&#0160;</span><b>Select All</b></div>";

    //get selected years
    var el = "txtYears_" + grant_id;
    if (document.getElementById("hidApplID").value != "") {
        var yars = document.getElementById("hidApplID").value;
    }else if (document.getElementById(el).value == "") {
        var yars = "All";
    } else yars = document.getElementById(el).value;

    //load category list bdepending on selected years
    $.ajax(
    {
        type: 'POST',
        url: "/Egrants/LoadCategories",
        data: { grant_id: grant_id, years: yars },  //fy: fy, mechan: mechan,
        success: function (resp) {
            //sessionStorage.year = resp;
            var jsonobj = JSON.parse(resp);
            //alert(jsonobj);       
            $.each(jsonobj, function (index, element) {
                var SplitPoint = element.indexOf(":");
                var category_id = element.substring(0, SplitPoint);
                var category_name = element.substring(SplitPoint + 1, element.length);
                create_str = create_str + "<div class='col-md-3' id='catlist_" + grant_id + "'style='font-size:small; padding:0px; margin-left:5px;'>";
                create_str = create_str + "<input type='checkbox' id='cat_" + category_id + "' name=" + category_id + " /><span>&#0160;&#0160;</span>";
                create_str = create_str + "<span style='vertical-align:central'>" + category_name + "</span></div>";
                CatsCheckBoxCount = CatsCheckBoxCount + 1;
            });

            $(create_str).appendTo(container);          
        }      
    });
}

function set_selected_cats(grant_id) {
    //alert("selected cats=" + document.getElementById("hidSelectedCats").value);

    if (document.getElementById("hidSelectedCats").value != "") {
        var selectedcats = document.getElementById("hidSelectedCats").value;
        var checkboxes = $("div#catlist_" + grant_id + ", input[type='checkbox']");

        if (selectedcats == "All" || selectedcats == "all") {
            SelectAllCats = 1;
            var el = "all_cats_" + grant_id
            document.getElementById(el).checked = true;
            //set all checkbox
            checkboxes.each(function (index) {
                $(this).prop("checked", true);
                CatsSelectedCount = CatsSelectedCount + 1;
            })
        }
        else
        {
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
                    if (document.getElementById(el)!=null) {
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
            SelectAllCats = 1;
            var el = "all_cats_" + grant_id
            document.getElementById(el).checked = true;
        }
    }
}

function select_all_cats(grant_id) {
    //alert("select all cats for " + grant_id);
    var checkboxes = $('div#catlist_' + grant_id + ', input[type="checkbox"]');
    var el = "all_cats_" + grant_id
    if (SelectAllCats==0){     
        document.getElementById(el).checked = true;
        SelectAllCats = 1;
        checkboxes.each(function (index) {
            $(this).prop("checked", true);
        })
    }else 
    {
        document.getElementById(el).checked = false;
        SelectAllCats = 0;
        checkboxes.each(function (index) {
            $(this).prop("checked", false);
        })
    }
}

//function hide_category_zone(grant_id) {
//    $("div[id^=category_zone_]").empty();
//    var el = "category_zone_" + grant_id;
//    document.getElementById(el).style.display = "none";
//}

//function hide_all_category_zone() {
//    //hide all category list zone
//    //$("div[id^=category_zone_]").empty();
//    var inputs = document.getElementsByTagName("div");
//    for (var i = 0; i < inputs.length; i++) {
//        if (inputs[i].className == 'cats_zone') {
//            inputs[i].style.display = "none";
//        } else continue
//    }
//}