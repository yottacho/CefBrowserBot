console.log("---------- manatoki_dn ----------");

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

    // 제목-화로 변경
    title[1] = (title[0].trim() + " " + title[1].trim()).trim();

    // 다운로드 요청
    var result = $Downloader$.downloadImages(title, img_list, location.href, navigator.userAgent);

    result.then(function (result) {
        console.log("downloadImages Success: [" + result + "]");
        $Downloader$_commonui.setInfoBarMessageSuccess();

        download_end(title);
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

    var content = document.getElementsByClassName("view-padding");
    image_walk(content, images_url_list);

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
            var f = false;
            for (j = 0; j < node[i].attributes.length; j++) {
                if (node[i].attributes[j].name.startsWith("data-")) {
                    //console.log("IMG> " + node[i].attributes[j].name + " = " + node[i].attributes[j].value);
                    var u = new URL(node[i].attributes[j].value, location.href);
                    images_list.push(u.href);

                    //console.log("IMG> " + node[i].attributes[j].value + "  " + node[i].naturalWidth + "x" + node[i].naturalHeight);
                    /*
                    원본에 장난칠 경우 화면에 보이는 이미지를 캡처해서 전달할 수 있도록 함
                    (지금은 화면에 보이는 이미지가 resize되어 있으므로 사용하지 않음)

                    node[i].crossOrigin = "Anonymous";
                    var canvas = document.createElement("canvas");
                    canvas.width = node[i].naturalWidth;
                    canvas.height = node[i].naturalHeight;

                    var ctx = canvas.getContext("2d");
                    ctx.drawImage(node[i], 0, 0);

                    var image_data = canvas.toDataURL();
                    console.log(image_data);
                    //images_list.push(image_data);
                    */
                    f = true;
                    break;
                }
            }

            if (f != true) {
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

function get_images_list2() {
    var images_url_list = [];

    //var content = document.getElementsByClassName('view-content');
    var content = document.getElementsByClassName('view-padding');
    var data_name = 'orignal';

    for (var c = 0; c < content.length; c++) {
        // data-(임의의 이름)으로 변경
        if (data_name == 'orignal') {
            // p 태그에서 임의의 이름 확인할 수 있다.
            var paragraph = content[c].getElementsByTagName('p');
            if (paragraph.length > 0 && paragraph[0].className != '') {
                data_name = paragraph[0].className;
            }
        }

        var images = content[c].getElementsByTagName('img');

        for (var i = 0; i < images.length; i++) {
            // TODO: getComputedStyle(images[0].parentElement).display
            if (images[i].parentElement.className == data_name)
                continue;

            var imageUrl = '';

            // data 태그 우선 처리
            if (typeof images[i].attributes['data-' + data_name] != 'undefined') {
                imageUrl = images[i].attributes['data-' + data_name].value;
            } else {
                if (images[i].src.indexOf('loading') >= 0)
                    continue;
                imageUrl = images[i].src;
            }

            images_url_list.push(imageUrl);
        }
    }

    console.log("Image list > " + images_url_list.length);
    return images_url_list;
}

/* ***************************************************************
 * 제목 추출
 * *************************************************************** */
function get_title() {
    var title_info = ["", ""];

    var main_title = document.getElementsByClassName("toon-title");
    if (main_title.length == 0)
        return title_info;

    var title_name;
    if (main_title[0].getAttribute("title") != null) {
        // title 속성이 있으면 title 속성의 값을 가져온다.
        title_name = main_title[0].getAttribute("title");
    }
    else {
        // title 속성이 없으면 innerText를 가져온다.
        var title_temp = main_title[0].innerText;

        var title_lb1 = title_temp.indexOf("\r");
        var title_lb2 = title_temp.indexOf("\n");
        if (title_lb1 > 0 && title_lb2 > 0) {
            title_name = title_temp.substr(0, Math.min(title_lb1, title_lb2));
        }
        else if (title_lb1 < 0 && title_lb2 < 0) {
            title_name = title_temp;
        }
        else {
            title_name = title_temp.substr(0, Math.max(title_lb1, title_lb2));
        }
    }

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
    await $Downloader$.registerUrl(location.href, "manatoki", (title[0] + " " + title[1]).trim());

    var autoNextFlag = await $Downloader$.getAutoNextJumpFlag();
    console.log("다음회차 자동이동: " + autoNextFlag);

    // 설정상 다음회차 자동넘김이 on 이고,
    // 다음 회차가 있으면 다음 회차로 넘어감
    var nextYn = false;
    if (autoNextFlag) {
        var btnNext = document.getElementsByClassName("btn-next");
        if (btnNext.length > 0) {
            nextYn = true;

            var link = btnNext[0].getElementsByTagName("a");
            if (link.length > 0) {
                console.log("다음 회차 이동 [" + link[0].href + "]");
                setTimeout("location.href = \"" + link[0].href + "\";", 3 * 1000);
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
        $('img').each(function (idx, item) { $(item).trigger('appear'); });
        //JQuery('[src]').each(function(idx,item) { $(item).trigger('appear'); });

        $(window).trigger('scroll');
    } catch (e) { }

    // 이미지 다운로드 처리
    setTimeout('auto_download_images()', 2000);
};

// onload
(function(){
    lazy_load_flush();

    var title = get_title();
    var img_list = get_images_list();

    $Downloader$_commonui.showInfoBar("download_images()");
    $Downloader$_commonui.getInfoBarCategoryObj().innerHTML = title[0];
    $Downloader$_commonui.getInfoBarTitleObj().innerHTML = title[1];
    $Downloader$_commonui.getInfoBarStatusObj().innerHTML = "(이미지: " + img_list.length + "개)";

})();

