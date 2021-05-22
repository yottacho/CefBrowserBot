console.log("---------- naver_blog_post ----------");

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

    var msg_id = document.getElementById("$Downloader$_Message");
    msg_id.innerHTML = "<font color=\"#99ccff\">요청 상태: </font><strong style=\"color: #66ffff;\">다운로드 요청중 ...</strong>";

    if (img_list.length == 0) {
        msg_id.innerHTML = "<font color=\"#99ccff\">요청 상태: </font><strong style=\"color: #66ffff;\">이미지 없음</strong>";
        return;
    }

    // 다운로드 요청
    var result = $Downloader$.downloadImages(title, img_list, location.href, navigator.userAgent);

    result.then(function (result) {
        console.log("downloadImages Success: [" + result + "]");
        msg_id.innerHTML = "<font color=\"#99ccff\">요청 상태: </font><strong style=\"color: #00ff00;\">다운로드 성공!</strong>";

    }).catch(function (error) {
        console.log("downloadImages Error: [" + error + "]");
        msg_id.innerHTML = "<font color=\"#99ccff\">요청 상태: </font><strong style=\"color: #ff0000;\">다운로드중 오류가 발생했습니다.</strong>";
        //alert("다운로드 오류입니다.\r\n[" + error + "]");
    });

};

/* ***************************************************************
 * 이미지 목록 추출
 * *************************************************************** */
function get_images_list() {
    var images_url_list = [];
    var i;

    get_images_list_class(document.getElementsByClassName("se-image-resource"), images_url_list);   // blog se editor
    get_images_list_class(document.getElementsByClassName("se_mediaImage"), images_url_list);       // old se editor
    get_images_list_class(document.getElementsByClassName("_photoImage"), images_url_list);         // old blog

    if (images_url_list.length == 0) {
        // _postView 아래의 img 태그
        var post_view = document.getElementsByClassName("_postView");
        if (post_view.length > 0) {
            var post_imgs = post_view[0].getElementsByTagName("img");
            for (i = 0; i < post_imgs.length; i++) {
                if (post_imgs[i].hasAttribute("largesrc")) {
                    var u = new URL(post_imgs[i].src, location.href);

                    if (u.hostname == "postfiles.pstatic.net" || u.hostname == "blogthumb.pstatic.net" ||
                        u.hostname == "mblogthumb-phinf.pstatic.net") {
                        // blogfiles.naver.net => blogfiles.pstatic.net
                        u.hostname = "blogfiles.pstatic.net";
                    }

                    // 이미지 주소(파라마터 없음)
                    var img_url = u.origin + u.pathname;

                    console.log(img_url);
                    images_url_list.push(img_url);
                }
            }
        }
    }

    console.log("Image list > " + images_url_list.length);
    return images_url_list;
}

function get_images_list_class(clsName, images_url_list) {

    for (i = 0; i < clsName.length; i++) {
        if (clsName[i].tagName == "IMG") {
            var u = new URL(clsName[i].src, location.href);

            if (u.hostname == "postfiles.pstatic.net" || u.hostname == "blogthumb.pstatic.net" ||
                u.hostname == "mblogthumb-phinf.pstatic.net") {
                // blogfiles.naver.net => blogfiles.pstatic.net
                u.hostname = "blogfiles.pstatic.net";
            }

            // 이미지 주소(파라마터 없음)
            var img_url = u.origin + u.pathname;

            console.log(img_url);
            images_url_list.push(img_url);
        }
    }

}

/* ***************************************************************
 * 제목 추출
 * *************************************************************** */

var bbs_category = [
    [ "blog.naver.com", "네이버블로그" ],
    [ "m.blog.naver.com", "네이버블로그" ],
    [ "post.naver.com", "네이버포스트" ],
    [ "m.post.naver.com", "네이버포스트" ],
    [ "m.bboom.naver.com", "네이버뿜" ],
];

function get_title() {
    var i;
    var title_info = ["", ""];
    var title_name = "";
    var user_name = "";

    var p = location.hostname;
    for (i = 0; i < bbs_category.length; i++) {
        if (p.startsWith(bbs_category[i][0])) {
            title_info[0] = bbs_category[i][1];
            break;
        }
    }

    var meta = document.getElementsByTagName("meta");
    for (i = 0; i < meta.length; i++) {
        if (meta[i].hasAttribute("property")) {
            var propertyName = meta[i].getAttribute("property");
            if (propertyName == "og:title") {
                title_name = meta[i].getAttribute("content");
            }
            // "og:author": post, "naverblog:nickname": blog
            if (propertyName == "og:author" || propertyName == "naverblog:nickname") {
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
        title_info[0] = "네이버";
    }

    console.log("get_title() **최종** 게시판명: [" + title_info[0] + "], 제목: [" + title_info[1] + "]");
    return title_info;
}


// onload
{
    // naver blog
    var moreLink = document.getElementsByClassName("_getSummaryContent");
    if (moreLink.length > 0) {
        moreLink[0].click();
    }

    show_info_bar();
}

function show_info_bar() {
    // add div
    var html = "";
    var title = get_title();
    var img_list = get_images_list();

    var div = document.createElement("div");
    div.id = "$Downloader$_Ident";

    div.style.position = "fixed";
    div.style.top = (window.innerHeight - 75) + "px";
    div.style.left = "30px";
    div.style.width = "95%";
    div.style.height = "30px";

    div.style.paddingTop = "5px";
    div.style.paddingLeft = "10px";
    div.style.paddingRight = "10px";
    div.style.paddingBottom = "5px";

    div.style.backgroundColor = "rgba(10, 10, 10, 0.7)";
    div.style.color = "#f0f0f0";

    html += "<table style=\"border: 0; padding: 0; border-spacing: 0; width: 100%; \"><tr>";
    html += "<td nowrap><a href=\"javascript:download_images()\" style=\"color: #f0f0f0; font-size: 10pt;\">";
    html += "분류: <strong style=\"color:#ffff80; font-size: 10pt;\">" + title[0] + "</strong>, ";
    html += "제목: <strong style=\"color:#ffff80; font-size: 10pt;\">" + title[1] + "</strong> </a>";
    html += "<font color=\"#bbbbbb\" style=\"font-size: 10pt;\">(이미지: " + img_list.length + "개)</font></td>";
    html += "<td width=\"300\" nowrap><span id=\"$Downloader$_Message\" style=\"font-size: 10pt;\">&nbsp;</span></td>";
    html += "</tr></table>";

    div.innerHTML  = html;

    document.body.appendChild(div);
    // div add end

    // 이미지 다운로드 처리
    setTimeout('auto_download_images()', 1000);
}
