﻿//Второй способ:
project.Profile.Load(project.Directory + @"\Вспомогательные файлы\profile.zpprofile"); //загружаем профиль из файла D:\profile.zpprofile
//Метод только для загрузки профиля из файла, сохранённого при помощи
//метода Profile.Save.
//Если при сохранении профиля в файл были включены данные о прокси,
//плагинах, данные локального хранилища. При вызове метода Profile.Load
//они будут загружены автоматически