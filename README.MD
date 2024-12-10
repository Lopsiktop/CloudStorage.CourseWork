# Гайд по установке облачного хранилища на локальный компьютер

P.S. Я изменил gitignore теперь пункты 4 и 5 можно пропустить

1. Распаковать архив
2. Открыть проект в Visual Studio 2022
3. Зайти в appsettings.json проекта CloudStorage.WebApi и поменять "ConnectionStrings" настройку "SqlServer", указав вместо "Server=DESKTOP-C0LRN55;" - "Server={наименование вашего сервера}"
4. Очистить папку Cloud в проекте CloudStorage.WebApi
5. Удалить файл session.txt (если таковой имеется) по пути "CloudStorage\CloudStorage.Client\bin\Debug\net8.0-windows"
6. Запустить MS SqlServer
7. Выполнить скрипт script.sql и проверить что база пуста
8. Можно запускать проект отладкой