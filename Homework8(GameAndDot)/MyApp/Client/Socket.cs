// using System.Net;
// using System.Net.Sockets;
// using System.Text;

// // Создаем UDP сокет: IPv4, датаграммы, UDP протокол
// // using - автоматически освободит ресурсы после использования
// using var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

// // Сообщение для отправки
// string message = "33 66";
// // Конвертируем строку в массив байт (UTF-8 кодировка)
// byte[] data = Encoding.UTF8.GetBytes(message);
// // Создаем точку назначения: IP адрес 127.0.0.1 (localhost), порт 5555
// EndPoint remotePoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);
// // Отправляем данные асинхронно, получаем количество отправленных байт
// int bytes = await udpSocket.SendToAsync(data, remotePoint);
// Console.WriteLine($"Отправлено {bytes} байт");