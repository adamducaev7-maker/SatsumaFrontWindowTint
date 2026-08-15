# Как загрузить проект в GitHub

1. Открой репозиторий `adamducaev7-maker/SatsumaFrontWindowTint`.
2. Нажми **Add file → Upload files**.
3. Распакуй этот ZIP на компьютере.
4. Перетащи ВСЕ файлы и папки из распакованной папки в окно GitHub:
   - `.github`
   - `References`
   - `SatsumaFrontWindowTint.cs`
   - `SatsumaFrontWindowTint.csproj`
   - `README.md`
5. Нажми **Commit changes**.
6. GitHub автоматически запустит **Actions → Build Satsuma Front Window Tint**.
7. Открой завершившийся workflow → внизу **Artifacts** → скачай `SatsumaFrontWindowTint`.
8. Внутри ZIP будет готовый `SatsumaFrontWindowTint.dll`.

Важно: сам ZIP этого пакета загружать в репозиторий как единственный файл НЕ надо — GitHub не распакует его для Actions. Нужно загрузить содержимое распакованной папки.
