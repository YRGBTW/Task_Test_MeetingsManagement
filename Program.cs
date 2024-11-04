using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MeetingsManagement
{
    public class Meeting
    {
        public string Description { get; set; } // Описание
        public DateTime TimeStart { get; set; } // Время начала
        public DateTime EstTimeEnd { get; set; } // Примерное время окончания
        public int Notification { get; set; }
        public string Status { get; }

        public Meeting(DateTime start, DateTime end, string desc = "", int notification = -1)
        {
            Description = desc;
            TimeStart = start;
            EstTimeEnd = end;
            Notification = notification;
            Status = UpdateStatus();
        }
        private string UpdateStatus()
        {
            DateTime now = DateTime.Now;
            if (this.EstTimeEnd < now)
            {
                return "Прошло";
            }
            else if (this.TimeStart < now && now < this.EstTimeEnd)
            {
                return "В процессе";
            }
            else
            {
                return "Запланировано";
            }
        }
    }
    public class MeetingsManagement
    {
        private List<Meeting> meetings = new List<Meeting>();
        public bool CreateMeeting(Meeting m)
        {
            if (m.TimeStart < DateTime.Now)
            {
                Console.WriteLine("Встречи можно создавать только на будущее время.");
                return false;
            }
            else
            {
                foreach (Meeting meeting in meetings)
                {
                    if (m.TimeStart < meeting.EstTimeEnd && m.EstTimeEnd > meeting.TimeStart)
                    {
                        Console.WriteLine("Встреча пересекается с сущетсвующей.");
                        return false;
                    }
                }
                if (m.Notification > 0)
                {
                    DateTime notificationTime = m.TimeStart.AddMinutes(-m.Notification);
                    if (notificationTime > DateTime.Now)
                    {
                        Timer timer = new Timer(_ =>
                        {
                            Console.WriteLine($"Напоминание: Встреча '{m.Description}' начинается в {m.TimeStart}");
                        }, null, notificationTime - DateTime.Now, Timeout.InfiniteTimeSpan);
                    }   
                }
                meetings.Add(m);
                Console.WriteLine("Встреча добавлена");
                return true;
            }
        }
        public bool EditMeeting (int idx, DateTime start, DateTime estEnd, string desc, int notification)
        {
            if (idx < 0 || idx >= meetings.Count)
            {
                Console.WriteLine("Неверный номер встречи.");
                return false;
            }
            else
            {
                meetings[idx] = new Meeting(start, estEnd, desc, notification);
                Console.WriteLine("Встреча изменена.");
                return true;
            }
        } 
        public bool DeleteMeeting(int idx)
        {
            if (idx < 0 || idx >= meetings.Count)
            {
                Console.WriteLine("Неверный номер встречи.");
                return false;
            }
            else
            {
                meetings.RemoveAt(idx);
                Console.WriteLine("Встреча удалена.");
                return true;
            }
        }

        public void ExportMeetings(DateTime day)
        {
            var dailyMeetings = meetings.Where(m => m.TimeStart.Date == day.Date).ToList();
            if (dailyMeetings.Count < 1)
            {
                Console.WriteLine("В выбранный день нет встреч для экспорта.");
                return;
            }
            using (StreamWriter writer = new StreamWriter($"Встречи_{day:yyyyMMdd}.txt"))
            {
                writer.WriteLine($"Время встречи: | Описание: | Статус:");
                foreach (var meeting in dailyMeetings)
                {
                    writer.WriteLine($"{meeting.TimeStart} - {meeting.EstTimeEnd} | {meeting.Description} | {meeting.Status}");
                }
            }
            Console.WriteLine("Встречи экспортированы в файл.");
        }

        public void ShowMeetings(DateTime day)
        {
            Console.WriteLine("Время встречи: | Описание: | Статус:");
            foreach (var meeting in meetings.Where(m => m.TimeStart.Date == day.Date))
            {
                Console.WriteLine($"{meeting.TimeStart} - {meeting.EstTimeEnd} | {meeting.Description} | {meeting.Status}");
            }
        }

    }
    internal class Program
    {
        static void Main(string[] args)
        {
            MeetingsManagement m = new MeetingsManagement();
            while (true)
            {
                Console.WriteLine("Текущий список встреч на сегодня:");
                m.ShowMeetings(DateTime.Now);
                Console.WriteLine("Выберите действие (введите число):\n1.Создать встречу\n2.Изменить встречу\n3.Удалить встречу\n4.Экспортировать список за выбранный день\n5.Выйти");
                if (!int.TryParse(Console.ReadLine(), out int choice) || (choice < 1 || choice > 5))
                {
                    Console.WriteLine("Введено неверное число.");
                }
                else
                {
                    try
                    {
                        switch (choice)
                        {
                            case 1:
                                Console.WriteLine("Введите описание:");
                                string desc = Console.ReadLine();
                                Console.WriteLine($"Введите дату и время начала в формате (ГГГГ-ММ-ДД ЧЧ:ММ):");
                                DateTime st = DateTime.Parse(Console.ReadLine());
                                Console.WriteLine($"Введите дату и время окончания в формате (ГГГГ-ММ-ДД ЧЧ:ММ):");
                                DateTime end = DateTime.Parse(Console.ReadLine());
                                Console.WriteLine("Введите время напоминания в минутах (-1 для отмены):");
                                int notification = int.Parse(Console.ReadLine());
                                m.CreateMeeting(new Meeting(st, end, desc, notification));
                                Console.Clear();
                                break;
                            case 2:
                                Console.WriteLine("Введите номер встречи для изменения: ");
                                int editIndex = int.Parse(Console.ReadLine()) - 1;
                                Console.WriteLine("Введите новое описание: ");
                                string newDesc = Console.ReadLine();
                                Console.WriteLine("Введите новую дату и время начала (ГГГГ-ММ-ДД ЧЧ:ММ): ");
                                DateTime newStart = DateTime.Parse(Console.ReadLine());
                                Console.WriteLine("Введите новую дату и время окончания (ГГГГ-ММ-ДД ЧЧ:ММ): ");
                                DateTime newEnd = DateTime.Parse(Console.ReadLine());
                                Console.WriteLine("Введите новое время напоминания (-1 для отмены): ");
                                int newNotification = int.Parse(Console.ReadLine());
                                m.EditMeeting(editIndex, newStart, newEnd, newDesc, newNotification);
                                Console.Clear();
                                break;
                            case 3:
                                Console.WriteLine("Введите номер встречи для удаления: ");
                                int deleteIndex = int.Parse(Console.ReadLine()) - 1;
                                m.DeleteMeeting(deleteIndex);
                                Console.Clear();
                                break;
                            case 4:
                                Console.WriteLine("Введите дату для экспорта (ГГГГ-ММ-ДД): ");
                                DateTime exportDay = DateTime.Parse(Console.ReadLine());
                                m.ExportMeetings(exportDay);
                                Console.Clear();
                                break;
                            case 5:
                                return;
                        }
                    }
                    catch ( Exception e)
                    {
                        Console.WriteLine("Ошибка. Попробуйте еще раз.");
                        Console.Clear();
                    }
                    
                }
            }
        }
    }
}
