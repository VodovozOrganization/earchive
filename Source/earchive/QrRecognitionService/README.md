# Сервис распознавания QR-кодов (WeChat QRCode)

Отдельный gRPC-сервис для распознавания QR-кодов на сканах УПД. Использует
движок **WeChat QRCode** из OpenCV (`opencv_contrib/wechat_qrcode`), который
устойчивее к шумам печати, низкому контрасту и перекосу скана, чем ZXing.

## Зачем отдельный процесс

Основное приложение `earchive` собирается под **x86 / .NET Framework 4.6.1**.
Пакет `OpenCvSharp4.runtime.win` 4.13 содержит только **x64**-сборку OpenCV,
поэтому встроить WeChat QR в процесс приложения нельзя. Сервис работает
отдельным x64-процессом, а приложение обращается к нему по gRPC (gRPC-Web,
HTTP/1.1). Штрих-коды в приложении по-прежнему распознаются локально через
ZXing и этот сервис не используют.

## Что делает сервис

Для каждой страницы последовательно пробует несколько вариантов изображения
и останавливается на первом успешном:

1. страница целиком;
2. область в правом верхнем углу (где обычно расположен QR-код УПД);
3. та же область с повышением контраста (CLAHE);
4. повороты области на −3…+3° (перекос скана);
5. повышение резкости;
6. бинаризация Оцу и увеличение ×2 (бледная печать).

На тестовом наборе из 10 сканов (8 страниц с QR-кодом) сервис распознал
все 8 QR-кодов; локальный ZXing на этих же сканах не распознал ни одного.

## Требования

- Windows x64;
- .NET 5 Runtime (5.0.x);
- Visual C++ Redistributable 2015–2022 (нужен для OpenCV).

## Целевая платформа

Сервис собирается под **net5.0**, чтобы совпадать с основным стеком. Код
использует обычный generic host (`Program` + `Startup`) и не зависит от
minimal hosting, поэтому для перехода на .NET 6/8 достаточно поменять
`<TargetFramework>` в `QrRecognitionService.csproj` — остальной код менять
не требуется.

Учтите, что .NET 5 и .NET 6 уже сняты с поддержки Microsoft. Если политика
позволяет, предпочтительнее `net8.0` (LTS); на net5 сервис работает, но
обновления безопасности для рантайма не выходят.

## Сборка

```powershell
msbuild Source\earchive\QrRecognitionService\QrRecognitionService.csproj `
  /t:Restore,Build /p:Configuration=Release
```

или

```powershell
dotnet publish Source\earchive\QrRecognitionService\QrRecognitionService.csproj `
  -c Release -r win-x64 --self-contained false -o publish\QrRecognitionService
```

Каталог `Models` (файлы `detect.prototxt`, `detect.caffemodel`, `sr.prototxt`,
`sr.caffemodel`) копируется в выходной каталог автоматически.

## Запуск

```powershell
Earchive.QrRecognition.Service.exe --urls http://0.0.0.0:5107
```

Порт по умолчанию — `5107` (задан в `appsettings.json`). Для доступа с других
машин откройте порт в брандмауэре.

Проверка:

```powershell
curl http://localhost:5107/
```

## Настройка приложения

Клиент читает параметры из таблицы `base_parameters`:

| Параметр | Назначение | Пример |
| --- | --- | --- |
| `qr_grpc_address` | Хост сервиса | `192.168.0.10` |
| `qr_grpc_port` | Порт сервиса | `5107` |
| `qr_grpc_use_tls` | Использовать HTTPS (`true`/`false`) | `false` |

Если `qr_grpc_address` или `qr_grpc_port` не заданы, приложение работает как
раньше: использует локальный ZXing. Если сервис недоступен, приложение
переключается на локальный распознаватель и повторяет попытку через минуту.

Для HTTPS настройте Kestrel-сертификат стандартными средствами ASP.NET Core
(`Kestrel:Endpoints:Https` в `appsettings.json`) и установите
`qr_grpc_use_tls = true`.

## Протокол

Контракт описан в `Source/earchive/Recognaze/Protos/earchive_qr.proto`
(сервис `earchive.qr.QrRecognition`, метод `Recognize`). На запрос передаётся
изображение страницы, в ответе — список текстов найденных QR-кодов.

## Модели и лицензия

Модели WeChat QRCode взяты из
[WeChatCV/opencv_3rdparty](https://github.com/WeChatCV/opencv_3rdparty/tree/a8b69ccc738421293254aec5ddb38bd523503252)
и распространяются по лицензии Apache 2.0 (см. `Models/LICENSE`).
