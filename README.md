# CefBrowserBot

Bot based on Chromium web browser.

Required latest [Microsoft Visual C++ Redistributable for Visual Studio(x64)](https://aka.ms/vs/16/release/vc_redist.x64.exe)([More info](https://support.microsoft.com/en-us/topic/the-latest-supported-visual-c-downloads-2647da03-1eea-4433-9aff-95f26a218cc0)) and [.NET 5 Desktop Runtime (Windows x64)](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-5.0.5-windows-x64-installer)([More info](https://dotnet.microsoft.com/download/dotnet/5.0)) for run.

## Usage

### Features

#### AutoReloader

AutoReloader는 열려있는 페이지를 일정 시간마다 새로 고침합니다.  
타이머는 각 탭마다 별도로 동작합니다.

- 주 타이머는 새로운 내용이 있는지 검토하기 위해 15분에 한 번 새로고침합니다. 이 타이머는 중지할 수 없습니다.
- 페이지 로드 타이머는 15초 이내에 페이지가 로드되지 않으면 일시적인 지연으로 판단하고 새로 고침을 시도합니다. 이 타이머는 버튼을 클릭하면 일시 중지합니다.

#### Downloader

Downloader는 특정 주소의 내용을 지정된 폴더에 다운로드합니다.  
자동화 처리하기 위한 API를 제공합니다.

- To do


## License

MIT License.

### License for nuget modules

3rd party modules in [Nuget](https://www.nuget.org).

타사 패키지는 해당 패키지의 라이선스를 따릅니다.

- [CefSharp.Wpf.NETCore](https://github.com/cefsharp/cefsharp): BSD
- [FontAwesome5](https://github.com/MartinTopfstedt/FontAwesome5): MIT & CC BY 4.0
- [MvvmLightStd10 & MvvmLightLibsStd10](https://github.com/lbugnion/mvvmlight): MIT
- [Newtonsoft.Json](https://www.newtonsoft.com/json): MIT
- [Syroot.Windows.IO.KnownFolders](https://gitlab.com/Syroot/KnownFolders): MIT
- [System.ServiceModel.Primitives](https://github.com/dotnet/wcf): MIT

