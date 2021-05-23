console.log("---------- jmana_dn ----------");

// run keyboard event
window.onkeydown = function (event) {
    // 107: +
    // 68: D
    if (event.keyCode == 107) {
        download_images();
    }
}

var check_completed = false;

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

    var msg_id = document.getElementById("$Downloader$_Message");
    msg_id.innerHTML = "<font color=\"#99ccff\" style=\"font-size: 10pt;\">요청 상태: </font><strong style=\"color: #66ffff; font-size: 10pt;\">다운로드 요청중 ...</strong>";

    // 제목-화로 변경
    title[1] = (title[0].trim() + " " + title[1].trim()).trim();

    // 다운로드 요청
    var result = $Downloader$.downloadImages(title, img_list, location.href, navigator.userAgent);

    result.then(function (result) {
        console.log("downloadImages Success: [" + result + "]");
        msg_id.innerHTML = "<font color=\"#99ccff\" style=\"font-size: 10pt;\">요청 상태: </font><strong style=\"color: #00ff00; font-size: 10pt;\">다운로드 성공!</strong>";

        download_end(title);
    }).catch(function (error) {
        console.log("downloadImages Error: [" + error + "]");
        msg_id.innerHTML = "<font color=\"#99ccff\" style=\"font-size: 10pt;\">요청 상태: </font><strong style=\"color: #ff0000; font-size: 10pt;\">다운로드 오류!</strong>";

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

    var content = document.getElementsByClassName("view-wrap");
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
                if (node[i].attributes[j].name.startsWith("data-")) {
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
                // 배너 삭제
                if (u.pathname.indexOf("notice_") < 0) {
                    images_list.push(u.href);
                }
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

    var content = document.getElementsByClassName("view-wrap");
    if (content.length == 0) {
        return title_info;
    }

    var main_title = content[0].getElementsByClassName("tit");
    if (main_title.length == 0)
        return title_info;

    var title_name;
    title_name = main_title[0].innerText;

    title_name = title_name.trim();
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
        .replaceAll("..", "⋯");

    console.log("get_title() 제목 정규화: [" + title_name + "]");

    // 제목과 회차정보 분리
    // 기본 패턴: 화/권
    // 변형 패턴: 번외/특별/단편/앤솔로지
    var pattern = [["번외", 0], ["특별", 0], ["단편", 0], ["앤솔로지", 1]];

    var found = false;
    var i;
    for (i = 0; i < pattern.length; i++) {
        var find_idx = title_name.lastIndexOf(pattern[i][0]);
        if (find_idx > 0) {
            if (pattern[i][1] == 0) {
                title_info[0] = title_name.substring(0, find_idx).trim();
                title_info[1] = title_name.substring(find_idx).trim();
            }
            else {
                find_idx += pattern[i][0].length;
                title_info[0] = title_name.substring(0, find_idx).trim();
                title_info[1] = title_name.substring(find_idx).trim();
            }

            found = true;
        }
    }

    if (!found) {
        // 화/권
        var regex = /^(.*?)\s+([\d~∼\-\. ]+[권화])$/;
        var result = regex.exec(title_name);
        if (result != null) {
            title_info[0] = result[1];
            title_info[1] = result[2];
            found = true;
        }
    }

    if (!found)
        title_info[0] = title_name;

    console.log("get_title() **최종** 제목: [" + title_info[0] + "], 회차: [" + title_info[1] + "]");
    return title_info;
}

/* ***************************************************************
 * 다운로드 완료 처리
 * *************************************************************** */
async function download_end(title) {
    console.log("다운로드 완료!");
    // 완료처리
    await $Downloader$.registerUrl(location.href, "jmana", (title[0] + " " + title[1]).trim());

    var autoNextFlag = await $Downloader$.getAutoNextJumpFlag();
    console.log("다음회차 자동이동: " + autoNextFlag);

    // 설정상 다음회차 자동넘김이 on 이고,
    // 다음 회차가 있으면 다음 회차로 넘어감
    var nextYn = false;
    if (autoNextFlag) {
        var btnNext = document.getElementsByClassName("btn-next");
        if (btnNext.length > 0) {
            nextYn = true;
            var link = "";

            if (btnNext[0].tagName == "A") {
                link = btnNext[0].href;
            } else {
                var tag_a = btnNext[0].getElementsByTagName("a");
                if (tag_a.length > 0) {
                    link = tag_a[0].href;
                }
            }

            if (link.length > 0) {
                console.log("다음 회차 이동 [" + link + "]");
                setTimeout("location.href = \"" + link + "\";", 3 * 1000);
            }
            else {
                console.log("다음 회차 정보가 오류임");
            }
        }
        else
            console.log("마지막 회차");
    }

    // 다음회차 이동이 없는 경우
    if (nextYn == false) {
        // 일반 종료 처리 (tab close 등)
        var autoDownloadFlag = await $Downloader$.getAutoDownloadFlag();
        console.log("자동 다운로드: " + autoDownloadFlag);

        if (autoDownloadFlag) {
            // 다음 기능... 탭 종료라던지
            $Downloader$.downloadBotTabEnd(location.href);
        }

        console.log("종료");
    }
}

/* ***************************************************************
 * Lazy Load 처리
 * *************************************************************** */
function lazy_load_flush() {
    try {
        // jquery lazyload
        jj('img').each(function (idx, item) { jj(item).trigger('appear'); });
        //JQuery('[src]').each(function(idx,item) { jj(item).trigger('appear'); });

        jj(window).trigger('scroll');
    } catch (e) {
        console.log("lazy_load_flush() error");
        console.log(e);
    }
};

// onload
var jj = jQuery.noConflict();
jj(window).ready(function(){
    lazy_load_flush();

    show_info_bar();

    // 화면이 로드된 후 이미지 show/hide 처리하고 있으므로 일정시간 기다려야 함
    var timer = 0;

    // inserted 변수 체크 (파훼된 것이 알려지면 변수명이 랜덤으로 변경될 수 있으니 유의)
    if (typeof inserted != "undefined") {
        console.log("랜덤이미지 변수: [" + inserted + "]");
        var ins_arr = inserted.split(",");
        var i;
        var m = 0;
        for (i = 0; i < ins_arr.length; i++) {
            m = Math.max(ins_arr[i] * 1, m);
        }

        if (m > 0) {
            timer = (m * 500) + 1000;
        }
    } else {
        var lis = jj(".pdf-wrap").children();
        timer = (lis.length * 500) - 1000;
    }

    if (timer <= 0)
        timer = 1000;

    console.log("로드 대기 시간> " + (timer / 1000) + "초.");
    setTimeout("check_image_count()", timer);
});

function show_info_bar() {
    // add div
    var html = "";
    var title = get_title();

    var div = document.createElement("div");
    div.id = "$Downloader$_Ident";

    div.style.position = "fixed";
    div.style.top = (window.innerHeight - 35) + "px";
    div.style.left = "30px";
    div.style.width = "85%";
    div.style.height = "30px";

    div.style.paddingTop = "5px";
    div.style.paddingLeft = "10px";
    div.style.paddingRight = "10px";
    div.style.paddingBottom = "5px";

    div.style.backgroundColor = "rgba(10, 10, 10, 0.8)";
    div.style.color = "#f0f0f0";

    var title_full = title[0] + " " + title[1];
    var title_1 = title[0];
    if (title_1.length > 40) {
        title_1 = title_1.substring(0, 40) + " ...";
    }

    html += "<table style=\"border: 1; padding: 0; border-spacing: 0; width: 100%; \"><tr>";
    html += "<td nowrap><a href=\"javascript:download_images()\" style=\"color: #f0f0f0; font-size: 10pt;\" title=\"" + title_full + "\">";
    html += "분류: <strong style=\"color:#ffff80; font-size: 10pt;\">" + title_1 + "</strong>, ";
    html += "제목: <strong style=\"color:#ffff80; font-size: 10pt;\">" + title[1] + "</strong> </a>";
    html += "<font color=\"#bbbbbb\" style=\"font-size: 9pt;\"><span id=\"$Downloader$_img\">(로드중...)</span></font></td>";
    html += "<td width=\"300\" nowrap><span id=\"$Downloader$_Message\">&nbsp;</span></td>";
    html += "</tr></table>";

    div.innerHTML  = html;

    document.body.appendChild(div);
    // div add end
}

function check_image_count() {
    var img_list = get_images_list();
    var msg_id = document.getElementById("$Downloader$_img");

    msg_id.innerHTML = "(이미지: " + img_list.length + "개)";
    check_completed = true;

    // 이미지 다운로드 처리
    setTimeout('auto_download_images()', 2000);
}
