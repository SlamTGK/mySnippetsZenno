﻿Random rand = new Random();

if (project.Variables["Source_language"].Value.Substring(0,2) == "US") project.Variables["Source_language"].Value = "en-US";
else if (project.Variables["Source_language"].Value.Substring(0,2) == "UK") project.Variables["Source_language"].Value = "en-GB";
else if (project.Variables["Source_language"].Value.Substring(0,2) == "PT") project.Variables["Source_language"].Value = "pt-PT";
else if (project.Variables["Source_language"].Value.Substring(0,2) == "BR") project.Variables["Source_language"].Value = "pt-BR";
else project.Variables["Source_language"].Value = project.Variables["Source_language"].Value.Substring(0,2);

if (project.Variables["Target_language"].Value.Substring(0,2) == "US") project.Variables["Target_language"].Value = "en-US";
else if (project.Variables["Target_language"].Value.Substring(0,2) == "UK") project.Variables["Target_language"].Value = "en-GB";
else if (project.Variables["Target_language"].Value.Substring(0,2) == "PT") project.Variables["Target_language"].Value = "pt-PT";
else if (project.Variables["Target_language"].Value.Substring(0,2) == "BR") project.Variables["Target_language"].Value = "pt-BR";
else project.Variables["Target_language"].Value = project.Variables["Target_language"].Value.Substring(0,2);

project.SendInfoToLog("Начали работу. Всего строк: " + project.Lists["Ваш текст"].Count.ToString() + ". Всего прокси: " + project.Lists["Прокси"].Count.ToString() + ".", true);

if (project.Lists["Прокси"].Count != 0)
{
	lock (SyncObjects.ListSyncer)
	{
		project.SendInfoToLog("Меняем прокси", true);
		project.Variables["Proxy"].Value = project.Lists["Прокси"][0].Trim();
		project.Lists["Прокси"].RemoveAt(0);
	}
}

for (int i = 0; i < project.Lists["Ваш текст"].Count; i++)
{
	int mark = 0;
	for (int j = 0; j < Convert.ToInt32(project.Variables["Counter_Global_Error"].Value); j++)
	{
		if (mark != 0) break;

		for (int k = 0; k < Convert.ToInt32(project.Variables["Counter_Error"].Value); k++)
		{
			if (mark != 0) break;
			var secret_id = ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString();
			var number = rand.Next(10000000, 99999999).ToString();
			var response = ZennoPoster.HttpPost("https://www2.deepl.com/jsonrpc", "{\"jsonrpc\":\"2.0\",\"method\": \"LMT_handle_jobs\",\"params\":{\"jobs\":[{\"kind\":\"default\",\"raw_en_sentence\":\"" + project.Lists["Ваш текст"][i] + "\",\"raw_en_context_before\":[],\"raw_en_context_after\":[],\"preferred_num_beams\":1}],\"lang\":{\"user_preferred_langs\":[\"" + project.Variables["Source_language"].Value + "\",\"" + project.Variables["Target_language"].Value + "\"],\"source_lang_computed\":\"" + project.Variables["Source_language"].Value + "\",\"target_lang\":\"" + project.Variables["Target_language"].Value + "\"},\"priority\":1,\"commonJobParams\":{},\"timestamp\":" + secret_id + "},\"id\":" + number + "}", "application/json;", project.Variables["Proxy_Kind"].Value + "://" + project.Variables["Proxy"].Value, "UTF-8", ZennoLab.InterfacesLibrary.Enums.Http.ResponceType.HeaderAndBody, 5000, "", "", true, 5);
			if (!string.IsNullOrEmpty(response))
			{
				if (response.Contains(",\"message\":\"Too many requests.\"}}"))
				{
					Thread.Sleep(rand.Next(Convert.ToInt32(project.Variables["Delay_LL"].Value), Convert.ToInt32(project.Variables["Delay_HH"].Value)));
					break;
				}
				else
				{
					string message = Macros.TextProcessing.Regex(response, "(?<=\"postprocessed_sentence\":\").*?(?=\")", "0")[0].FirstOrDefault();
					project.Lists["Результат"].Add(System.Text.RegularExpressions.Regex.Unescape(message));
					mark++;
					project.SendInfoToLog("Успешно перевели строку #" + i, true);
					Thread.Sleep(rand.Next(Convert.ToInt32(project.Variables["Delay_L"].Value), Convert.ToInt32(project.Variables["Delay_H"].Value)));
					break;
				}
			}
		}

		if ((mark == 0) && (project.Lists["Прокси"].Count != 0))
		{
			lock (SyncObjects.ListSyncer)
			{
				project.SendInfoToLog("Меняем прокси", true);
				project.Variables["Proxy"].Value = project.Lists["Прокси"][0].Trim();
				project.Lists["Прокси"].RemoveAt(0);
			}
		}
		else if ((mark == 0) && (project.Lists["Прокси"].Count == 0))
		{
			project.SendErrorToLog("Кончились прокси", true);
			return null;
		}
	}

	if (mark == 0)
	{
		project.SendErrorToLog("Лимит по количеству неудач для проекта", true);
		return null;
	}
}

