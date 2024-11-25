using System;

namespace SkillProfiCRM.Models
{
    public class BlogPost
    {
        // Добавляем первичный ключ
        public int Id { get; set; }

        // Остальные свойства
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublishedDate { get; set; }
    }
}
