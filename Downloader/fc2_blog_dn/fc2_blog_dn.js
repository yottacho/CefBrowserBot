console.log("---------- fc2_blog_dn ----------");

// run keyboard event
window.onkeydown = function (event) {
    // 107: +
    // 68: D
    if (event.keyCode == 107) {
        download_images();
    }
}

/* ***************************************************************
 * 자동 다운로드 실행
 * *************************************************************** */
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

        // 리로드 잘못하면 영원히 돌게 됨... 그냥 완료처리하는게 더 나음
        //setTimeout("location.reload();", 30 * 1000);
    });
};

/* ***************************************************************
 * 이미지 목록 추출
 * *************************************************************** */
function get_images_list() {
    var images_url_list = [];
    var i;

    var content = document.getElementsByClassName("blog_entry");
    image_walk(content, images_url_list);

    console.log("인식한 이미지: " + images_url_list.length);

    return images_url_list;
}

function image_walk(node, images_list) {
    var i;
    var j;

    for (i = 0; i < node.length; i++) {
        // skip unvisable
        var c_css = window.getComputedStyle(node[i]);
        if (c_css.display == "none") {
            continue;
        }

        if (node[i].tagName == "IMG") {
            if (node[i].style.display == "none") {
                //console.log("skip >" + node[i].src);
                continue;
            }

            var f = false;
            for (j = 0; j < node[i].attributes.length; j++) {
                if (node[i].attributes[j].name.startsWith("srcset")) {
                    //console.log("IMG> " + node[i].attributes[j].name + " = " + node[i].attributes[j].value);
                    var u = new URL(node[i].attributes[j].value, location.href);
                    images_list.push(u.href);
                    f = true;
                    break;
                }
            }

            if (f != true) {
                //console.log("IMG> src=" + node[i].src);

                var u = new URL(node[i].src, location.href);
                images_list.push(u.href);
            }
        }

        // child node
        if (node[i].children.length > 0) {
            image_walk(node[i].children, images_list);
        }
    }
}

/* ***************************************************************
 * 제목 추출
 * *************************************************************** */
function get_title() {
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
            if (propertyName == "author") {
                user_name = meta[i].getAttribute("content");
            }
        }
    }

    title_name = title_name.trim();
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
        title_info[0] = "FC2블로그";
    }

    console.log("get_title() **최종** 제목: [" + title_info[0] + "], 회차: [" + title_info[1] + "]");
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
