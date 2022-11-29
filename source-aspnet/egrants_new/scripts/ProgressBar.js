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
            $('#mssg').text(data.message);
            $("#mssg").css('display', 'inline');
            endtime = new Date().getTime();
            $('#dropArea').html('');
            $('#dropArea').removeClass('active-drop');
            $(".loading").css('display', 'none');
            $("#UploadFile").val("");
            $('#reload').css('visibility', 'visible');
            $(".custom-file-label").text('Choose file...');
          //  $(".custom-file-input").prop("disabled", true);
            if (data.message == 'please waiting window refresh...' && percent == 100) {
                refresh();
            }
        }
    });
}


