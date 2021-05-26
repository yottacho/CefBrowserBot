console.log("---------- common_ui ----------");

let $Downloader$_commonui = (function() {
    let id_base = "$Downloader$_commonui_";
    let infoBarDivId = id_base + "infobar";
    let infoBarDiv = null;

    let showInfoBar = function(funcName) {
        if (infoBarDiv != null)
            return infoBarDiv;

        infoBarDiv = document.createElement("div");
        infoBarDiv.id = infoBarDivId;

        infoBarDiv.style.position = "fixed";
        //infoBarDiv.style.width = "900px";
        infoBarDiv.style.height = "32px";
        infoBarDiv.style.bottom = "0";
        infoBarDiv.style.left = "50%";
        infoBarDiv.style.transform = "translateX(-50%)";
        infoBarDiv.style.maxWidth = "100vw";

        infoBarDiv.style.zIndex = 65535;
        //infoBarDiv.style.zIndex = 2147483647;
        //infoBarDiv.style.zIndex = 16777271;

        infoBarDiv.style.boxSizing = "border-box !important";
        infoBarDiv.style.borderRadius = "16px";
        infoBarDiv.style.color = "#f0f0f0";
        infoBarDiv.style.backgroundColor = "rgba(10, 10, 10, 0.7)";
        infoBarDiv.style.float = "left";
        infoBarDiv.style.display = "inline"; // inline, table
        infoBarDiv.style.fontSize = "10pt";
        infoBarDiv.style.textDecoration = "none";
        // border-style: solid; border-width: 1px; border-colo: white;

        let html = "";
        html += "<div style=\"float: left; margin: 3px 16px 3px 16px; line-height: 26px; width: 80vw;\">";
        html +=   "<div style=\"float: left; height: 24px; \">";
        html +=     "<a href=\"javascript:;\" onclick=\"" + funcName  + "\" style=\"color: #f0f0f0;\">";
        html +=     "<div style=\"float: left; font-weight: normal; padding: 0 4px 0 0;\">분류:</div>";
        html +=     "<div style=\"float: left; font-weight: bold; color: #ffff80; width: 250px; overflow: hidden;\" id=\"" + infoBarDiv.id + "_cate\"> </div>";
        html +=     "</a>";
        html +=   "</div>";
        html +=   "<div style=\"float: left; margin: 0 4px 0 4px; height: 24px;\">";
        html +=     "<a href=\"javascript:;\" onclick=\"" + funcName  + "\" style=\"color: #f0f0f0;\">";
        html +=     "<div style=\"float: left; font-weight: normal; padding: 0 4px 0 0;\">제목:</div>";
        html +=     "<div style=\"float: left; font-weight: bold; color: #ffff80; width: 250px; overflow: hidden;\" id=\"" + infoBarDiv.id + "_title\"> </div>";
        html +=     "</a>";
        html +=   "</div>";
        html +=   "<div style=\"float: left; height: 24px;\">";
        html +=     "<div style=\"float: left; font-weight: normal; color: #bbbbbb; width: 120px;\" id=\"" + infoBarDiv.id + "_stat\"> </div>";
        html +=   "</div>";

        html +=   "<div style=\"float: left; margin: 0 4px 0 4px; height: 24px;\">";
        html +=     "<div style=\"float: left; width: 250px;\" id=\"" + infoBarDiv.id + "_message\"> </div>";
        html +=   "</div>";
        html += "</div>";

        infoBarDiv.innerHTML  = html;

        document.body.appendChild(infoBarDiv);
        return infoBarDiv;
    };

    // public attributes & methods
    return {
        // property
        infoBarDivId: infoBarDivId,
        // function
        showInfoBar: showInfoBar,
        getInfoBarCategoryObj: (function() { return document.getElementById(infoBarDivId + "_cate"); }),
        getInfoBarTitleObj:    (function() { return document.getElementById(infoBarDivId + "_title"); }),
        getInfoBarStatusObj:   (function() { return document.getElementById(infoBarDivId + "_stat"); }),
        getInfoBarMessageObj:  (function() { return document.getElementById(infoBarDivId + "_message"); }),

        setInfoBarMessageNone:    (function() { this.getInfoBarMessageObj().innerHTML = "<font color=\"#99ccff\">요청 상태: </font><strong style=\"color: #66ffff;\">이미지가 없음!</strong>"; }),
        setInfoBarMessageStart:   (function() { this.getInfoBarMessageObj().innerHTML = "<font color=\"#99ccff\">요청 상태: </font><strong style=\"color: #66ffff;\">다운로드 요청중 ...</strong>"; }),
        setInfoBarMessageSuccess: (function() { this.getInfoBarMessageObj().innerHTML = "<font color=\"#99ccff\">요청 상태: </font><strong style=\"color: #00ff00;\">다운로드 성공!</strong>"; }),
        setInfoBarMessageError:   (function() { this.getInfoBarMessageObj().innerHTML = "<font color=\"#99ccff\">요청 상태: </font><strong style=\"color: #ff0000;\">다운로드 오류!</strong>"; }),
    };
}());

/*
    $Downloader$_commonui.showInfoBar("download_images()");
    $Downloader$_commonui.getInfoBarCategoryObj().innerHTML = "Title 1";
    $Downloader$_commonui.getInfoBarTitleObj().innerHTML = "Title 2";
    $Downloader$_commonui.getInfoBarStatusObj().innerHTML = "Message";

    $Downloader$_commonui.getInfoBarMessageObj().innerHTML = "Status";
*/
