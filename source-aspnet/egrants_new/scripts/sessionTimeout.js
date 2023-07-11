
var sessServerAliveTime = 10 * 60 * 2; // 1200 seconds
var sessionTimeout = 1 * 60000;  // 60 seconds - The amount of time that the countdown should count.
var sessLastActivity;
var idleTimer, remainingTimer;
var isTimout = false;

var sess_intervalID, idleIntervalID;
var sess_lastActivity;
var timer;
var isIdleTimerOn = false;
localStorage.setItem('sessionSlide', 'isStarted');

function sessPingServer() {
//   if (!isTimout) {
//        $.ajax({
//            url: "~/Egrants/SessionTimeout",
//            dataType: "json",
//            async: false,
//            type: "POST"  
//        });

        return true;
    }
//}

function sessServerAlive() {
    sess_intervalID = setInterval('sessPingServer()', sessServerAliveTime);
}

function initSessionMonitor() {
    $(document).bind('keypress.session', function (ed, e) {
        sessKeyPressed(ed, e);
    });
    $(document).bind('mousedown keydown', function (ed, e) {

        sessKeyPressed(ed, e);
    });
    sessServerAlive();
    startIdleTime();
}



$(window).scroll(function (e) {
    console.log('in scolling');
    localStorage.setItem('sessionSlide', 'isStarted');
    startIdleTime();
});

function sessKeyPressed(ed, e) {
    console.log('in sessKeyPressed');
    var target = ed ? ed.target : window.event.srcElement;
    var sessionTarget = $(target).parents("#session-expire-warning-modal").length;

    if (sessionTarget != null && sessionTarget != undefined) {
        if (ed.target.id != "btnSessionExpiredCancelled" && ed.target.id != "btnSessionModal" && ed.currentTarget.activeElement.id != "session-expire-warning-modal" && ed.target.id != "btnExpiredOk"
             && ed.currentTarget.activeElement.className != "modal fade modal-overflow in" && ed.currentTarget.activeElement.className != 'modal-header'
            && sessionTarget != 1 && ed.target.id != "session-expire-warning-modal") {
            console.log('met big condition');
            localStorage.setItem('sessionSlide', 'isStarted');
            startIdleTime();
        }
    }
}

function startIdleTime() {
    console.log('in startIdleTime');
    stopIdleTime();
    console.log('ran stop idle time');
    console.log('setting idle counter to: ' + $.now());
    localStorage.setItem("sessIdleTimeCounter", $.now());
    console.log('setting idleIntervalID checkIdleTimeout : 1000');
    idleIntervalID = setInterval('checkIdleTimeout()', 1000);
    isIdleTimerOn = false;
}

// var sessionExpired = document.getElementById("session-expired-modal");

//function sessionExpiredClicked(evt) {
//    window.location = "Logout.html";
//}





//sessionExpired.addEventListener("click", sessionExpiredClicked, false);


function stopIdleTime() {
    console.log('in stopIdleTimeout');
    console.log('clearInterval: idleIntervalID');
    console.log('clearInterval: remainingTimer');
    clearInterval(idleIntervalID);
    clearInterval(remainingTimer);
}

