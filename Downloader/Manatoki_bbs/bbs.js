console.log("---------- manatoki_bbs ----------");

// run keyboard event
window.onkeydown = function (event) {
    // 107: +
    // 68: D
    if (event.keyCode == 107) {
        download_images();
    }
}

/* ***************************************************************
 * 다운로드 처리
 * *************************************************************** */
async function download_images() {
    var title = get_title();
    var img_list = get_images_list();

    var msg_id = document.getElementById("$Downloader$_Message");
    msg_id.innerHTML = "<font color=\"#99ccff\">요청 상태: </font><strong style=\"color: #66ffff;\">다운로드 요청중 ...</strong>";

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
    var c_idx;

/*
    var image_node = document.body.querySelectorAll("[itemprop=image]");
    for (i = 0; i < image_node.length; i++) {
        if (image_node[i].hasAttribute("content")) {
            imageUrl = image_node[i].getAttribute("content");
            images_url_list.push(imageUrl);
        }
    }
*/
    var contents = document.getElementsByClassName("view-content");
    for (c_idx = 0; c_idx < contents.length; c_idx++) {
        var content = contents[c_idx];

        var imgs = content.getElementsByTagName("img");
        for (i = 0; i < imgs.length; i++) {
            if (imgs[i].parentElement.tagName == "A") {
                var u = new URL(imgs[i].href, location.href);
                if (u.pathname == "/bbs/view_image.php") {
                    var org_file = u.searchParams.get("fn");
                    if (org_file != null) {
                        var u2 = new URL(org_file, location.href)
                        images_url_list.push(u2.href);
                    }
                    else {
                        // bbs/view_image.php 파라마터를 모르는 경우
                        alert("Error: " + u.href);
                    }
                }
                else {
                    if (imgs[i].hasAttribute("content")) {
                        imageUrl = imgs[i].getAttribute("content");
                    }
                    else {
                        imageUrl = imgs[i].src;
                    }

                    var u = new URL(imageUrl, location.href);
                    images_url_list.push(u.href);
                }
            }
            else {
                if (imgs[i].hasAttribute("content")) {
                    imageUrl = imgs[i].getAttribute("content");
                }
                else {
                    imageUrl = imgs[i].src;
                }

                var u = new URL(imageUrl, location.href);
                images_url_list.push(u.href);
            }
        }
    }

    console.log("Image list > " + images_url_list.length);
    return images_url_list;
}

/* ***************************************************************
 * 제목 추출
 * *************************************************************** */

var bbs_category = [
    [ "/mana_free", "마나토끼_자유게시판" ],
    [ "/humor", "마나토끼_유머게시판" ],
    [ "/trantype", "마나토끼_역식자게시판" ],
    [ "/origin", "마나토끼_원본게시판" ],
];

function get_title() {
    var i;
    var title_info = ["", ""];
    var title_name = "";

    var p = location.pathname;
    for (i = 0; i < bbs_category.length; i++) {
        if (p.startsWith(bbs_category[i][0])) {
            title_info[0] = bbs_category[i][1];
            break;
        }
    }

    var title_head = document.body.querySelector("[itemprop=headline]");
    if (title_head != null && title_head.hasAttribute("content")) {
        title_name = title_head.getAttribute("content");
    }

    if (title_name == "") {
        var h = document.getElementsByTagName("h1");
        for (i = 0; i < h.length; i++) {
            if (h[i].hasAttribute("content")) {
                title_name = h[i].getAttribute("content");
                break;
            }
        }

        if (title_name == "") {
            title_name = h[h.length - 1].innerText;
        }
    }

    title_name = title_name.trim();

    // user name
    var user_attr = document.body.querySelector("[itemprop=publisher]");
    if (user_attr != null && user_attr.hasAttribute("content")) {
        var user_name = user_attr.getAttribute("content");
        user_name = user_name.trim();
        // user name + title
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
        title_info[0] = "마나토끼_기타게시판";
    }

    console.log("get_title() **최종** 게시판명: [" + title_info[0] + "], 제목: [" + title_info[1] + "]");
    return title_info;
}


/* ***************************************************************
 * Lazy Load 처리
 * *************************************************************** */
function lazy_load_flush() {
    try {
        // jquery lazyload
        $('img').each(function (idx, item) { $(item).trigger('appear'); });
        //JQuery('[src]').each(function(idx,item) { $(item).trigger('appear'); });

        $(window).trigger('scroll');
    } catch (e) { }
};

// onload
{
    lazy_load_flush();

    // add div
    var html = "";
    var title = get_title();
    var img_list = get_images_list();

    var div = document.createElement("div");
    div.id = "$Downloader$_Ident";

    div.style.position = "fixed";
    div.style.top = (window.innerHeight - 35) + "px";
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
    html += "<td><a href=\"javascript:download_images()\" style=\"color: #f0f0f0; \">";
    html += "분류: <strong style=\"color:#ffff80;\">" + title[0] + "</strong>, ";
    html += "제목: <strong style=\"color:#ffff80;\">" + title[1] + "</strong> </a>";
    html += "<font color=\"#bbbbbb\">(이미지: " + img_list.length + "개)</font></td>";
    html += "<td><span id=\"$Downloader$_Message\">&nbsp;</span></td>";
    html += "</tr></table>";

    //html += "<span><a href=\"javascript:download_images()\" style=\"color: #ffffff; \">";
    //html += "분류: " + title[0] + ", 제목: " + title[1];
    //html += " (이미지:" + img_list.length + "개)";
    //html += "</a></span> &nbsp; &nbsp;<span id=\"$Downloader$_Message\">&nbsp;</span>";

    div.innerHTML  = html;

    document.body.appendChild(div);
    // div add end
}
