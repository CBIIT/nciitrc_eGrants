function docupload(frmdata, ctrAction) {
    //5-16
    var starttime;
    var endtime;
    $.ajax({
        xhr: function () {
            starttime = new Date().getTime();
            var xhr = new XMLHttpRequest();
            //$('#mssg').text("Uploading the doc. Please wait....");
            //$(".loading").css('display', 'none');
            xhr.upload.addEventListener('progress', function (e) {
                if (e.lengthComputable) {
                    console.log('Bytes Loaded: ' + e.loaded);
                    var percent = Math.round((e.loaded / e.total) * 100);

                    if (percent != 100) {
                        $(".progress").css('display', 'inline');
                        $('#progressBar').attr('aria-valuenow', percent).css('width', percent + '%').text(percent + '%');
                    }
                    else {
                        $(".progress").css('display', 'none');
                        $(".loading").css('display', 'inline');
                    }
                };
            });
            return xhr;
        },
        type: 'POST',
        url: ctrAction,
        data: frmdata,
        processData: false,
        contentType: false,
        success: function (data) {
            $(".progress").css('display', 'none');
            $('#notice').css('visibility', 'visible').attr('href', data.url);
            var processedMssg = '';
            if (data && data.message) {
                var sections = data.message.split("**#7|n3br3@k#**");
                for (var i = 0; i < sections.length; i++) {
                    var section = sections[i];
                    var color = '';
                    var header = section.includes("**#h3@d3r#**");
                    section = section.replace("**#h3@d3r#**", "");
                    if (!section?.includes("Done!")) {
                        var style = "color:rgb(255, 0, 0)";
                        if (header)
                            style += "; margin-left:-5px;margin-bottom:4px;";
                        processedMssg += '<div style="' + style + '">' + section + '</div><br style="display:block; margin-top:-9px;visibility:hidden;content:\'\';"/>';
                    } else {
                        $('#mssgGood').html(section);
                        $("#mssgGood").css('display', 'inline');
                    }                    
                }
            }
            $('#mssg').html(processedMssg);
            $("#mssg").css('display', 'inline');
            endtime = new Date().getTime();
            $('#dropArea').html('');
            $('#dropArea').removeClass('active-drop');
            $(".loading").css('display', 'none');
            $("#UploadFile").val("");
            $('#reload').css('visibility', 'visible');
         //   $(".custom-file-label").text('Choose file...');
            //$(".custom-file-input").prop("disabled", true);
        //    function resetFile() {

                $(".custom-file-label").text('Choose files...');
                var es = document.forms[1].elements;
                clearInputFile(es[0]);
          //  }

            function clearInputFile(f) {
                if (f.value) {
                    try {
                        f.value = ''; //for IE11, latest Chrome/Firefox/Opera...
                    } catch (err) {
                    }
                    if (f.value) { //for IE5 ~ IE10
                        var form = document.createElement('form'), ref = f.nextSibling;
                        form.appendChild(f);
                        form.reset();
                        ref.parentNode.insertBefore(f, ref);
                    }
                }
            }
            if (data.message == 'please waiting window refresh...' && percent == 100) {
                refresh();
            }
        }
        
    });
}


