using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace MessageSevice_peer11.Models
{
    public class User
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Email пользователя.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Создания списка случайных пользователей.
        /// </summary>
        /// <returns> Список пользователей. </returns>
        public static List<User> GenerateUsers()
        {
            var users = new List<User>();
            // Считываем список английских слов, чтобы составлять из них email.
            var englishWords = File.ReadAllLines("english.txt");
            // Считываем список имен.
            var names = File.ReadAllLines("names.txt");
            var rnd = new Random();
            // Создаем 100 пользователей.
            while (users.Count < 100)
            {
                var email = englishWords[rnd.Next(0, englishWords.Length)]+"@gmail.com";
                if (users.All(user => user.Email != email))
                    users.Add(new User{UserName = names[rnd.Next(0, names.Length)], Email = email});
            }
            return users.OrderBy(x=>x.Email).ToList();
        }
    }
}