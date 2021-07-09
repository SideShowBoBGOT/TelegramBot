using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;


using MySql.Data.MySqlClient;
using Telegram.Bot.Types.ReplyMarkups;

namespace learnBot
{
    class Program
    {
        private const string TEXT_1 = "Так";
        private static TelegramBotClient client;


        static void Main(string[] args)
        {
            client = new TelegramBotClient(Config.Token);
        
            client.StartReceiving();
            client.OnMessage += onMessageHandler;
            Console.WriteLine("[Log]: Bot started");
            Console.ReadLine();
        }

        private static async void onMessageHandler(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            if (message.Text != null && e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text && !string.IsNullOrEmpty(e.Message.Text)||message.Contact!=null)
            {
                try
                {
                    //Inicializing connection to database of clients
                    string connStr = "server=localhost;user=root;database=db;password=LAMPANABEARZNUMBER_5_;";
                    MySqlConnection conn = new MySqlConnection(connStr);

                    conn.Open();
                    MySqlCommand command1 = new MySqlCommand($"SELECT id FROM clients WHERE id = {message.Chat.Id}", conn);
                    var i = command1.ExecuteScalar();
                    var step = new MySqlCommand($"SELECT step FROM clients WHERE id = {message.Chat.Id}", conn).ExecuteScalar();
                    if (step is int a)
                        if (a == 5)
                            step = null;
                    //switch step on which client is
                    switch (step) {
                        case null:
                            //if client is new to the bot so we inicialize new chatid
                            if (i == null)
                            {
                                string sql1 = $"INSERT INTO clients(id) VALUES({message.Chat.Id});UPDATE clients SET step = 1 WHERE id ={message.Chat.Id};" +
                                    $"UPDATE clients SET updates = 1 WHERE id ={message.Chat.Id};";
                                MySqlCommand commandFirst = new MySqlCommand(sql1, conn);
                                await client.SendTextMessageAsync(message.Chat.Id, "Уведіть своє ім'я");
                                e.Message.Text = null;
                                var res1 = commandFirst.ExecuteScalar();
                            }
                            //if client wants to change something that bot does bot create new chatid but works with existing one
                            else
                            {
                                int updates = (int)new MySqlCommand($"SELECT updates FROM clients WHERE id={message.Chat.Id}", conn).ExecuteScalar() + 1;
                                string sql2 = $"UPDATE clients SET step = 1 WHERE id ={message.Chat.Id};" +
                                    $"UPDATE clients SET updates = {updates} WHERE id ={message.Chat.Id};";
                                MySqlCommand commandSecond = new MySqlCommand(sql2, conn);
                                await client .SendTextMessageAsync(message.Chat.Id, "Уведіть своє ім'я");
                                e.Message.Text = null;
                                var res2 = commandSecond.ExecuteScalar();
                            }
                            break;
                        case 1:
                            string name = e.Message.Text;
                            string sql3 = $"UPDATE clients SET name = '{name}' WHERE id = {message.Chat.Id};UPDATE clients SET step = 2 WHERE id={message.Chat.Id}";
                            MySqlCommand commandThird = new MySqlCommand(sql3, conn);
                            await client .SendTextMessageAsync(message.Chat.Id, "Уведіть свій email");
                            e.Message.Text = null;
                            var res3 = commandThird.ExecuteScalar();
                            break;
                        case 2:
                            string email = e.Message.Text;
                            string sql4 = $"UPDATE clients SET email = '{email}' WHERE id = {message.Chat.Id};UPDATE clients SET step = 3 WHERE id={message.Chat.Id}";
                            MySqlCommand commandFourth = new MySqlCommand(sql4, conn);
                            await client .SendTextMessageAsync(message.Chat.Id, "Уведіть своє місто");
                            e.Message.Text = null;
                            var res4 = commandFourth.ExecuteScalar();
                            break;
                        case 3:
                            string city = e.Message.Text;
                            string sql5 = $"UPDATE clients SET city = '{city}' WHERE id = {message.Chat.Id};UPDATE clients SET step = 4 WHERE id={message.Chat.Id}";
                            MySqlCommand commandFifth = new MySqlCommand(sql5, conn);
                            await client .SendTextMessageAsync(message.Chat.Id, @"Уведіть свій номер, натиснувши кнопку 'Надіслати контакт' або ввівши його у форматі +380991234567");
                            e.Message.Text = null;
                            var res5 = commandFifth.ExecuteScalar();
                            break;
                        case 4:
                            string telNumber = e.Message.Text;
                            string sql6 = null;
                            if (e.Message.Text != null)
                                 sql6 = $"UPDATE clients SET telNumber = '{telNumber}' WHERE id = {message.Chat.Id};UPDATE clients SET step = 5 WHERE id={message.Chat.Id}";
                            if (e.Message.Contact != null)
                            {
                                telNumber = e.Message.Contact.PhoneNumber;
                                sql6 = $"UPDATE clients SET telNumber = '{telNumber}' WHERE id = {message.Chat.Id};UPDATE clients SET step = 5 WHERE id={message.Chat.Id}";
                            }

                            MySqlCommand commandSixth = new MySqlCommand(sql6, conn);
                            var res6 = commandSixth.ExecuteScalar();
                            await client.SendTextMessageAsync(message.Chat.Id, "Дякуємо, уведення даних завершено. Гарного дня!");
                            if (e.Message.Text != "Так")
                            {
                                await client.SendTextMessageAsync(message.Chat.Id, "Бажаєте змінити дані?", replyMarkup: GetButtons());
                            }
                            string text = e.Message.Text;
                            switch (text)
                            {
                                case TEXT_1:
                                    await client.SendTextMessageAsync(message.Chat.Id, "Гаразд, будьте уважні!");
                                    string sql7 = $"UPDATE clients SET step = 5 WHERE id ={message.Chat.Id}";
                                    MySqlCommand commandSeventh = new MySqlCommand(sql7, conn);
                                    var res7 = commandSeventh.ExecuteScalar();
                                    break;
                            }
                            break;
                    }

                    conn.Close();
                    Console.WriteLine("Successful");
                }
                catch(Exception ex)
                {

                }
            }
          
        }

        private static IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
               {
                   new List<KeyboardButton>{new KeyboardButton {Text=TEXT_1 } }
               },
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        }
    }

}
