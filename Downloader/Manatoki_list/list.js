console.log("---------- manatoki_list ----------");

async function get_title_list() {
    var i;
    var procinfo = [];
    var dummyinfo = [];

    console.log("get_title_list()");

    var titles_details = document.getElementsByClassName("wr-subject");
    await check_title(titles_details, dummyinfo);

    var titles = document.getElementsByClassName("post-subject");
    await check_title(titles, procinfo);

    console.log("count = " + procinfo.length);
    return procinfo;
}

async function check_title(title_list, proc_info) {
    for (i = 0; i < title_list.length; i++) {

        var linkTag = title_list[i].getElementsByTagName("a");
        if (linkTag.length == 0)
            continue;

        var href = linkTag[0].href;
        var text = linkTag[0].text.trim();

        if (text.indexOf("\n") > 0)
            text = text.substring(0, text.indexOf("\n"));
        if (text.indexOf("\r") > 0)
            text = text.substring(0, text.indexOf("\r"));

        var registered = await $Downloader$.getRegisteredUrl(href, "manatoki");
        if (registered == true) {
            try
            {
                title_list[i].style.backgroundColor = "rgba(255, 255, 100, 0.7)";
            }
            catch (ex) { }
        }
        else {
            //console.log("next> " + href);
            proc_info.push([href, text]);
        }
    }
}

async function auto_download_list() {
    var autoDownloadFlag = await $Downloader$.getAutoDownloadFlag();
    var procinfo = await get_title_list();
    console.log("자동 다운로드: " + autoDownloadFlag + ", Count: " + procinfo.length);

    if (autoDownloadFlag == true) {
        // 마지막으로 다운로드한 url을 기억
        window.sessionStorage.setItem("autoDownload", "y");
        window.sessionStorage.setItem("autoDownloadUrl", location.href);
    }
    else {
        window.sessionStorage.setItem("autoDownload", "n");
    }

    if (autoDownloadFlag == true && procinfo.length > 0) {
        await $Downloader$.startDownloadBot(procinfo, "manatoki");
    }
}

// onload
{
    var autoDownloadContinue = window.sessionStorage.getItem("autoDownload");
    if (autoDownloadContinue == "y") {
        autoDownloadUrl = window.sessionStorage.getItem("autoDownloadUrl");
        if (autoDownloadUrl != location.href) {
            // 마지막으로 다운로드한 url과 다르면 이동
            console.log("autoDownload 이동");
            setTimeout("location.href = '" + autoDownloadUrl + "'", 5 * 1000);
        }
    }

    setInterval("get_title_list()", 10 * 1000);

    // 업데이트에서만 동작
    if (location.pathname == "/page/update" || location.pathname == "/bbs/page.php") {
        auto_download_list();
    }
    else {
        get_title_list().then(function(o) { console.log(">" + o.length); });
    }

}
