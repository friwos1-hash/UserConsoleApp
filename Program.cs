using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UserConsoleApp
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя обязательно для заполнения")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Имя должно быть от 2 до 50 символов")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email обязателен для заполнения")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Пароль обязателен для заполнения")]
        [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Пароль должен содержать хотя бы одну заглавную букву, одну строчную и одну цифру")]
        public string Password { get; set; }

        [Range(18, 120, ErrorMessage = "Возраст должен быть от 18 до 120 лет")]
        public int Age { get; set; }

        // Метод для валидации пользователя
        public List<string> Validate()
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(this);

            bool isValid = Validator.TryValidateObject(this, validationContext, validationResults, true);

            var errors = new List<string>();
            if (!isValid)
            {
                foreach (var validationResult in validationResults)
                {
                    errors.Add(validationResult.ErrorMessage);
                }
            }

            return errors;
        }
    }

    // Менеджер для работы с пользователями
    public class UserManager
    {
        private List<User> _users = new List<User>();
        private int _nextId = 1;

        // Метод добавления пользователя с try-catch
        public void AddUserWithTryCatch(User user)
        {
            try
            {
                Console.WriteLine($"\nПопытка добавления пользователя: {user.Name}");

                // 1. Валидация данных
                var validationErrors = user.Validate();
                if (validationErrors.Count > 0)
                {
                    Console.WriteLine("Ошибки валидации:");
                    foreach (var error in validationErrors)
                    {
                        Console.WriteLine($"  - {error}");
                    }
                    throw new ValidationException("Данные пользователя не прошли валидацию");
                }

                // 2. Проверка уникальности email (имитация бизнес-правила)
                if (_users.Any(u => u.Email == user.Email))
                {
                    throw new ArgumentException($"Пользователь с email {user.Email} уже существует");
                }

                // 3. Устанавливаем ID и добавляем в список
                user.Id = _nextId++;
                _users.Add(user);

                Console.WriteLine($"✅ Пользователь {user.Name} успешно добавлен с ID: {user.Id}");

                // 4. Имитация возможной ошибки при сохранении в БД
                SimulateDatabaseOperation();

            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"❌ Ошибка валидации: {ex.Message}");
                LogError($"ValidationError: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"❌ Ошибка аргумента: {ex.Message}");
                LogError($"ArgumentError: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"❌ Ошибка операции: {ex.Message}");
                LogError($"OperationError: {ex.Message}");
            }
            catch (Exception ex) // Общий catch для всех остальных исключений
            {
                Console.WriteLine($"❌ Непредвиденная ошибка: {ex.Message}");
                LogError($"UnexpectedError: {ex.GetType().Name} - {ex.Message}");
                throw; // Пробрасываем исключение дальше, если нужно
            }
            finally
            {
                // Этот блок выполнится ВСЕГДА, даже если было исключение
                Console.WriteLine("Операция добавления пользователя завершена (finally блок)");
            }
        }

        // Имитация операции с базой данных (может выбросить исключение)
        private void SimulateDatabaseOperation()
        {
            // Симулируем случайную ошибку (шанс 30%)
            Random random = new Random();
            if (random.Next(1, 100) <= 30)
            {
                throw new InvalidOperationException("Ошибка при сохранении в базу данных. Соединение потеряно.");
            }
        }

        // Метод для логирования ошибок
        private void LogError(string errorMessage)
        {
            string logFile = "user_errors.log";
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {errorMessage}\n";

            try
            {
                File.AppendAllText(logFile, logEntry);
                Console.WriteLine($"📝 Ошибка записана в лог-файл: {logFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Не удалось записать в лог: {ex.Message}");
            }
        }

        // Метод для вывода всех пользователей
        public void DisplayAllUsers()
        {
            Console.WriteLine("\n=== СПИСОК ВСЕХ ПОЛЬЗОВАТЕЛЕЙ ===");
            if (_users.Count == 0)
            {
                Console.WriteLine("Пользователей нет");
            }
            else
            {
                foreach (var user in _users)
                {
                    Console.WriteLine($"ID: {user.Id}, Имя: {user.Name}, Email: {user.Email}, Возраст: {user.Age}");
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== КОНСОЛЬНОЕ ПРИЛОЖЕНИЕ ДЛЯ РАБОТЫ С ПОЛЬЗОВАТЕЛЯМИ ===");

            var userManager = new UserManager();

            // Тестовые пользователи
            var usersToAdd = new List<User>
            {
                // Правильный пользователь
                new User
                {
                    Name = "Иван Иванов",
                    Email = "ivan@example.com",
                    Password = "Password123",
                    Age = 25
                },
                
                // Пользователь с неправильным email
                new User
                {
                    Name = "Петр Петров",
                    Email = "неправильный-email",
                    Password = "pass",
                    Age = 17
                },
                
                // Пользователь с коротким паролем
                new User
                {
                    Name = "А",
                    Email = "short@example.com",
                    Password = "123",
                    Age = 150
                },
                
                // Еще один правильный пользователь
                new User
                {
                    Name = "Мария Сидорова",
                    Email = "maria@example.com",
                    Password = "SecurePass456",
                    Age = 30
                },
                
                // Пользователь с таким же email (для проверки дубликата)
                new User
                {
                    Name = "Дубликат",
                    Email = "ivan@example.com",
                    Password = "AnotherPass789",
                    Age = 35
                }
            };

            // Добавляем пользователей с обработкой ошибок
            foreach (var user in usersToAdd)
            {
                userManager.AddUserWithTryCatch(user);
            }

            // Показываем всех добавленных пользователей
            userManager.DisplayAllUsers();

            // Пример ручного ввода пользователя
            Console.WriteLine("\n=== РУЧНОЙ ВВОД ПОЛЬЗОВАТЕЛЯ ===");

            try
            {
                Console.Write("Введите имя: ");
                string name = Console.ReadLine();

                Console.Write("Введите email: ");
                string email = Console.ReadLine();

                Console.Write("Введите пароль: ");
                string password = Console.ReadLine();

                Console.Write("Введите возраст: ");
                int age = int.Parse(Console.ReadLine());

                var manualUser = new User
                {
                    Name = name,
                    Email = email,
                    Password = password,
                    Age = age
                };

                userManager.AddUserWithTryCatch(manualUser);
            }
            catch (FormatException)
            {
                Console.WriteLine("❌ Ошибка: Возраст должен быть числом!");
            }

            Console.WriteLine("\n=== ПРИЛОЖЕНИЕ ЗАВЕРШИЛО РАБОТУ ===");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}