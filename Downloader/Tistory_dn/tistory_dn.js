console.log("---------- tistory_blog ----------");

// run keyboard event
window.onkeydown = function (event) {
    // 107: +
    // 68: D
    if (event.keyCode == 107) {
        download_images();
    }
}

async function auto_download_images() {
    // 자동다운로드가 ON이면 자동다운로드 수행...
    var autoDownloadFlag = await $Downloader$.getAutoDownloadFlag();
    console.log("자동 다운로드: " + autoDownloadFlag);

    if (autoDownloadFlag != true)
        return;

    download_images();
}

/* ***************************************************************
 * 다운로드 처리
 * *************************************************************** */
async function download_images() {
    var title = get_title();
    var img_list = get_images_list();

    if (img_list.length == 0) {
        $Downloader$_commonui.setInfoBarMessageNone();
        return;
    }

    $Downloader$_commonui.setInfoBarMessageStart();

    // 다운로드 요청
    var result = $Downloader$.downloadImages(title, img_list, location.href, navigator.userAgent);

    result.then(function (result) {
        console.log("downloadImages Success: [" + result + "]");
        $Downloader$_commonui.setInfoBarMessageSuccess();

    }).catch(function (error) {
        console.log("downloadImages Error: [" + error + "]");
        $Downloader$_commonui.setInfoBarMessageError();

        //alert("다운로드 오류입니다.\r\n[" + error + "]");
    });

};

/* ***************************************************************
 * 이미지 목록 추출
 * *************************************************************** */
function get_images_list() {
    var images_url_list = [];
    var i;

    if (images_url_list.length == 0) {
        var post_view = document.getElementsByClassName("entry-content");
        if (post_view.length > 0) {
            var post_imgs = post_view[0].getElementsByTagName("img");
            for (i = 0; i < post_imgs.length; i++) {
                if (post_imgs[i].hasAttribute("srcset")) {
                    var u = new URL(post_imgs[i].src, location.href);

                    // 이미지 주소(파라마터 없음)
                    var img_url = u.href;

                    console.log(img_url);
                    images_url_list.push(img_url);
                }
            }
        }
    }

    console.log("Image list > " + images_url_list.length);
    return images_url_list;
}

/* ***************************************************************
 * 제목 추출
 * *************************************************************** */

function get_title() {
    var i;
    var title_info = ["", ""];
    var title_name = "";
    var user_name = "";

    var meta = document.getElementsByTagName("meta");
    for (i = 0; i < meta.length; i++) {
        if (meta[i].hasAttribute("property")) {
            var propertyName = meta[i].getAttribute("property");
            if (propertyName == "og:title") {
                title_name = meta[i].getAttribute("content");
            }
            if (propertyName == "og:site_name") {
                user_name = meta[i].getAttribute("content");
            }
        }
    }

    title_name = title_name.trim();
    if (title_name == "") {
        var hdr = document.getElementsByTagName("title");
        if (hdr.length > 0) {
            title_name = hdr[0].innerText.trim();
        }
    }

    if (user_name != "") {
        title_name = "(" + user_name + ") " + title_name;
    }

    console.log("get_title() 제목: [" + title_name + "]");

    // 제목 정규화: 사용 불가능한 문자 삭제
    title_name = title_name
        .replaceAll("\n", "")
        .replaceAll("\r", "")
        .replaceAll("\\", "＼")
        .replaceAll("/", "／")
        .replaceAll(":", "：")
        .replaceAll("*", "＊")
        .replaceAll("?", "？")
        .replaceAll("\"", "＂")
        .replaceAll("<", "＜")
        .replaceAll(">", "＞")
        .replaceAll("|", "｜") // O/S 사용불가 문자열 \\, /, :, *, ?, ", <, >, |
        .replaceAll("!!", "!") // 연속된 !!는 !하나로 통합
        .replaceAll("!", "！")
        .replaceAll("~", "∼")
        //.replaceAll("~", "～")
        .replaceAll("！？", "⁉")
        //.Replace("'", "＇")
        .replaceAll("\u200b", "")  // zero with space
        .replaceAll("...", "⋯")
        .replaceAll("..", "⋯")
        .replace(/  +/g, " ");

    console.log("get_title() 제목 정규화: [" + title_name + "]");
    title_info[1] = title_name;

    if (title_info[0] == "") {
        title_info[0] = "티스토리";
    }

    console.log("get_title() **최종** 게시판명: [" + title_info[0] + "], 제목: [" + title_info[1] + "]");
    return title_info;
}


// onload
(function(){
    var title = get_title();
    var img_list = get_images_list();

    $Downloader$_commonui.showInfoBar("download_images()");
    $Downloader$_commonui.getInfoBarCategoryObj().innerHTML = title[0];
    $Downloader$_commonui.getInfoBarTitleObj().innerHTML = title[1];
    $Downloader$_commonui.getInfoBarStatusObj().innerHTML = "(이미지: " + img_list.length + "개)";

    // 이미지 다운로드 처리
    setTimeout('auto_download_images()', 1000);
})();