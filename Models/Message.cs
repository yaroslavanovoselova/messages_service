using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace MessageSevice_peer11.Models
{
    public class Message
    {
        /// <summary>
        /// Тема сообщения.
        /// </summary>
        public string Subject { get; set; }
        
        /// <summary>
        /// Текст сообщения.
        /// </summary>
        public string MessageText { get; set; }
        
        /// <summary>
        /// Email отправителя.
        /// </summary>
        public string SenderId { get; set; }
        
        /// <summary>
        /// Email получателя.
        /// </summary>
        public string ReceiverId { get; set; }
        
        /// <summary>
        /// Создания списка случайных сообщений.
        /// </summary>
        /// <param name="users"> Список пользователей. </param>
        /// <returns> Список сообщений. </returns>
        public static List<Message> GenerateMessages(List<User> users)
        {
            var messages = new List<Message>();
            // Считываем список русских слов, чтобы составлять из них сообщение.
            var russianWords = File.ReadAllLines("russian.txt");
            var rnd = new Random();
            for (int i = 0; i < users.Count; i++)
            {
                // Количество отправленных пользователем сообщений.
                var numberOfMessages = rnd.Next(1, 5);
                for (int j = 0; j < numberOfMessages; j++)
                {
                    // Генерируем текст сообщения.
                    var message = "";
                    for (int k = 0; k < rnd.Next(5,20); k++)
                    {
                        message += russianWords[rnd.Next(russianWords.Length)]+" ";
                    }
                    messages.Add(new Message
                    {
                        Subject = russianWords[rnd.Next(russianWords.Length)],
                        MessageText = message,
                        SenderId = users[i].Email,
                        ReceiverId = users[rnd.Next(users.Count)].Email
                    });
                }
            }
            return messages;
        }
    }
}