project.SendInfoToLog("Работа выполнена", true);

/*
ниже файл настроек xml который просто импортируйте в свой проект для применения настроек
below is the xml settings file which just import into your project to apply the settings


<InputSettings><InputSetting><Name>Язык</Name><Value></Value><OutputVariable></OutputVariable><Type>Tab</Type><Help></Help></InputSetting><InputSetting><Name>Язык вашего текста {US - Английский (Американский)|UK - Английский (Британский)|DE - Немецкий|FR - Французский|ES - Испанский|PT - Португальский|BR - Португальский (Бразильский)|IT - Итальянский|NL - Нидерландский|PL - Польский|JA - Японский|ZH - Китайский (Упрощенный)|RU - Русский}</Name><Value>US - Английский (Американский)</Value><OutputVariable>{-Variable.Source_language-}</OutputVariable><Type>DropDown</Type><Help></Help></InputSetting><InputSetting><Name>Какой язык нужен {US - Английский (Американский)|UK - Английский (Британский)|DE - Немецкий|FR - Французский|ES - Испанский|PT - Португальский|BR - Португальский (Бразильский)|IT - Итальянский|NL - Нидерландский|PL - Польский|JA - Японский|ZH - Китайский (Упрощенный)|RU - Русский}</Name><Value>RU - Русский</Value><OutputVariable>{-Variable.Target_language-}</OutputVariable><Type>DropDown</Type><Help></Help></InputSetting><InputSetting><Name>Количество неудач для IP {1|2|3|4|5|6|7|8|9}</Name><Value>1</Value><OutputVariable>{-Variable.Counter_Error-}</OutputVariable><Type>DropDown</Type><Help></Help></InputSetting><InputSetting><Name>Количество неудач для проекта {5|10|15|20|25|30|35|40|45|50}</Name><Value>50</Value><OutputVariable>{-Variable.Counter_Global_Error-}</OutputVariable><Type>DropDown</Type><Help></Help></InputSetting><InputSetting><Name>Тип прокси {http|https|socks4|socks5}</Name><Value>socks4</Value><OutputVariable>{-Variable.Proxy_Kind-}</OutputVariable><Type>DropDown</Type><Help></Help></InputSetting><InputSetting><Name>Путь к файлам</Name><Value></Value><OutputVariable></OutputVariable><Type>Tab</Type><Help></Help></InputSetting><InputSetting><Name>Путь к файлу с вашим текстом</Name><Value></Value><OutputVariable>{-Variable.Text_dir-}</OutputVariable><Type>FileName</Type><Help></Help></InputSetting><InputSetting><Name>Путь к файлу с результатом</Name><Value></Value><OutputVariable>{-Variable.Result_dir-}</OutputVariable><Type>FileName</Type><Help></Help></InputSetting><InputSetting><Name>Путь к файлу с прокси</Name><Value></Value><OutputVariable>{-Variable.Proxy_dir-}</OutputVariable><Type>FileName</Type><Help></Help></InputSetting><InputSetting><Name>Задержка</Name><Value></Value><OutputVariable></OutputVariable><Type>Tab</Type><Help></Help></InputSetting><InputSetting><Name>После каждого запроса. От (x) ms {100|250|500|750|1000}</Name><Value>250</Value><OutputVariable>{-Variable.Delay_L-}</OutputVariable><Type>DropDown</Type><Help></Help></InputSetting><InputSetting><Name>После каждого запроса. До (x) ms {1000|1250|1500|1750|2000}</Name><Value>1000</Value><OutputVariable>{-Variable.Delay_H-}</OutputVariable><Type>DropDown</Type><Help></Help></InputSetting><InputSetting><Name>После отказа в переводе. От (x) ms {1000|2500|5000|7500|10000}</Name><Value>1000</Value><OutputVariable>{-Variable.Delay_LL-}</OutputVariable><Type>DropDown</Type><Help></Help></InputSetting><InputSetting><Name>После отказа в переводе. До (x) ms {10000|12500|15000|17500|20000}</Name><Value>10000</Value><OutputVariable>{-Variable.Delay_HH-}</OutputVariable><Type>DropDown</Type><Help></Help></InputSetting></InputSettings>

*/