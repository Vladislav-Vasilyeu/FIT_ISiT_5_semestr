using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL004
{
    public interface IRepository : IDisposable
    {
        string BasePath { get; }                            // полный дирректорий для JSON и фотографии
        Celebrity[] getAllCelebrities();                    // получить весь список знаменитостей
        Celebrity? getCelebrityById(int id);                // получить знаменитость по Id
        Celebrity[] getCelebritiesBySurname(string surname);// получить знаменитость по фамилии
        string? getPhotoPathById(int id);                   // получить путь для GET-запроса к фотографии
        int? addCelebrity(Celebrity celebrity);              // добавить знаменитость,   =Id новой знаменитости
        bool delCelebrityById(int id);                      // удалить знаменитость по Id, =true - успех
        int? updCelebrityById(int id, Celebrity celebrity); // изменить знаменитость по Id,  =Id - новый Id - успех
        int SaveChanges();                                  // сохранить изменения в Json, =количество изменений
    }
    public record Celebrity(int Id, string Firstname, string Surname, string PhotoPath);
}