function checkIdleTimeout() {
    console.log('in checkIdleTimeout');
     // $('#sessionValue').val() * 60000;

    var idleTime = (parseInt(localStorage.getItem('sessIdleTimeCounter')) + (sessionTimeout)); 
    console.log('idleTime = ' + idleTime);
    console.log('if condition = ' + $.now() + ' > ' + idleTime + 60000);

    // check that now is > idleTime plus 60 seconds
    if ($.now() > idleTime + 60000) {
      //  $("#session-expire-warning-modal").modal('hide');
     //   $("#session-expired-modal").modal('show');
        console.log('clearInterval sess_intervalID = ' + sess_intervalID);
        console.log('clearInterval idleIntervalID = ' + idleIntervalID);
        clearInterval(sess_intervalID);
        clearInterval(idleIntervalID);

        $('.modal-backdrop').css("z-index", parseInt($('.modal-backdrop').css('z-index')) + 100);
       // $("#session-expired-modal").css('z-index', 2000);
        $('#btnExpiredOk').css('background-color', '#428bca');
        $('#btnExpiredOk').css('color', '#fff');

        isTimout = true;


        // at the end of the timeout countdown, log the user out
        sessLogOut();

    }
    else if ($.now() > idleTime) {
        console.log('else if condition = ' + $.now() + ' > ' + idleTime);
        ////var isDialogOpen = $("#session-expire-warning-modal").is(":visible");
        if (!isIdleTimerOn) {
            ////alert('Reached idle');
            localStorage.setItem('sessionSlide', false);
            countdownDisplay();

            $('.modal-backdrop').css("z-index", parseInt($('.modal-backdrop').css('z-index')) + 500);
            $('#session-expire-warning-modal').css('z-index', 1500);
            $('#btnOk').css('background-color', '#428bca');
            $('#btnOk').css('color', '#fff');
            $('#btnSessionExpiredCancelled').css('background-color', '#428bca');
            $('#btnSessionExpiredCancelled').css('color', '#fff');
            $('#btnLogoutNow').css('background-color', '#428bca');
            $('#btnLogoutNow').css('color', '#fff');

            $("#seconds-timer").empty();
            $("#session-expire-warning-modal").modal('show');


            isIdleTimerOn = true;
        }
    }
}

$("#btnSessionExpiredCancelled").click(function () {
    console.log('btnSessionExpiredCancelled clicked');
    $('.modal-backdrop').css("z-index", parseInt($('.modal-backdrop').css('z-index')) - 500);
});

$("#btnOk").click(function () {
    $("#session-expire-warning-modal").modal('hide');
    $('.modal-backdrop').css("z-index", parseInt($('.modal-backdrop').css('z-index')) - 500);
    startIdleTime();
    clearInterval(remainingTimer);
    localStorage.setItem('sessionSlide', 'isStarted');
});

$("#btnLogoutNow").click(function () {
    localStorage.setItem('sessionSlide', 'loggedOut');
    window.location = "Logout.html";
    sessLogOut();
 //   $("#session-expired-modal").modal('hide');

});
//$('#session-expired-modal').on('shown.bs.modal', function () {
//    $("#session-expire-warning-modal").modal('hide');
//    $(this).before($('.modal-backdrop'));
//    $(this).css("z-index", parseInt($('.modal-backdrop').css('z-index')) + 1);
//});

//$("#session-expired-modal").on("hidden.bs.modal", function () {
//    window.location = "Logout.html";
//});
$('#session-expire-warning-modal').on('shown.bs.modal', function () {
    $("#session-expire-warning-modal").modal('show');
    $(this).before($('.modal-backdrop'));
    $(this).css("z-index", parseInt($('.modal-backdrop').css('z-index')) + 1);
});


function countdownDisplay() {

    var dialogDisplaySeconds = 60;

    remainingTimer = setInterval(function () {
        if (localStorage.getItem('sessionSlide') == "isStarted") {
            $("#session-expire-warning-modal").modal('hide');
            startIdleTime();
            clearInterval(remainingTimer);
        }
       // else if (localStorage.getItem('sessionSlide') == "loggedOut") {         
       //     $("#session-expire-warning-modal").modal('hide');
           // $("#session-expired-modal").modal('show');
      //  }
        else {

            $('#seconds-timer').html(dialogDisplaySeconds);
            dialogDisplaySeconds -= 1;
        }
    }
    , 1000);
};

function sessLogOut() {
   // $.ajax({
   //     url: 'Logout.html',
   //     dataType: "json",
  //      async: false,
  //      type: "POST"
 //   });
	
	//window.location = "Logout.html";

}
