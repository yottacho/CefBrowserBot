using CefBrowserBot.Services;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CefBrowserBot.Extensions.Downloader
{
    public class BrowserJavascriptInterface
    {
        private DownloaderExtension Settings;

        public BrowserJavascriptInterface(DownloaderExtension extensionSetting)
        {
            Settings = extensionSetting;
        }

        /// <summary>
        /// 자동 다운로드 설정여부
        /// </summary>
        /// <returns></returns>
        public bool GetAutoDownloadFlag()
        {
            return Settings.AutoDownload;
        }

        /// <summary>
        /// 다음페이지 자동이동 설정여부
        /// </summary>
        /// <returns></returns>
        public bool GetAutoNextJumpFlag()
        {
            return Settings.MoveNext;
        }

        /// <summary>
        /// 확장 스크립트
        /// </summary>
        /// <returns></returns>
        public string GetExtensionScript()
        {
            return Settings.ExtensionScript ?? "";
        }

        /// <summary>
        /// 이미지 다운로더
        /// </summary>
        /// <param name="title">["제목", "화"]</param>
        /// <param name="images">["이미지 url"]</param>
        /// <param name="referer">레퍼러</param>
        /// <param name="finishedCallback">완료시 콜백함수</param>
        public async Task<int> DownloadImages(object title, object images, string referer, string userAgent)
        {
            List<object> TitleName = title as List<object>;
            List<object> ImageList = images as List<object>;

            Collection<DownloadLog> current = new Collection<DownloadLog>();

            if (TitleName == null || ImageList == null)
            {
                throw new Exception("Argument format error!");
            }

            Debug.WriteLine($"BrowserJavascriptInterface.DownloadImages: Download start! [{TitleName[0]}:{TitleName[1]}]");

            string downloadsPath = ConfigManager.Default.Config.DownloadDirectory;

            string targetPath = Path.Combine(downloadsPath, TitleName[0].ToString().Trim(), TitleName[1].ToString().Trim());
            Debug.WriteLine($"BrowserJavascriptInterface.DownloadImages: Download dir [{targetPath}]");

            DirectoryInfo dir = new DirectoryInfo(targetPath);
            if (!dir.Exists)
                dir.Create();

            // make download list
            int index = 1;
            foreach (var imageUrl in ImageList)
            {
                string imageSource = imageUrl.ToString();

                string fileCount = string.Format("{0:d3}", index);
                string fileName = Path.Combine(targetPath, fileCount);

                DownloadLog log = new DownloadLog()
                {
                    Title = $"{TitleName[0].ToString().Trim()} {TitleName[1].ToString().Trim()}".Trim(),
                    Url = referer,
                    Agent = userAgent,
                    FileName = fileCount,
                    FullPathName = fileName,
                    SourceUrl = imageSource,
                    Status = DownloadLogStatus.Ready,
                    Message = ""
                };

                // data:image/png;base64,<data>
                if (imageSource.StartsWith("data:"))
                {
                    // drop "data:" flag
                    imageSource = imageSource.Substring(5);
                    var i1 = imageSource.IndexOf(";") + 1;
                    var i2 = imageSource.IndexOf(",", i1);

                    var mime = imageSource.Substring(0, i1 - 1);
                    var encode = imageSource.Substring(i1, i2 - i1);    // base64 only
                    var data = imageSource.Substring(i2 + 1);

                    byte[] raw = Convert.FromBase64String(data);

                    // image/gif, image/png, image/jpeg, image/bmp, image/webp
                    string extension = FileHeader.GetImageExtension(raw) ?? mime.Replace("/", "_");

                    log.FileName += extension;
                    log.FullPathName += extension;
                    log.SourceUrl = mime + ";" + encode;

                    Debug.WriteLine($"BrowserJavascriptInterface.DownloadImages: (direct) {log.FullPathName}.");

                    File.WriteAllBytes(log.FullPathName, raw);
                    log.Status = DownloadLogStatus.Ok;
                    log.Message = "Success";
                }
                else
                {
                    current.Add(log);
                }

                DispatcherHelperService.Invoke(() =>
                {
                    while (DownloadHistory.Default.ActionLog.Count > 999)
                        DownloadHistory.Default.ActionLog.RemoveAt(0);
                    DownloadHistory.Default.ActionLog.Add(log);

                    Settings.RaiseActionLogChanged();
                });
                index++;
            }

            // start download
            bool isError = false;
            index = 1;
            foreach (var item in current)
            {
                DispatcherHelperService.Invoke(() => { Settings.ViewModel.StatusMessage = $"다운로드중 ... {index:D3}/{ImageList.Count:D3}"; });
                await DownloadImage(item);

                // download status check...
                if (item.Status != DownloadLogStatus.Ok)
                    isError = true;

                index++;
            }
            Debug.WriteLine($"BrowserJavascriptInterface.DownloadImages: Download finished. Error?{isError}");

            var msg = isError ? "오류" : $"정상 {ImageList.Count}건";
            DispatcherHelperService.Invoke(() => { Settings.ViewModel.StatusMessage = $"다운로드 완료 [{msg}]"; });

            // 오류가 있으면 오류 정보 기록
            string errorPath = targetPath + ".ErrorLog";
            string errorLog = Path.Combine(errorPath, "log.txt");
            if (isError)
            {
                if (!Directory.Exists(errorPath))
                    Directory.CreateDirectory(errorPath);

                using var outs = File.OpenWrite(errorLog);
                using var writer = new StreamWriter(outs, Encoding.UTF8);

                writer.WriteLine("------------------------------------------------------------");
                writer.WriteLine($"Url : {referer}");
                writer.WriteLine("");
                foreach (var item in current)
                {
                    writer.WriteLine($"{item.SourceUrl} => {item.FileName}: {item.Status} {item.Message}");
                }
                writer.WriteLine("------------------------------------------------------------");
                writer.Flush();

                throw new Exception("다운로드 오류가 있습니다.");
            }
            else
            {
                // 이전에 오류가 발생한 적 있으면 오류 로그 삭제
                if (Directory.Exists(errorPath))
                {
                    if (File.Exists(errorLog))
                        File.Delete(errorLog);
                    Directory.Delete(errorPath);
                }
            }

            return 1;
        }

        /// <summary>
        /// 이미지 다운로드
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private async Task DownloadImage(DownloadLog item)
        {
            item.Status = DownloadLogStatus.Download;
            DispatcherHelperService.Invoke(() => { Settings.RaiseActionLogChanged(); });

            for (int retryCnt = 0; retryCnt < 3; retryCnt++)
            {
                item.Status = DownloadLogStatus.Download;
                item.Message = "";

                bool downloadFinished = false;
                using (WebClient wc = new WebClient())
                {
                    wc.Headers["Accept"] = "image/webp,image/apng,image/*,*/*;q=0.8";
                    //wc.Headers["Accept-Encoding"] = "gzip, deflate";
                    wc.Headers["Accept-Language"] = "en-US,en;q=0.9";
                    wc.Headers["Referer"] = item.Url;
                    wc.Headers["Sec-Fetch-Dest"] = "image";
                    wc.Headers["Sec-Fetch-Mode"] = "no-cors";
                    wc.Headers["Sec-Fetch-Site"] = "cross-site";
                    wc.Headers["User-Agent"] = item.Agent;

                    Debug.WriteLine($"BrowserJavascriptInterface.DownloadImage: (WebClient) {item.SourceUrl} => {item.FullPathName}.");

                    var timer = Task.Run(() =>
                    {
                        // 60 sec = 60,000 msec
                        for (int i = 0; i < 600; i++)
                        {
                            Thread.Sleep(100);
                            if (downloadFinished)
                                break;
                        }

                        if (!downloadFinished)
                        {
                            item.Status = DownloadLogStatus.Error;
                            item.Message = "Timeout";
                            wc.CancelAsync();
                        }
                    });

                    try
                    {
                        await wc.DownloadFileTaskAsync(new Uri(item.SourceUrl), item.FullPathName);
                        downloadFinished = true;

                        // 확장자 처리
                        string extension;
                        using (var fs = File.OpenRead(item.FullPathName))
                        {
                            int len = (int)Math.Min(fs.Length, 50);
                            byte[] buf = new byte[len];
                            fs.Read(buf, 0, len);
                            fs.Close();

                            extension = FileHeader.GetImageExtension(buf);
                        }

                        if (extension != null)
                        {
                            if (File.Exists(item.FullPathName + extension))
                                File.Delete(item.FullPathName + extension);
                            File.Move(item.FullPathName, item.FullPathName + extension);

                            item.FileName += extension;
                            item.FullPathName += extension;
                        }

                        await timer;
                    }
                    catch (Exception ex)
                    {
                        item.Status = DownloadLogStatus.Error;
                        item.Message = ex.Message;
                        Debug.WriteLine($"Download error {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Debug.WriteLine($"    : {ex.InnerException.Message}");
                        }
                        /*
                        var respHeader = wc.ResponseHeaders;
                        var respHeaderKey = respHeader.AllKeys;
                        Debug.WriteLine("Response Header ------------------------------");
                        foreach (var key in respHeaderKey)
                        {
                            Debug.WriteLine($"{key}: {respHeader[key]}");
                        }
                        Debug.WriteLine("------------------------------");
                        */

                        if (File.Exists(item.FullPathName))
                            File.Delete(item.FullPathName);

                        //MessageBox.Show($"Download {item.SourceUrl} to {item.FileName}.\r\n{ex.Message}", "예외 발생");
                    }
                }

                if (downloadFinished)
                    break;
                else
                {
                    Debug.WriteLine($"Error -> Retry {retryCnt}: {item.Message}");
                    try
                    {
                        Thread.Sleep(1000);
                    }
                    catch (Exception) { }
                }
            } // end retry

            if (item.Status != DownloadLogStatus.Error)
            {
                item.Status = DownloadLogStatus.Ok;
                item.Message = "Success";
            }

            DispatcherHelperService.Invoke(() => { Settings.RaiseActionLogChanged(); });
        }

        /// <summary>
        /// 등록된 URL인지 검사
        /// </summary>
        /// <param name="url"></param>
        /// <param name="siteKey"></param>
        /// <returns></returns>
        public bool GetRegisteredUrl(string url, string siteKey)
        {
            Uri uri = new Uri(url);

            var data = DownloadHistory.Default.Data.Where(x => x.Site == siteKey && x.Path == uri.AbsolutePath).FirstOrDefault();
            if (data == default)
                return false;

            return true;
        }

        /// <summary>
        /// URL 등록
        /// </summary>
        /// <param name="url"></param>
        /// <param name="siteKey"></param>
        /// <param name="name"></param>
        public void RegisterUrl(string url, string siteKey, string name)
        {
            Uri uri = new Uri(url);

            var data = DownloadHistory.Default.Data.Where(x => x.Site == siteKey && x.Path == uri.AbsolutePath).FirstOrDefault();
            if (data == default)
            {
                // save it
                DownloadedInfo dnInfo = new DownloadedInfo()
                {
                    Url = url,
                    Path = uri.AbsolutePath,
                    Site = siteKey,
                    Title = name,
                    DateTime = DateTime.Now.ToString("s")
                };

                DownloadHistory.Default.Data.Add(dnInfo);
                DownloadHistory.Default.Save();
            }
        }

        /// <summary>
        /// 봇 프로세스 시작
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="siteKey"></param>
        public void StartDownloadBot(object urls, string siteKey)
        {
            List<object> UrlList = urls as List<object>;

            Debug.WriteLine($"BrowserJavascriptInterface.StartDownloadBot: {UrlList?.Count}");

            if (UrlList == null)
                throw new Exception("Argument error!");

            Collection<string> list = new Collection<string>();
            for (int i = UrlList.Count - 1; i >= 0; i--)
            {
                var item = UrlList[i];

                List<object> sublist = item as List<object>;
                if (sublist == null)
                    continue;

                Debug.WriteLine($"BrowserJavascriptInterface.StartDownloadBot: Add {sublist[0]}");
                AutoBotBatchRunner.Default.UrlList.Add(sublist[0].ToString());
            }

            AutoBotBatchRunner.Default.StartProcess(siteKey);
        }

        /// <summary>
        /// 봇 프로세스 항목 종료시마다 처리
        /// </summary>
        /// <param name="url"></param>
        public void DownloadBotTabEnd(string url)
        {
            Debug.WriteLine($"BrowserJavascriptInterface.DownloadBotTabEnd: {url}");
            // url에 해당하는 tab 닫고 다음 처리 시작
            AutoBotBatchRunner.Default.BotFinished(url);
        }

    }
}
