console.log("---------- manatoki_ad ----------");

// onload 로드
(function() {
    try {
        // hide ad-banner
        //$('.basic-banner').each(function (idx, item) { $(item).hide(); });
        //$('.board-tail-banner').each(function (idx, item) { $(item).hide(); });

        var banner = document.getElementsByClassName("basic-banner");
        hide_item(banner);

        banner = document.getElementsByClassName("basic-banner");
        hide_item(banner);

        banner = document.getElementsByClassName("board-tail-banner");
        hide_item(banner);

        banner = document.getElementById("id_mbv");
        if (banner != null)
            banner.style.display = 'none';

        banner = document.getElementById("main-banner-view");
        if (banner != null)
            banner.style.display = 'none';

        //var scr_id = $('.view-wrap').offset();
        //var scr_id = $('#viewcomment').offset();
        //$('html, body').scrollTop(scr_id.top);

    } catch (e) { }

    function hide_item(item) {
        var i;
        for (i = 0; i < item.length; i++) {
            item[i].style.display = 'none';
        }
    }


})();
