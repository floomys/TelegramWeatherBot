using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TgWeatherBot
{
    internal class Program
    {
        static TelegramBotClient bot;
        static string token;
        static int temperature, pressure, feelsLike;

        static void Main(string[] args)
        {
            Console.Write("Введите токен: ");
            token = Console.ReadLine();
            bot = new TelegramBotClient(token);
            bot.StartReceiving(Update, Error);
            Console.WriteLine("Бот запущен\n");
            Console.WriteLine("Чтобы остановить бота, нажмите Enter");
            Console.ReadLine();
            Console.WriteLine("Бот остановлен");
        }

        private static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw arg2;
        }

        async private static Task Update(ITelegramBotClient arg1, Update arg2, CancellationToken arg3)
        {
            if (arg2.Type == UpdateType.Message)
            {
                if (arg2.Message is not null)
                {
                    var message = arg2.Message;
                    CheckMessage(message);
                }
            }
        }

        private static async void CheckMessage(Message msg)
        {
            if (msg.Type is MessageType.Text)
            {
                switch (msg.Text)
                {
                    case "/start":
                        System.IO.File.WriteAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\users.txt", $"Пользователь @{msg.Chat.Username} запустил бота\n");
                        await bot.SendTextMessageAsync(msg.Chat.Id, "Привет!\nОтправь мне свою геолокацию, а я отправлю погоду");
                        break;
                    case "test":
                        
                        break;
                    default:
                        await bot.SendTextMessageAsync(msg.Chat.Id, "Извини, но я тебя не понимаю =(");
                        break;
                }   
            }
            else if (msg.Type is MessageType.Location)
            {
                await GetWeather(msg.Location.Longitude, msg.Location.Latitude);
                await bot.SendTextMessageAsync(msg.Chat.Id,
                    $"Погода в вашем регионе:\n\nТемпература воздуха: {temperature}\nОщущается как: {feelsLike}\nДавление мм.рт.ст: {pressure}");
            }
        }

        private async static Task GetWeather(double longitude, double latitude)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Yandex-API-Key", "02a58a4b-2685-44e9-a92a-77d0c3c9d577");
                var response = await client.GetStringAsync($"https://api.weather.yandex.ru/v2/forecast?lat={latitude.ToString().Replace(',', '.')}&lon={longitude.ToString().Replace(',', '.')}&lang=ru_RU");
                var objects = JObject.Parse(response);

                foreach (KeyValuePair<string, JToken> item in objects)
                {
                    if (item.Key != "fact")
                        continue;
                    temperature = item.Value.Value<int>("temp");
                    pressure = item.Value.Value<int>("pressure_mm");
                    feelsLike = item.Value.Value<int>("feels_like");
                    return;
                }
            }
        }
    }
}