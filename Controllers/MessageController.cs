using System;
using System.Collections.Generic;
using MessageSevice_peer11.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MessageSevice_peer11.Controllers
{
    public class MessageController : Controller
    {
        /// <summary>
        /// Список пользователей.
        /// </summary>
        private static List<User> _users = new List<User>();

        /// <summary>
        /// Список сообщений.
        /// </summary>
        private static List<Message> _messages = new List<Message>();

        /// <summary>
        /// Загрузка данных из файлов.
        /// </summary>
        static MessageController()
        {
            DeserialiseInfo();
        }

        /// <summary>
        /// Инициализация списка пользователей и сообщений.
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreateNewUsersAndMessages")]
        public IActionResult PostNewListOfUsersAndMessages()
        {
            if (_users.Count == 0 || _messages.Count == 0)
            {
                _users = Models.User.GenerateUsers();
                _messages = Message.GenerateMessages(_users);
                SerialiseInfo();
                return Ok("Список пользователей и сообщений успешно создан !");
            }
            return BadRequest(new { Message = "Ошибка! Список пользователей и сообщений можно инициализировать только один раз." });
        }

        /// <summary>
        /// Создание нового пользователя. 
        /// </summary>
        /// <param name="userName"> Имя пользователя. </param>
        /// <param name="email"> Email пользователя. </param>
        /// <returns></returns>
        [HttpPost("CreateNewUser")]
        public IActionResult PostNewUser([FromQuery] string userName, string email)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email))
                return BadRequest(new { Message = "Ошибка! Вы не заполнили все обязательные поля." });
            if (_users.Any(x => x.Email == email))
                return BadRequest(new { Message = "Пользователь с такой почтой уже сушествует." });
            _users.Add(new User { UserName = userName, Email = email });
            _users = _users.OrderBy(x => x.Email).ToList();
            SerialiseInfo();
            return Ok($"Создан пользователь {userName} с почтой {email}.");
        }

        /// <summary>
        /// Отправка сообщения.
        /// </summary>
        /// <param name="subject"> Тема сообщения. </param>
        /// <param name="messageText"> Текст сообщения. </param>
        /// <param name="senderId"> Email отправителя. </param>
        /// <param name="receiverId"> Email получателя. </param>
        /// <returns></returns>
        [HttpPost("SentNewMessage")]
        public IActionResult PostAMessage([FromQuery] string subject, string messageText, string senderId, string receiverId)
        {
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(messageText)
                                                   || string.IsNullOrWhiteSpace(senderId) || string.IsNullOrWhiteSpace(receiverId))
                return BadRequest(new { Message = "Ошибка! Все поля должны быть заполнены." });
            if (_users.All(user => user.Email != receiverId))
                return NotFound(new { Message = $"Ошибка! Пользователя с почтой {receiverId} не существует." });
            if (_users.All(user => user.Email != senderId))
                return NotFound(new { Message = $"Ошибка! Пользователя с почтой {senderId} не существует." });
            _messages.Add(new Message
            {
                Subject = subject,
                MessageText = messageText,
                SenderId = senderId,
                ReceiverId = receiverId
            });
            SerialiseInfo();
            return Ok($"Сообщение отправлено.");
        }

        /// <summary>
        /// Вывод списка пользователей.
        /// </summary>
        /// <param name="Limit"> Количество пользователей, которое необходимо вернуть. </param>
        /// <param name="Offset"> Порядковый номер пользователя, начиная с которого необходимо получать информацию. </param>
        /// <returns></returns>
        [HttpGet("GetAllUsers")]
        public IActionResult GetListOfAllUsers([FromQuery] int Limit = 1000000, int Offset = 0)
        {
            if (_users.Count == 0)
                return BadRequest(new { Message = "Ошибка! Для начала создайте пользователей." });
            if (Offset < 0 || Limit <= 0)
                return BadRequest(new { Message = "Ошибка! Limit должен быть меньше нуля, а Offset неотрицательным." });
            return Ok(_users.Skip(Offset).Take(Limit));
        }

        /// <summary>
        /// Получение информации о пользователе.
        /// </summary>
        /// <param name="email"> Email пользователя. </param>
        /// <returns></returns>
        [HttpGet("GetUserInfo/{email}")]
        public IActionResult GetUserInfo([FromRoute] string email)
        {
            if (_users.All(user => user.Email != email))
                return NotFound(new { Message = $"Ошибка! Пользователя с почтой {email} не существует." });
            return Ok(_users.First(user => user.Email == email));
        }

        /// <summary>
        /// Вывод списка всех сообщений.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAllMessages")]
        public IActionResult GetMessages()
        {
            if (_messages.Count == 0)
                return BadRequest(new { Message = "Ошибка! Для начала создайте сообщения." });
            return Ok(_messages);
        }

        /// <summary>
        /// Получение списка сообщений по ID отправителя и получателя.
        /// </summary>
        /// <param name="senderId"> Email отправителя. </param>
        /// <param name="receiverId"> Email получателя. </param>
        /// <returns></returns>
        [HttpGet("GetMessages/{senderId}/{receiverId}")]
        public IActionResult GetMessages([FromRoute] string senderId, string receiverId)
        {
            if (_users.Count == 0 || _messages.Count == 0)
                return BadRequest(new { Message = "Ошибка! Для начала создайте пользователей и сообщения." });
            if (_users.All(user => user.Email != receiverId))
                return NotFound(new { Message = $"Ошибка! Пользователя с почтой {receiverId} не существует." });
            if (_users.All(user => user.Email != senderId))
                return NotFound(new { Message = $"Ошибка! Пользователя с почтой {senderId} не существует." });
            if (_messages.Any(mes => mes.ReceiverId == receiverId && mes.SenderId == senderId))
                return Ok(_messages.Where(mes => mes.ReceiverId == receiverId && mes.SenderId == senderId));
            return Ok($"Пользователь {senderId} не отправлял сообщения {receiverId}");
        }

        /// <summary>
        /// Получение списка сообщений по ID отправителя (получатель -- любой).
        /// </summary>
        /// <param name="senderId"> Email отправителя. </param>
        /// <returns></returns>
        [HttpGet("GetMessagesForSender/{senderId}")]
        public IActionResult GetMessagesForSender([FromRoute] string senderId)
        {
            if (_users.Count == 0 || _messages.Count == 0)
                return BadRequest(new { Message = "Ошибка! Для начала создайте пользователей и сообщения." });
            if (_users.All(user => user.Email != senderId))
                return NotFound(new { Message = $"Ошибка! Пользователя с почтой {senderId} не существует." });
            if (_messages.Any(mes => mes.SenderId == senderId))
                return Ok(_messages.Where(mes => mes.SenderId == senderId).ToList());
            return Ok($"Пользователь {senderId} не отправлял сообщения никому сообщения");
        }

        /// <summary>
        /// Получение списка сообщений по ID получателя (отправитель -- любой).
        /// </summary>
        /// <param name="receiverId"> Email получателя. </param>
        /// <returns></returns>
        [HttpGet("GetMessagesForReceiver/{receiverId}")]
        public IActionResult GetMessagesForReceiver([FromRoute] string receiverId)
        {
            if (_users.Count == 0 || _messages.Count == 0)
                return BadRequest(new { Message = "Ошибка! Для начала создайте пользователей и сообщения." });
            if (_users.All(user => user.Email != receiverId))
                return NotFound(new { Message = $"Ошибка! Пользователя с почтой {receiverId} не существует." });
            if (_messages.Any(mes => mes.ReceiverId == receiverId))
                return Ok(_messages.Where(mes => mes.ReceiverId == receiverId).ToList());
            return Ok($"Пользователь {receiverId} не получал ни от кого собщения.");
        }


        /// <summary>
        /// Сериализация данных.
        /// </summary>
        static void SerialiseInfo()
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All };

            var usersJson = JsonConvert.SerializeObject(_users, settings);
            System.IO.File.WriteAllText("users.json", usersJson);

            var messageJson = JsonConvert.SerializeObject(_messages, settings);
            System.IO.File.WriteAllText("messages.json", messageJson);
        }

        /// <summary>
        /// Десериализация данных.
        /// </summary>
        static void DeserialiseInfo()
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All };

            try
            {
                if (System.IO.File.Exists("users.json"))
                    _users = JsonConvert.DeserializeObject<List<User>>(System.IO.File.ReadAllText("users.json"),
                        settings);
                else
                    _users = new List<User>();
                if (System.IO.File.Exists("messages.json"))
                    _messages = JsonConvert.DeserializeObject<List<Message>>(System.IO.File.ReadAllText("messages.json"),
                        settings);
                else
                    _messages = new List<Message>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _messages = new List<Message>();
                _users = new List<User>();
            }
        }
    }
}